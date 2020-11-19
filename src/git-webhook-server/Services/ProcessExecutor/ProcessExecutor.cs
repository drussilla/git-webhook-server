using System.IO;
using System.Threading.Tasks;

namespace git_webhook_server.Services.ProcessExecutor
{
    public class ProcessExecutor : IProcessExecutor
    {
        public async Task<ProcessExecutionResult> Execute(string commandline)
        {
            using var outputMemoryStream = new MemoryStream();
            using var outputTextWriter = new StreamWriter(outputMemoryStream);

            using var errorMemoryStream = new MemoryStream();
            using var errorTextWriter = new StreamWriter(errorMemoryStream);

            var exitCode = await AsyncProcessExecutor.StartProcess(
                commandline, 
                outputTextWriter: outputTextWriter,
                errorTextWriter: errorTextWriter);

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