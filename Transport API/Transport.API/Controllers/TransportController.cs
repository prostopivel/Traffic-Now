using Microsoft.AspNetCore.Mvc;
using Transport.Core.Abstractions;

namespace Transport.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransportController : ControllerBase
    {
        private readonly IDataService _dataService;

        public TransportController(IDataService dataService)
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
