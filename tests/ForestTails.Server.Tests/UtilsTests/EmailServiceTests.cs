using FluentAssertions;
using ForestTails.Server.Logic.Config;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ForestTails.Server.Tests.UtilsTests
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> loggerMock;

        public EmailServiceTests()
        {
            loggerMock = new Mock<ILogger<EmailService>>();
        }

        [Fact]
        public async Task SendEmailAsyncTestShouldLogWarningAndReturnWhenConfigIsMissing() 
        {
            var emptySettings = Options.Create(new SmtpSettings
            {
                Host = string.Empty,
                SenderEmail = string.Empty
            });
            var service = new EmailService(emptySettings, loggerMock.Object);
            await service.SendEmailAsync("test@user.com", "Subject", "Body");
            loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("is not configured")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task SendEmailAsyncTestShouldNotThrowWhenCalledWithValidParamsButNoServer()
        {
            var settings = Options.Create(new SmtpSettings
            {
                Host = "smpt.fake.local",
                Port = 25,
                SenderEmail = "admin@forest.com"
            });
            var service = new EmailService(settings, loggerMock.Object);
            Func<Task> action = async () => await service.SendEmailAsync("test@user.com", "Subject", "Body");
            await action.Should().ThrowAsync<InfrastructureException>();
        }
    }
}
