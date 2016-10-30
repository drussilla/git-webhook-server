using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace git_webhook_server.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : Controller
    {
        // GET api/webhook
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] dynamic data)
        {
            return Ok();
        }
    }
}
