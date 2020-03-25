using System.Diagnostics;

namespace git_webhook_server.Services
{
    public class ProcessExecutor : IProcessExecutor
    {
        public void Execute(string commandline)
        {
            var startInfo = new ProcessStartInfo(commandline)
            {
                UseShellExecute = false, 
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }
    }
}