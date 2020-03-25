using System;
using git_webhook_server.PayloadModels;
using Microsoft.Extensions.Logging;

namespace git_webhook_server.Services
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

        public bool Process(PushEventPayload payload)
        {
            try
            {
                var rule = _ruleMatcher.Match(payload.Ref, payload.Repository?.Url);
                if (rule != null)
                {
                    _processExecutor.Execute(rule.Execute);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error while trying to execute script");
                return false;
            }

            return false;
        }
    }
}