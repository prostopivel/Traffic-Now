using Microsoft.AspNetCore.Mvc;
using Transport.Application.Services;
using Transport.Core.Abstractions;

namespace Transport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly IDataService _dataService;
        private readonly IRouteService _routeService;

        public TransportController(IDataService dataService, IRouteService routeService)
        {
            _dataService = dataService;
            _routeService = routeService;
        }

        [HttpGet]
        public IActionResult GetTransport()
        {
            return Ok(_dataService.Transport);
        }

        [HttpGet("getSpeed")]
        public IActionResult GetTransportSpeed()
        {
            return Ok(_routeService.Speed);
        }
    }
}
