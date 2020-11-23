using git_webhook_server.Services.ProcessExecutor;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace git_webhook_server.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<EventLog> EventLogs { get; set; }
        
        public DbSet<ExecutionRequest> ExecutionRequests { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<EventLog>()
                .HasOne(x => x.ExecutionRequest)
                .WithOne(x => x.EventLog)
                .HasForeignKey<ExecutionRequest>(x => x.EventLogId);

            modelBuilder
                .Entity<EventLog>()
                .Property(x => x.MatchedRule)
                .HasConversion(
                    v => JsonSerializer.Serialize<WebHookRule>(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<WebHookRule>(v, new JsonSerializerOptions()));

            modelBuilder
                .Entity<EventLog>()
                .Property(x => x.ExecutionResult)
                .HasConversion(
                    v => JsonSerializer.Serialize<ProcessExecutionResult>(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<ProcessExecutionResult>(v, new JsonSerializerOptions()));
        }
    }
}
