using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using git_webhook_server.Models;
using Microsoft.EntityFrameworkCore;

namespace git_webhook_server.Repositories
{
    public interface IEventLogRepository
    {
        Task<EventLog> CreateFromPayloadAsync(string payload, string headers, CancellationToken token);
        Task<EventLog> CreateManualAsync(CancellationToken token);
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

        public async Task<EventLog> CreateFromPayloadAsync(string payload, string headers, CancellationToken token) 
        {
            var eventLog = new EventLog(payload, headers);
            await using var context = _dbContextFactory.CreateDbContext();
            await context.EventLogs.AddAsync(eventLog, token);
            await context.SaveChangesAsync(token);

            return eventLog;
        }

        public Task<EventLog> CreateManualAsync(CancellationToken token)
        {
            return CreateFromPayloadAsync("Manually triggered", "-", token);
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
