using System;

namespace shared
{
    public class EventLogItem
    {
        public Guid Id { get; set; }
        public DateTime EventReceivedOn { get; set; }
        public string Payload { get; set; }
        public string Headers { get; set; }
        public string WebHookRuleName { get; set; }
        public string WebHookRuleCommand { get; set; }
        public int? ExitCode { get; set; }
        public string CommandOutput { get; set; }
        public string CommandError { get; set; }
        public string StatusMessage { get; set; }
        public bool? Succeeded { get; set; }
    }
}