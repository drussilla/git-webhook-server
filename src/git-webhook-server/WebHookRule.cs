using System;

namespace git_webhook_server
{
    public class WebHookRule
    {
        public string Name { get; set; }

        [Obsolete("Please use Ref property, this field will be remove in future releases")]
        public string Match { get; set; }
        public string Ref { get; set; }
        public string RepositoryUrl { get; set; }
        public string Execute { get; set; }
    }
}