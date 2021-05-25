using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using git_webhook_server.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using shared;

namespace git_webhook_server.Controllers
{
    [Route("api/[controller]")]
    public class RuleController : Controller
    {
        private readonly IOptions<WebHookOptions> _options;
        private readonly IExecutionRequestRepository _executionRequestRepository;
        private readonly IEventLogRepository _eventLogRepository;
        private readonly ILogger<RuleController> _log;

        public RuleController(
            IOptions<WebHookOptions> options, 
            IExecutionRequestRepository executionRequestRepository, 
            IEventLogRepository eventLogRepository,
            ILogger<RuleController> log)
        {
            _options = options;
            _executionRequestRepository = executionRequestRepository;
            _eventLogRepository = eventLogRepository;
            _log = log;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_options.Value.Rules.Select(x => new RuleItem
            {
                Name = x.Name,
                Execute = x.Execute,
                Ref = x.Ref,
                RepositoryUrl = x.RepositoryUrl
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Trigger([FromForm]string ruleName, CancellationToken token)
        {
            var rule = _options.Value.Rules.FirstOrDefault(x =>
                x.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));


            if (rule == null)
            {
                _log.LogWarning($"Tried to trigger {ruleName} rule but could not find it in the configuration");
                return NotFound();
            }

            var eventLog = await _eventLogRepository.CreateManualAsync(token);
            await _executionRequestRepository.Create(rule, eventLog.Id, token);

            return Ok();
        }
    }
}