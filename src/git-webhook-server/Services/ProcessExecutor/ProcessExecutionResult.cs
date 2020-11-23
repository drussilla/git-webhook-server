namespace git_webhook_server.Services.ProcessExecutor
{
    public record ProcessExecutionResult(int ExitCode, string Output, string Error);
}
