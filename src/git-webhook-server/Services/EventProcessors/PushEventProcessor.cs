using System;
using System.Threading;
using System.Threading.Tasks;
using git_webhook_server.PayloadModels;
using git_webhook_server.Repositories;
using Microsoft.Extensions.Logging;

namespace git_webhook_server.Services.EventProcessors
{
    public class PushEventProcessor : IPushEventProcessor
    {
        private readonly IExecutionRequestRepository _executionRequestRepository;
        private readonly IRuleMatcher _ruleMatcher;
        private readonly ILogger<PushEventProcessor> _log;
        

        public PushEventProcessor(IExecutionRequestRepository executionRequestRepository, IRuleMatcher ruleMatcher, ILogger<PushEventProcessor> log)
        {
            _executionRequestRepository = executionRequestRepository;
            _ruleMatcher = ruleMatcher;
            _log = log;
        }

        public async Task<EventProcessorResult> Process(Guid eventLog, PushEventPayload payload, CancellationToken token)
        {
            try
            {
                var rule = _ruleMatcher.Match(payload.Ref, payload.Repository?.Url);
                if (rule != null)
                {
                    await _executionRequestRepository.Create(rule, eventLog, token);
                    return new EventProcessorResult(true, $"{rule.Name} rule matched", rule);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error while trying to execute script");
                return new EventProcessorResult(false, $"Unexpected error while trying to process event: {ex.Message}");
            }

            return new EventProcessorResult(false, "No matching rule");
        }
    }
}