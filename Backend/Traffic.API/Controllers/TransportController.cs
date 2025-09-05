using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Traffic.API.Contracts;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransportController : ControllerBase
    {
        private readonly ILogger<TransportController> _logger;
        private readonly ITransportHttpConnection _transportHttpConnection;
        private readonly ITransportAPIConnection _transportAPIConnection;
        private readonly ITransportService _transportService;
        private readonly IPointService _pointService;

        public TransportController(ILogger<TransportController> logger,
            ITransportHttpConnection transportHttpConnection, ITransportService transportService,
            ITransportAPIConnection transportAPIConnection, IPointService pointService)
        {
            _logger = logger;
            _transportHttpConnection = transportHttpConnection;
            _transportService = transportService;
            _transportAPIConnection = transportAPIConnection;
            _pointService = pointService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTransport(Guid transportId)
        {
            var transport = await _transportService.GetAsync(transportId);

            if (transport == null || transport.Point == null)
            {
                return NotFound(new { message = "Транспорт не найден!" });
            }

            transport.IsActive = _transportAPIConnection.IsTransportActive(transport.Id.ToString());
            var response = ContractsFactory.CreateTransportResponse(transport);

            return Ok(response);
        }

        [HttpPost("connect")]
        [Authorize]
        public async Task<IActionResult> ConnectTransport(string url)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var transportResponse = await _transportHttpConnection.GetTransport(url);

            if (transportResponse == null)
            {
                return BadRequest(new { message = $"Не удаётся подключиться к {url}!" });
            }

            _logger.LogInformation("Transport '{content}' reseived successfully!", transportResponse.Id);
            var finalUrl = _transportAPIConnection.GetConnectionUrl(url);
            (var transport, var Error) = ContractsFactory.CreateTransport(transportResponse, id, finalUrl);

            if (!string.IsNullOrEmpty(Error) || transport == null)
            {
                return BadRequest(new { message = Error });
            }

            id = await _transportService.CreateAsync(transport) ?? Guid.Empty;
            _logger.LogInformation("Transport '{content}' created successfully!", id);

            var isConnected = await ConnectToUrl(id.ToString(), finalUrl);

            if (!isConnected)
            {
                return BadRequest(new { message = $"Не удаётся подключиться к {finalUrl}!" });
            }

            return Ok(id);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteTransport(Guid transportId)
        {
            var id = await _transportService.DeleteAsync(transportId);

            var isConnected = await _transportAPIConnection.RemoveConnectionAsync(transportId.ToString());

            if (!isConnected)
            {
                return BadRequest(new { message = $"Не удаётся найти подключение '{transportId}'!" });
            }

            return Ok(id);
        }

        [HttpGet("getTransport")]
        [Authorize]
        public async Task<IActionResult> GetUserTransport()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var transport = await _transportService.GetUserTransportAsync(id);

            var result = new List<TransportResponse>(transport?.Count ?? 0);

            foreach (var item in transport ?? [])
            {
                var point = await _pointService.GetAsync(item.PointId);
                if (point == null)
                {
                    return NotFound($"Точка '{item.PointId}' не найдена!");
                }

                item.Point = point;
                item.IsActive = _transportAPIConnection.IsTransportActive(item.Id.ToString());
                item.IsActive = await ConnectToUrl(item.Id.ToString(), item.Url);

                result.Add(ContractsFactory.CreateTransportResponse(item));

            }

            return Ok(result);
        }

        private async Task<bool> ConnectToUrl(string id, string url)
        {
            var connectUrl = await _transportAPIConnection.AddConnectionAsync(id, url);
            if (connectUrl == null)
            {
                return false;
            }

            _logger.LogInformation("Transport '{content}' connected successfully to {url}!", id, url);
            return true;
        }
    }
}
