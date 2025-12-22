using ForestTails.Server.Logic.Utils;
using ForestTails.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.UtilsTests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IEmailService> emailServiceMock;
        private readonly Mock<ILogger<NotificationService>> loggerMock;
        private readonly NotificationService notificationService;

        public NotificationServiceTests()
        {
            emailServiceMock = new Mock<IEmailService>();
            loggerMock = new Mock<ILogger<NotificationService>>();
            notificationService = new NotificationService(emailServiceMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task SendVerificationCodeAsyncTestShouldSendEmailWithCorrectCode()
        {
            string email = "test@user.com";
            string code = "ABC123";
            await notificationService.SendVerificationCodeAsync(email, code, CodeType.Registration);
            emailServiceMock.Verify(x => x.SendEmailAsync(
                It.Is<string>(e => e == email),
                It.Is<string>(s => s.Contains("Verify")),
                It.Is<string>(b => b.Contains(code))
            ), Times.Once);
        }

        [Fact]
        public async Task SendWelcomeAsyncTestShouldIncludeUsername()
        {
            string email = "new@user.com";
            string username = "GamerOne";
            await notificationService.SendWelcomeAsync(email, username);
            emailServiceMock.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("Welcome")),
                It.Is<string>(b => b.Contains(username))
            ), Times.Once);
        }

        [Fact]
        public async Task SendPasswordRecoveryCodeAsyncTestShouldSendEmailWithCorrectCode()
        {
            string email = "test@user.com";
            string code = "XYZ456";
            await notificationService.SendVerificationCodeAsync(email, code, CodeType.PasswordRecovery);
            emailServiceMock.Verify(x => x.SendEmailAsync(
                It.Is<string>(e => e == email),
                It.Is<string>(s => s.Contains("Password")),
                It.Is<string>(b => b.Contains(code))
            ), Times.Once);
        }
    }
}
