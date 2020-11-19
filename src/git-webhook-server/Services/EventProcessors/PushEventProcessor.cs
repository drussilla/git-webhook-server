using System;
using System.Threading.Tasks;
using git_webhook_server.PayloadModels;
using git_webhook_server.Services.ProcessExecutor;
using Microsoft.Extensions.Logging;

namespace git_webhook_server.Services.EventProcessors
{
    public class PushEventProcessor : IPushEventProcessor
    {
        private readonly IProcessExecutor _processExecutor;
        private readonly IRuleMatcher _ruleMatcher;
        private readonly ILogger<PushEventProcessor> _log;
        

        public PushEventProcessor(IProcessExecutor processExecutor, IRuleMatcher ruleMatcher, ILogger<PushEventProcessor> log)
        {
            _processExecutor = processExecutor;
            _ruleMatcher = ruleMatcher;
            _log = log;
        }

        public async Task<EventProcessorResult> Process(PushEventPayload payload)
        {
            try
            {
                var rule = _ruleMatcher.Match(payload.Ref, payload.Repository?.Url);
                if (rule != null)
                {
                    var executionResult = await _processExecutor.Execute(rule.Execute);
                    return new EventProcessorResult(true, "Rule matched", executionResult, rule);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error while trying to execute script");
                return new EventProcessorResult(false, $"Unexpected error while trying to execute script: {ex.Message}");
            }

            return new EventProcessorResult(false, "No matching rule");
        }
    }
}