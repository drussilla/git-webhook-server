using System;

namespace git_webhook_server.Models
{
    public class ExecutionRequest
    {
        public Guid Id { get; set; }
        
        public string Command { get; set; }
        
        public Guid EventLogId { get; set; }
        public EventLog EventLog { get; set; }
        
        public ExecutionRequestStatus Status { get; set; }
        
        public DateTime CreatedOn { get; set; }
        
        public DateTime? StartedOn { get; set; }
        
        public DateTime? ProcessedOn { get; set; }
    }

    public enum ExecutionRequestStatus
    {
        Scheduled,
        Executing,
        Processed
    }
}