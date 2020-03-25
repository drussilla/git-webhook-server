using git_webhook_server.PayloadModels;

namespace git_webhook_server.Services
{
    public interface IPushEventProcessor
    {
        public bool Process(PushEventPayload payload);
    }
}