using System.Linq;
using System.Threading.Tasks;
using git_webhook_server.Repositories;
using Microsoft.AspNetCore.Mvc;
using shared;

namespace git_webhook_server.Controllers
{
    [Route("api/[controller]")]
    public class EventLogController : Controller
    {
        private readonly IEventLogRepository _eventLogRepository;

        public EventLogController(IEventLogRepository eventLogRepository)
        {
            _eventLogRepository = eventLogRepository;
        }
        
        public async Task<IActionResult> Get()
        {
            var itemsFromDb = await _eventLogRepository.Get(20);
            
            return Ok(itemsFromDb.Select(item => new EventLogItem
            {
                Id = item.Id,
                Headers = item.Headers,
                Payload = item.Payload,
                Succeeded = item.Succeeded,
                CommandError = item.ExecutionResult?.Error,
                CommandOutput = item.ExecutionResult?.Output,
                ExitCode = item.ExecutionResult?.ExitCode,
                StatusMessage = item.StatusMessage,
                EventReceivedOn = item.EventReceivedOn,
                WebHookRuleCommand = item.MatchedRule?.Execute,
                WebHookRuleName = item.MatchedRule?.Name
            }));
        }
    }
}
