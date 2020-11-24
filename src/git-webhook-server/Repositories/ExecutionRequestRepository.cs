using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using git_webhook_server.Models;
using git_webhook_server.Services.ProcessExecutor;
using Microsoft.EntityFrameworkCore;

namespace git_webhook_server.Repositories
{
    public interface IExecutionRequestRepository
    {
        ValueTask<ExecutionRequest> GetFirstAvailable(CancellationToken token);
        ValueTask StartExecuting(ExecutionRequest request, CancellationToken token);
        ValueTask Finish(ExecutionRequest request, ProcessExecutionResult result, CancellationToken stoppingToken);
        ValueTask Fail(ExecutionRequest request, Exception exception, CancellationToken stoppingToken);
        ValueTask Create(WebHookRule rule, Guid eventLog, CancellationToken token);
    }

    public class ExecutionRequestRepository : IExecutionRequestRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public ExecutionRequestRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async ValueTask<ExecutionRequest> GetFirstAvailable(CancellationToken token)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            return await context.ExecutionRequests
                .Include(x => x.EventLog)
                .Where(x => x.Status == ExecutionRequestStatus.Scheduled)
                .OrderBy(x => x.CreatedOn)
                .FirstOrDefaultAsync(token);
        }

        public async ValueTask StartExecuting(ExecutionRequest request, CancellationToken token)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            request.Status = ExecutionRequestStatus.Executing;
            request.StartedOn = DateTime.UtcNow;
            
            context.ExecutionRequests.Update(request);
            await context.SaveChangesAsync(token);
        }

        public async ValueTask Finish(ExecutionRequest request, ProcessExecutionResult result, CancellationToken stoppingToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            request.Status = ExecutionRequestStatus.Processed;
            request.ProcessedOn = DateTime.UtcNow;
            request.EventLog.ExecutionResult = result;
            request.EventLog.Succeeded = result.ExitCode == 0;
            request.EventLog.StatusMessage =
                result.ExitCode == 0 ? "Finished" : $"Failed with {result.ExitCode} exit code";

            context.ExecutionRequests.Update(request);
            await context.SaveChangesAsync(stoppingToken);
        }

        public async ValueTask Fail(ExecutionRequest request, Exception exception, CancellationToken stoppingToken)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            request.Status = ExecutionRequestStatus.Processed;
            request.ProcessedOn = DateTime.UtcNow;
            request.EventLog.ExecutionResult = new ProcessExecutionResult(-1, "", exception.Message);
            request.EventLog.Succeeded = false;
            request.EventLog.StatusMessage = "Failed";
            
            context.ExecutionRequests.Update(request);
            await context.SaveChangesAsync(stoppingToken);
        }

        public async ValueTask Create(WebHookRule rule, Guid eventLog, CancellationToken token)
        {
            await using var context = _dbContextFactory.CreateDbContext();
            await context.ExecutionRequests.AddAsync(new ExecutionRequest
            {
                Id = Guid.NewGuid(),
                Status = ExecutionRequestStatus.Scheduled,
                CreatedOn = DateTime.UtcNow,
                Command = rule.Execute,
                EventLogId = eventLog
            }, token);

            var eventLogItem = await context.EventLogs.FirstAsync(x => x.Id == eventLog, token);
            eventLogItem.StatusMessage = $"\"{rule.Name}\" rule matched";
            eventLogItem.MatchedRule = rule;
            await context.SaveChangesAsync(token);
        }
    }
}