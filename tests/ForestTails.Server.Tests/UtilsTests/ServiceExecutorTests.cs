using FluentAssertions;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Utils;
using ForestTails.Shared.Constants;
using ForestTails.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.UtilsTests
{
    public class ServiceExecutorTests
    {
        private readonly Mock<ILogger<ServiceExecutor>> loggerMock;
        private readonly ServiceExecutor serviceExecutor;

        public ServiceExecutorTests()
        {
            loggerMock = new Mock<ILogger<ServiceExecutor>>();
            serviceExecutor = new ServiceExecutor(loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsyncTestShouldReturnSuccessWhenActionSucceeds()
        {
            string expectedResult = "Hello World!";
            var response = await serviceExecutor.ExecuteSafeAsync(async () =>
            {
                return await Task.FromResult(expectedResult);
            },
            "TestOperation");
            response.IsSuccess.Should().BeTrue();
            response.Data.Should().Be(expectedResult);
            response.MessageCode.Should().Be(MessageCode.Success);
        }

        [Fact]
        public async Task ExecuteAsyncTestShouldReturnSpecificErrorWhenForestTailsExceptionIsThrown()
        {
            var exception = new NotFoundException("Resource not found");
            var response = await serviceExecutor.ExecuteSafeAsync<bool>(async () =>
            {
                throw exception;
            },
            "TestOperation");
            response.IsSuccess.Should().BeFalse();
            response.MessageCode.Should().Be(MessageCode.NotFound);
            response.Message.Should().Be("Resource not found");
        }

        [Fact]
        public async Task ExecuteAsyncTestShouldHandleTimeoutException()
        {
            var response = await serviceExecutor.ExecuteSafeAsync<int>(async () =>
            {
                await Task.Yield();
                throw new TimeoutException("Database Timeout");
            },
            "TestTimeout");
            response.IsSuccess.Should().BeFalse();
            response.MessageCode.Should().Be(MessageCode.Timeout);
        }

        [Fact]
        public async Task ExecuteAsyncTestShouldHandleSqlTagsInGenericException()
        {
            var exception = new Exception(SqlErrorTags.DataConflict + ": The user already exists.");
            var response = await serviceExecutor.ExecuteSafeAsync<string>(async () =>
            {
                throw exception;
            },
            "TestSqlError");
            response.IsSuccess.Should().BeFalse();
            response.MessageCode.Should().Be(MessageCode.Conflict);
            response.Message.Should().Be("The user already exists.");
        }

        [Fact]
        public async Task ExecuteAsyncTestShouldCatchUnhandledExceptionsAsInternalError()
        {
            var response = await serviceExecutor.ExecuteSafeAsync<object>(async () =>
            {
                throw new InvalidOperationException("Unexpected crash");
            },
            "TestCrash");
            response.IsSuccess.Should().BeFalse();
            response.MessageCode.Should().Be(MessageCode.ServerInternalError);
            response.Message.Should().NotContain("Unexpected crash");
            response.Message.Should().Contain("internal error");
        }
    }
}
