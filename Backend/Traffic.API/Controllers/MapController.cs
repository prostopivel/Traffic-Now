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
    [Authorize]
    public class MapController : ControllerBase
    {
        private readonly IMapService _mapService;
        private readonly IMapSerializeService _mapSerializeService;

        public MapController(IMapService mapService, IMapSerializeService mapSerializeService)
        {
            _mapService = mapService;
            _mapSerializeService = mapSerializeService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMap([FromBody] MapRequestCreate mapRequest)
        {
            (var map, var Error) = ContractsFactory.CreateMap(mapRequest);
            if (!string.IsNullOrEmpty(Error) || map == null)
            {
                return BadRequest(new { message = Error });
            }

            var id = await _mapService.CreateAsync(map);

            (var points, Error) = await GenerateMapService.GenerateMap(map);
            if (!string.IsNullOrEmpty(Error) || points == null)
            {
                return BadRequest(new { message = Error });
            }

            Error = (await _mapService.CreateMapPointsAsync(points)).Error;
            if (!string.IsNullOrEmpty(Error))
            {
                return BadRequest(new { message = Error });
            }

            return Ok(id);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMap([FromQuery] Guid id)
        {
            var map = await _mapService.GetMapPointsAsync(id);

            if (map == null || map.Points == null)
            {
                return NotFound(new { message = "Карта не найдена!" });
            }

            var mapResponse = ContractsFactory.CreateMapResponse(map);

            return Ok(mapResponse);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchMap([FromQuery] string name)
        {
            var maps = await _mapService.SearchMap(name);

            var mapsResponse = new List<Contracts.MapResponse>(maps?.Count ?? 0);
            foreach (var map in maps ?? [])
            {
                mapsResponse.Add(ContractsFactory.CreateMapResponse(map));
            }

            return Ok(mapsResponse);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMap([FromBody] MapRequest request)
        {
            (var map, var Error) = ContractsFactory.CreateMap(request);

            if (string.IsNullOrEmpty(Error) || map == null)
            {
                return BadRequest(new { message = Error });
            }

            var id = await _mapService.UpdateAsync(map);

            return Ok(id);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMap([FromQuery] Guid id)
        {
            id = (Guid)await _mapService.DeleteAsync(id);

            return Ok(id);
        }

        [HttpGet("getMaps")]
        [Authorize]
        public async Task<IActionResult> GetUserMaps()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var maps = await _mapService.GetUserMaps(id);

            var mapsResponse = new List<Contracts.MapResponse>(maps?.Count ?? 0);
            foreach (var map in maps ?? [])
            {
                mapsResponse.Add(ContractsFactory.CreateMapResponse(map));
            }

            return Ok(mapsResponse);
        }

        [HttpPost("addUserMap")]
        [Authorize]
        public async Task<IActionResult> AddUserMap([FromQuery] Guid mapId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var resId = await _mapService.AddUserMap(id, mapId);

            return Ok(resId);
        }

        [HttpDelete("deleteMap")]
        [Authorize]
        public async Task<IActionResult> DeleteUserMap([FromQuery] Guid mapId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { message = "Идентификатор не найден в токене" });
            }

            var resId = await _mapService.DeleteUserMap(id, mapId);

            return Ok(resId);
        }


        [HttpPost("saveMap")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveMapToJsonAsync([FromQuery] string path, [FromQuery] Guid mapId)
        {
            var id = await _mapSerializeService.CreateMapJson(path, mapId);

            return Ok(id);
        }

        [HttpGet("exportMap")]
        [Authorize]
        public async Task<IActionResult> GetJsonMapAsync([FromQuery] Guid mapId)
        {
            var json = await _mapSerializeService.ExportMapJson(mapId);

            return Ok(json);
        }
    }
}
