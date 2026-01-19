using Microsoft.AspNetCore.Mvc;

namespace Extraordinary.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UploadController : Controller
    {
        [HttpGet]
        public IActionResult GetStatusAsync()
        {
            return Ok("Upload service is running.");
        }
    }
}
