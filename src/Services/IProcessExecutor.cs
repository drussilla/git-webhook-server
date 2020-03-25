namespace git_webhook_server.Services
{
    public interface IProcessExecutor
    {
        public void Execute(string commandline);
    }
}