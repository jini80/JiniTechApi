using Microsoft.AspNetCore.Mvc;

namespace JiniTechApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("✅ JiniTech API is running successfully on Render!");
        }
    }
}
