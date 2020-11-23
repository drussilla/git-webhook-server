using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace git_webhook_server.Services.ProcessExecutor
{
    public class ProcessExecutor : IProcessExecutor
    {
        public async Task<ProcessExecutionResult> Execute(string commandline, CancellationToken token)
        {
            await using var outputMemoryStream = new MemoryStream();
            await using var outputTextWriter = new StreamWriter(outputMemoryStream);

            await using var errorMemoryStream = new MemoryStream();
            await using var errorTextWriter = new StreamWriter(errorMemoryStream);

            var exitCode = await AsyncProcessExecutor.StartProcess(
                commandline, 
                outputTextWriter: outputTextWriter,
                errorTextWriter: errorTextWriter,
                cancellationToken: token);

            await outputTextWriter.FlushAsync();
            await errorTextWriter.FlushAsync();

            outputMemoryStream.Seek(0, SeekOrigin.Begin);
            errorMemoryStream.Seek(0, SeekOrigin.Begin);

            return new ProcessExecutionResult(
                exitCode, 
                await new StreamReader(outputMemoryStream).ReadToEndAsync(), 
                await new StreamReader(errorMemoryStream).ReadToEndAsync());
        }
    }
}