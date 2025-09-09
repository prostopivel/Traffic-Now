using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Traffic.API.Contracts;
using Traffic.Core.Abstractions.Services;

namespace Traffic.API.Controllers
{
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RouteController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateRoute([FromBody] RouteRequest routeRequest)
        {
            (var route, var Error) = Core.Models.Route.Create(
                Guid.NewGuid(),
                routeRequest.TransportId,
                DateTime.Now);

            if (string.IsNullOrEmpty(Error) || route == null)
            {
                return BadRequest(new { message = Error });
            }

            var id = await _routeService.CreateAsync(route);

            return Ok(id);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRoute([FromQuery] Guid routeId)
        {
            var route = await _routeService.GetRoutePointsAsync(routeId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            if (route == null || !route.Transport.UsersId.Contains(id))
            {
                return Unauthorized(new { message = $"У вас нет доступа к маршруту с идентификатором '{routeId}'" });
            }

            var routeResponse = ContractsFactory.CreateRouteResponse(route);

            return Ok(routeResponse);
        }

        [HttpGet("getRoutes")]
        [Authorize]
        public async Task<IActionResult> GetUserRoutes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var routes = await _routeService.GetUserRoutesAsync(id);

            var routesResponse = new List<Contracts.RouteResponse>(routes?.Count ?? 0);
            foreach (var route in routes ?? [])
            {
                routesResponse.Add(ContractsFactory.CreateRouteResponse(route));
            }

            return Ok(routesResponse);
        }
    }
}
