using git_webhook_server.Services.EventProcessors;
using git_webhook_server.Services.ProcessExecutor;
using System;

namespace git_webhook_server.Models
{
    public class EventLog
    {
        public EventLog(string payload, string headers)
        {
            Id = Guid.NewGuid();
            EventReceivedOn = DateTime.UtcNow;
            Payload = payload;
            Headers = headers;
        }

        public Guid Id { get; set; }
        public DateTime EventReceivedOn { get; set; }
        public string Payload { get; set; }
        public string Headers { get; set; }
        public WebHookRule MatchedRule { get; set; }
        public ProcessExecutionResult ExecutionResult { get; set; }
        public string StatusMessage { get; set; }
        public bool? Succeeded { get; set; }
        
        public ExecutionRequest ExecutionRequest { get; set; }

        public void Error(string message)
        {
            Succeeded = false;
            StatusMessage = message;
        }

        internal void ReadResult(EventProcessorResult result)
        {
            Succeeded = result.Success == false ? false : null;
            StatusMessage = result.Message;
            MatchedRule = result.MatchedRule;
        }
    }
}