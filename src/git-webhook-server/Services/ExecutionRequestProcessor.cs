using System;
using System.Threading;
using System.Threading.Tasks;
using git_webhook_server.Repositories;
using git_webhook_server.Services.ProcessExecutor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace git_webhook_server.Services
{
    public class ExecutionRequestProcessor : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ExecutionRequestProcessor> _logger;

        public ExecutionRequestProcessor(
            IServiceProvider services,
            ILogger<ExecutionRequestProcessor> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAvailableRequest(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while processing ExecutionRequest");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async ValueTask ProcessAvailableRequest(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IExecutionRequestRepository>();
            
            var request = await repository.GetFirstAvailable(stoppingToken);
            if (request == null)
            {
                return;
            }

            await repository.StartExecuting(request, stoppingToken);
            try
            {
                var executor = scope.ServiceProvider.GetRequiredService<IProcessExecutor>();
                var result = await executor.Execute(request.Command, stoppingToken);
                await repository.Finish(request, result, stoppingToken);
            }
            catch (Exception e)
            {
                await repository.Fail(request, e, stoppingToken);
            }
        }
    }
}