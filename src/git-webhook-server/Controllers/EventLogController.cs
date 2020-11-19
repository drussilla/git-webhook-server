using Microsoft.AspNetCore.Mvc;

namespace git_webhook_server.Controllers
{
    [Route("api/[controller]")]
    public class EventLogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
