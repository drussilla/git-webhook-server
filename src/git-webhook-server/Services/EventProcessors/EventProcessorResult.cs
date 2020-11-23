namespace git_webhook_server.Services.EventProcessors
{
    public sealed record EventProcessorResult(bool Success, string Message, WebHookRule MatchedRule = null);
}