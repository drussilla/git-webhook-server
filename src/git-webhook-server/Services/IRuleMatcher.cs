namespace git_webhook_server.Services
{
    public interface IRuleMatcher
    {
        WebHookRule Match(string @ref, string repositoryUrl);
    }
}