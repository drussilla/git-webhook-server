using git_webhook_server.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace git_webhook_server.Services.EventLogService
{
    public interface IEventLogService
    {
        Task<EventLog> CreateAsync(string payload, string headers);
        Task UpdateAsync(EventLog eventLog);
    }

    public class EventLogService : IEventLogService
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public EventLogService(IDbContextFactory<DatabaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<EventLog> CreateAsync(string payload, string headers) 
        {
            var eventLog = new EventLog(payload, headers);
            using var context = _dbContextFactory.CreateDbContext();
            await context.EventLogs.AddAsync(eventLog);
            await context.SaveChangesAsync();

            return eventLog;
        }

        public async Task UpdateAsync(EventLog eventLog)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.EventLogs.Attach(eventLog);
            await context.SaveChangesAsync();
        }
    }
}
