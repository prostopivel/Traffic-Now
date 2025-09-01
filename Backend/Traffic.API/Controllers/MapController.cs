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

        public MapController(IMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMap([FromBody] string name)
        {
            (var map, var Error) = Map.Create(
                Guid.NewGuid(),
                name,
                []);
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

            var points = new List<PointResponse>(map.Points.Count);
            foreach (var point in map?.Points ?? [])
            {
                points.Add(new PointResponse(
                    point.Id,
                    point.MapId,
                    point.X,
                    point.Y,
                    point.Name,
                    point.ConnectedPointsIds));
            }

            var mapResponse = new MapResponse(
                map!.Id,
                map.Name,
                points);

            return Ok(mapResponse);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> SearchMap([FromQuery] string name)
        {
            var maps = await _mapService.SearchMap(name);

            var mapsResponse = new List<MapResponse>(maps?.Count ?? 0);
            foreach (var map in maps ?? [])
            {
                var points = new List<PointResponse>(map.Points.Count);
                foreach (var point in map?.Points ?? [])
                {
                    points.Add(new PointResponse(
                        point.Id,
                        point.MapId,
                        point.X,
                        point.Y,
                        point.Name,
                        point.ConnectedPointsIds));
                }

                mapsResponse.Add(new MapResponse(
                map!.Id,
                map.Name,
                points));
            }

            return Ok(mapsResponse);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMap([FromBody] MapRequest request)
        {
            (var map, var Error) = Map.Create(
                request.Id,
                request.Name,
                []);

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
                return Unauthorized("Email not found in token");
            }

            var maps = await _mapService.GetUserMaps(id);

            var mapsResponse = new List<MapResponse>(maps?.Count ?? 0);
            foreach (var map in maps ?? [])
            {
                var points = new List<PointResponse>(map.Points.Count);
                foreach (var point in map?.Points ?? [])
                {
                    points.Add(new PointResponse(
                        point.Id,
                        point.MapId,
                        point.X,
                        point.Y,
                        point.Name,
                        point.ConnectedPointsIds));
                }

                mapsResponse.Add(new MapResponse(
                map!.Id,
                map.Name,
                points));
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
                return Unauthorized("Email not found in token");
            }

            var maps = await _mapService.GetUserMaps(id);

            var resId = await _mapService.AddUserMap(id, mapId);

            return Ok(resId);
        }
    }
}
