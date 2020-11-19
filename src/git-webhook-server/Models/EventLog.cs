using git_webhook_server.Services.EventProcessors;
using git_webhook_server.Services.ProcessExecutor;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace git_webhook_server.Models
{
    public class EventLog
    {
        public EventLog()
        {
            EventReceivedOn = DateTime.UtcNow;
        }

        public DateTime EventReceivedOn { get; set; }
        public string Payload { get; set; }
        public WebHookRule MatchedRule { get; set; }
        public ProcessExecutionResult ExecutionResult { get; set; }
        public string StatusMessage { get; set; }
        public bool Succeeded { get; set; }

        public async Task ReadPayload(MemoryStream body)
        {
            using var reader = new StreamReader(body, Encoding.UTF8);
            Payload = await reader.ReadToEndAsync();
        }

        public void Error(string message)
        {
            Succeeded = false;
            StatusMessage = message;
        }

        public void Success(string message)
        {
            Succeeded = true;
            StatusMessage = message;
        }

        internal void ReadResult(EventProcessorResult result)
        {
            Succeeded = result.Success;
            StatusMessage = result.Message;
            MatchedRule = result.MatchedRule;
            ExecutionResult = result.ExecutionResult;
        }
    }
}