using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Traffic.API.Contracts;
using Traffic.Application.Services;
using Traffic.Core.Abstractions.Services;
using Traffic.Core.Models;

namespace Traffic.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly ILogger<TransportController> _logger;
        private readonly ITransportHttpConnection _transportHttpConnection;
        private readonly ITransportService _transportService;
        private readonly IPointService _pointService;
        private readonly ITransportDataService _transportDataService;

        public TransportController(ILogger<TransportController> logger,
            ITransportHttpConnection transportHttpConnection, ITransportService transportService,
            IPointService pointService, ITransportDataService transportDataService)
        {
            _logger = logger;
            _transportHttpConnection = transportHttpConnection;
            _transportService = transportService;
            _pointService = pointService;
            _transportDataService = transportDataService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetTransport([FromQuery] Guid transportId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var transport = await _transportService.GetAsync(transportId);

            if (transport == null || transport.Point == null)
            {
                return NotFound(new { message = "Транспорт не найден!" });
            }

            _transportDataService.UserTransport.TryGetValue(id, out var trIds);
            transport.IsActive = trIds?.Contains(transportId) ?? false;
            var response = ContractsFactory.CreateTransportResponse(transport);

            return Ok(response);
        }

        [HttpPost("connect")]
        [Authorize]
        public async Task<IActionResult> ConnectTransport([FromQuery] string url)
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
            (var transport, var Error) = ContractsFactory.CreateTransport(transportResponse, id, url);

            if (!string.IsNullOrEmpty(Error) || transport == null)
            {
                return BadRequest(new { message = Error });
            }

            var transportId = await _transportService.CreateAsync(transport) ?? Guid.Empty;
            _logger.LogInformation("Transport '{content}' created successfully!", id);

            await _transportDataService.AddUser(id, transportId);

            return Ok(transportId);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteTransport([FromQuery] Guid transportId)
        {
            var id = await _transportService.DeleteAsync(transportId);

            await _transportDataService.RemoveTransport(transportId);

            return Ok(id);
        }

        [HttpDelete("deleteUser")]
        [Authorize]
        public async Task<IActionResult> DeleteUserTransport([FromQuery] Guid transportId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var result = await _transportService.DeleteTransportUserAsync(transportId, id);

            await _transportDataService.RemoveTransportUser(id, transportId);

            return Ok(result);
        }

        [HttpGet("getTransport")]
        [Authorize]
        public async Task<IActionResult> GetUserTransport()
        {
            return await GetUserTransportAsync();
        }

        [HttpGet("getMapTransport")]
        [Authorize]
        public async Task<IActionResult> GetUserMapTransport([FromQuery] Guid mapId)
        {
            return await GetUserTransportAsync(mapId);
        }

        [HttpPost("getData")]
        public async Task<IActionResult> GetTransportData([FromBody] PointTransportResponse data)
        {
            //Console.WriteLine($"{data.TransportId} - {data?.X}:{data?.Y}");

            var (transportData, Error) = TransportData.Create(
                data.X,
                data.Y);

            if (!string.IsNullOrEmpty(Error) || transportData == null)
            {
                return BadRequest(new { message = Error });
            }

            await _transportDataService.AddTransport(data.TransportId, transportData);

            return Ok(data.TransportId);
        }

        [HttpPut("setGarage")]
        public async Task<IActionResult> SetTransportGarage([FromQuery] Guid transportId, Guid pointId)
        {
            var transport = await _transportService.GetAsync(transportId);

            if (transport == null)
            {
                return NotFound(new { message = $"Транспорт с Id '{transportId}' не найден!" });
            }

            transport.PointId = pointId;
            await _transportService.UpdateAsync(transport);

            return Ok(transportId);
        }

        private async Task<IActionResult> GetUserTransportAsync(Guid? mapId = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var transport = mapId == null
                ? await _transportService.GetUserTransportAsync(id)
                : await _transportService.GetUserTransportAsync((Guid)mapId, id);

            var result = new List<TransportResponse>(transport?.Count ?? 0);

            foreach (var item in transport ?? [])
            {
                var point = await _pointService.GetAsync(item.PointId);
                if (point == null)
                {
                    return NotFound($"Точка '{item.PointId}' не найдена!");
                }

                item.Point = point;
                _transportDataService.UserTransport.TryGetValue(id, out var trIds);
                item.IsActive = trIds?.Contains(item.Id) ?? false;
                result.Add(ContractsFactory.CreateTransportResponse(item));
            }

            return Ok(result);
        }
    }
}
