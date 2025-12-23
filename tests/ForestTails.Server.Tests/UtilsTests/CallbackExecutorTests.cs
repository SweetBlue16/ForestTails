using FluentAssertions;
using ForestTails.Server.Logic.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.UtilsTests
{
    public class CallbackExecutorTests
    {
        private readonly Mock<ILogger<CallbackExecutor>> loggerMock;
        private readonly CallbackExecutor callbackExecutor;

        public CallbackExecutorTests()
        {
            loggerMock = new Mock<ILogger<CallbackExecutor>>();
            callbackExecutor = new CallbackExecutor(loggerMock.Object);
        }

        [Fact]
        public void ExecuteTestShouldCatchExceptionWhenOperationContextIsMissing()
        {
            Action act = () => callbackExecutor.Execute<object>(cb => cb.ToString());
            act.Should().NotThrow("the executor must internally catch any WCF errors");
            loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }
    }
}
