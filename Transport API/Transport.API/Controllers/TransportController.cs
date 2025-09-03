using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transport.Application.Services;

namespace Transport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly DataService _dataService;

        public TransportController(DataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public IActionResult GetTransport()
        {
            return Ok(_dataService.Transport);
        }
    }
}
