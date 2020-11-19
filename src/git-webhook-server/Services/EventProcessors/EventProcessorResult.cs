using git_webhook_server.Services.ProcessExecutor;

namespace git_webhook_server.Services.EventProcessors
{
    public record EventProcessorResult(bool Success, string Message, ProcessExecutionResult ExecutionResult = null, WebHookRule MatchedRule = null);
}