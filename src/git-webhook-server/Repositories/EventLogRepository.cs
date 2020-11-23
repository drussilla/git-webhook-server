using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using git_webhook_server.Models;
using Microsoft.EntityFrameworkCore;

namespace git_webhook_server.Repositories
{
    public interface IEventLogRepository
    {
        Task<EventLog> CreateAsync(string payload, string headers);
        Task UpdateAsync(EventLog eventLog);
        Task<List<EventLog>> Get(int itemsToReturn);
    }

    public class EventLogRepository : IEventLogRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public EventLogRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<EventLog> CreateAsync(string payload, string headers) 
        {
            var eventLog = new EventLog(payload, headers);
            await using var context = _dbContextFactory.CreateDbContext();
            await context.EventLogs.AddAsync(eventLog);
            await context.SaveChangesAsync();

            return eventLog;
        }

        public async Task UpdateAsync(EventLog eventLog)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            context.EventLogs.Update(eventLog);
            await context.SaveChangesAsync();
        }

        public async Task<List<EventLog>> Get(int itemsToReturn)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.EventLogs
                .OrderByDescending(x => x.EventReceivedOn)
                .Take(itemsToReturn)
                .ToListAsync();
        }
    }
}
