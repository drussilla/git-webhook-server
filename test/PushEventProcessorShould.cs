using FluentAssertions;
using git_webhook_server;
using git_webhook_server.PayloadModels;
using git_webhook_server.Services;
using git_webhook_server.Services.EventProcessors;
using git_webhook_server.Services.ProcessExecutor;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace git_webhook_server_tests
{
    public class PushEventProcessorShould
    {
        [Fact]
        public async Task ExecuteScript_WhenRuleIsMatched()
        {
            // Arrange
            var processExecutor = new Mock<IProcessExecutor>();
            var ruleMatcher = new Mock<IRuleMatcher>();

            ruleMatcher
                .Setup(x => 
                    x.Match("something", "url"))
                .Returns(new WebHookRule
                {
                    Execute = "exe"
                });

            processExecutor.Setup(x => x.Execute("exe"));

            var sup = new PushEventProcessor(processExecutor.Object, ruleMatcher.Object, new NullLogger<PushEventProcessor>());

            // Act
            var result = await sup.Process(new PushEventPayload
            {
                Ref = "something", Repository = new Repository { Url = "url" }
            });

            // Assert
            result.Success.Should().Be(true);
            ruleMatcher.Verify();
            processExecutor.Verify();
        }

        [Fact]
        public async Task NotExecuteAnything_WhenRuleIsNotMatched()
        {
            // Arrange
            var processExecutor = new Mock<IProcessExecutor>();
            var ruleMatcher = new Mock<IRuleMatcher>();

            ruleMatcher
                .Setup(x => 
                    x.Match("something", "url"))
                .Returns((WebHookRule)null);

            var sup = new PushEventProcessor(processExecutor.Object, ruleMatcher.Object, new NullLogger<PushEventProcessor>());

            // Act
            var result = await sup.Process(new PushEventPayload
            {
                Ref = "something", Repository = new Repository { Url = "url" }
            });

            // Assert
            result.Success.Should().Be(false);
            ruleMatcher.Verify();
            processExecutor.Verify(x => x.Execute(It.IsAny<string>()), Times.Never);
        }
    }
}