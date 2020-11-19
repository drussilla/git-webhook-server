using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace git_webhook_server.PayloadModels
{
    public class PushEventPayload
    {
        public string Ref { get; set; }
        public Repository Repository { get; set; }
    }

    public class Repository
    {
        public long Id { get; set; }
        public string Name { get; set; }
        [JsonPropertyName("full_name")]
        public string FullName { get; set; }
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }
        public string Url { get; set; }
    }
}