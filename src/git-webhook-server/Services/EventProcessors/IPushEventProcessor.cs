using System;
using System.Threading;
using System.Threading.Tasks;
using git_webhook_server.PayloadModels;

namespace git_webhook_server.Services.EventProcessors
{
    public interface IPushEventProcessor
    {
        public Task<EventProcessorResult> Process(Guid eventLog, PushEventPayload payload, CancellationToken token);
    }
}