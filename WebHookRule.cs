namespace git_webhook_server
{
    public class WebHookRule
    {
        public string Name { get; set; }
        public string Match { get; set; }
        public string Execute { get; set; }
    }
}