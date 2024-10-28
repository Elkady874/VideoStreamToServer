using Microsoft.AspNetCore.Mvc;

namespace VideoStreamToServer.Controllers
{
    public class TusController : Controller
    {
        [HttpPost("files/test")]
        public IActionResult test()
        {
            return Created();
        }

    }
}
