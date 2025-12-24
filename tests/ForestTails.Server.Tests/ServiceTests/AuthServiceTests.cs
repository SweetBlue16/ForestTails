using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Services;
using ForestTails.Server.Logic.Utils;
using ForestTails.Server.Logic.Validators;
using ForestTails.Shared.Contracts;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.ServiceTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock = new();
        private readonly Mock<ISanctionRepository> sanctionRepositoryMock = new();
        private readonly Mock<IVerificationCodeRepository> verificationCodeRepositoryMock = new();
        private readonly Mock<INotificationService> notificationMock = new();

        private readonly Mock<IAuthValidator> validatorMock = new();
        private readonly Mock<CallbackExecutor> callbackExecutorMock;

        private readonly ServiceExecutor serviceExecutor;
        private readonly AuthService authService;
        private readonly SessionManager sessionManager;

        public AuthServiceTests()
        {
            serviceExecutor = new ServiceExecutor(new Mock<ILogger<ServiceExecutor>>().Object);
            callbackExecutorMock = new Mock<CallbackExecutor>(new Mock<ILogger<CallbackExecutor>>().Object);
            sessionManager = new SessionManager(new Mock<ILogger<SessionManager>>().Object);
            callbackExecutorMock.Setup(x => x.Execute(It.IsAny<Action<IAuthCallback>>()))
                .Callback<Action<IAuthCallback>>(action =>
                {
                });
            authService = new AuthService(
                userRepositoryMock.Object,
                sanctionRepositoryMock.Object,
                verificationCodeRepositoryMock.Object,
                notificationMock.Object,
                serviceExecutor,
                callbackExecutorMock.Object,
                validatorMock.Object,
                sessionManager
            );
        }

        [Fact]
        public async Task LoginAsyncTestShouldCallCallbackWithSuccessWhenValid()
        {
            var request = new LoginRequestDTO { Username = "Hero", Password = "Pw" };
            var user = new User { Id = 1, Username = "Hero", IsVerified = true, PasswordHash = "hash" };
            userRepositoryMock.Setup(x => x.GetByUsernameAsync("Hero")).ReturnsAsync(user);
            sanctionRepositoryMock.Setup(x => x.GetActiveBanAsync(1)).ReturnsAsync((Sanction?)null);
            validatorMock.Setup(v => v.ValidateLogin(request));
            validatorMock.Setup(v => v.ValidateUserFound(user));
            validatorMock.Setup(v => v.ValidatePassword(user, "Pw"));
            validatorMock.Setup(v => v.ValidateSanctions(null));
            await authService.LoginAsync(request);
            userRepositoryMock.Verify(x => x.UpdateUserAsync(It.Is<User>(u => u.LastLogin != default)), Times.Once);
            callbackExecutorMock.Verify(x => x.Execute(It.IsAny<Action<IAuthCallback>>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsyncTestShouldReturnErrorWhenValidatorFails()
        {
            var request = new LoginRequestDTO();
            validatorMock.Setup(x => x.ValidateLogin(request))
                .Throws(new ValidationException("Bad Input"));
            await authService.LoginAsync(request);
            userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
            callbackExecutorMock.Verify(x => x.Execute(It.IsAny<Action<IAuthCallback>>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsyncTestShouldReturnAccountNotVerifiedWhenUserIsNotVerified()
        {
            var user = new User { Id = 1, IsVerified = false };
            userRepositoryMock.Setup(x => x.GetByUsernameAsync("Hero")).ReturnsAsync(user);
            await authService.LoginAsync(new LoginRequestDTO { Username = "Hero" });
            callbackExecutorMock.Verify(x => x.Execute(It.IsAny<Action<IAuthCallback>>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsyncTestShouldCreateUserAndSendCode()
        {
            var request = new RegisterRequestDTO { Username = "New", Email = "new@mail.com" };
            userRepositoryMock.Setup(x => x.UserExistsAsync("New", "new@mail.com")).ReturnsAsync(false);
            await authService.RegisterAsync(request);
            userRepositoryMock.Verify(x => x.CreateUserAsync(It.Is<User>(u => u.MichiCoins == 0 && !u.IsVerified)), Times.Once);
            verificationCodeRepositoryMock.Verify(x => x.SaveCodeAsync("new@mail.com", It.IsAny<string>(), CodeType.Registration), Times.Once);
            notificationMock.Verify(x => x.SendVerificationCodeAsync("new@mail.com", It.IsAny<string>(), CodeType.Registration), Times.Once);
            callbackExecutorMock.Verify(x => x.Execute(It.IsAny<Action<IAuthCallback>>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsyncTestShouldReturnConflictWhenUserExists()
        {
            var request = new RegisterRequestDTO { Username = "Exist", Email = "e@mail.com" };
            userRepositoryMock.Setup(x => x.UserExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            await authService.RegisterAsync(request);
            userRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LogoutAsyncTestShouldExecuteCallback()
        {
            await authService.LogoutAsync();
            callbackExecutorMock.Verify(x => x.Execute(It.IsAny<Action<IAuthCallback>>()), Times.Once);
        }
    }
}
