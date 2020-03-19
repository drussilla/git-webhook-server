using System;
using git_webhook_server.PayloadModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace git_webhook_server.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : Controller
    {
        private readonly IOptions<WebHookOptions> _options;
        private readonly ILogger<WebhookController> _log;

        public WebhookController(IOptions<WebHookOptions> options, ILogger<WebhookController> log)
        {
            _options = options;
            _log = log;
        }

        // GET api/webhook
        [HttpPost]
        public IActionResult Post([FromBody] PushEventPayload payload)
        {
            if (payload == null)
            {
                return Ok();
            }

            try
            {
                _log.LogDebug($"Input data: {payload}");
                foreach (var rule in _options.Value.Rules)
                {
                    _log.LogDebug($"Try rule {rule.Name}");
                    if (payload.Ref.Equals(rule.Match, StringComparison.OrdinalIgnoreCase))
                    {
                        _log.LogInformation($"Rule {rule.Name} matches by {rule.Match}.");
                        _log.LogInformation($"Start {rule.Execute}");
                        System.Diagnostics.Process.Start(rule.Execute);
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error");
                return Ok();
            }

            _log.LogInformation("No matching rule found");
            return Ok();
        }
    }
}
