using System.Threading.Tasks;

namespace git_webhook_server.Services.ProcessExecutor
{
    public interface IProcessExecutor
    {
        public Task<ProcessExecutionResult> Execute(string commandline);
    }
}