using System.Collections.Generic;

namespace git_webhook_server
{
    public class WebHookOptions
    {
        public List<WebHookRule> Rules { get; set; }
    }
}