using CoreWCF;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Utils;
using ForestTails.Server.Logic.Validators;
using ForestTails.Shared.Contracts;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AuthService : IAuthService
    {
        private const int VerificationCodeLength = 6;

        private readonly IUserRepository userRepository;
        private readonly ISanctionRepository sanctionRepository;
        private readonly IVerificationCodeRepository verificationCodeRepository;
        private readonly INotificationService notificationService;

        private readonly ServiceExecutor executor;
        private readonly CallbackExecutor callbackExecutor;
        private readonly IAuthValidator authValidator;
        private readonly SessionManager sessionManager;

        private User? currentUser;

        public AuthService(
            IUserRepository userRepository,
            ISanctionRepository sanctionRepository,
            IVerificationCodeRepository verificationCodeRepository,
            INotificationService notificationService,
            ServiceExecutor executor,
            CallbackExecutor callbackExecutor,
            IAuthValidator authValidator,
            SessionManager sessionManager)
        {
            this.userRepository = userRepository;
            this.sanctionRepository = sanctionRepository;
            this.verificationCodeRepository = verificationCodeRepository;
            this.notificationService = notificationService;
            this.executor = executor;
            this.callbackExecutor = callbackExecutor;
            this.authValidator = authValidator;
            this.sessionManager = sessionManager;
        }

        public async Task LoginAsync(LoginRequestDTO request)
        {
            var response = await executor.ExecuteSafeAsync(async () =>
            {
                authValidator.ValidateLogin(request);
                var user = await userRepository.GetByUsernameAsync(request.Username);
                authValidator.ValidateUserFound(user);
                authValidator.ValidatePassword(user, request.Password);

                if (!user.IsVerified)
                { 
                    throw new AuthException("Email not verified", MessageCode.AccountNotVerified);
                }
                var sanction = await sanctionRepository.GetActiveBanAsync(user.Id);
                authValidator.ValidateSanctions(sanction);

                user.LastLogin = DateTime.UtcNow;
                await userRepository.UpdateUserAsync(user);
                currentUser = user;

                var currentCallback = OperationContext.Current.GetCallbackChannel<IAuthCallback>();
                sessionManager.AddSession(user.Username, currentCallback);

                return new UserDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    MichiCoins = user.MichiCoins,
                    SelectedAvatarId = user.SelectedAvatarId,
                };
            },
            nameof(LoginAsync));
            callbackExecutor.Execute<IAuthCallback>(callback => callback.OnLoginResult(response));
        }

        public async Task LogoutAsync()
        {
            var response = await executor.ExecuteSafeAsync(async () =>
            {
                if (currentUser != null)
                {
                    sessionManager.RemoveSession(currentUser.Username);
                    currentUser = null;
                }
                return await Task.FromResult(true);
            },
            nameof(LogoutAsync));
            callbackExecutor.Execute<IAuthCallback>(callback => callback.OnLogoutResult(response));
        }

        public async Task RegisterAsync(RegisterRequestDTO request)
        {
            var response = await executor.ExecuteSafeAsync(async () =>
            {
                authValidator.ValidateRegister(request);
                bool exists = await userRepository.UserExistsAsync(request.Username, request.Email);
                if (exists)
                {
                    throw new AuthException("Username or email already in use", MessageCode.UserAlreadyExists);
                }

                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = PasswordHelper.HashPassword(request.Password),
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    MichiCoins = 0
                };
                await userRepository.CreateUserAsync(newUser);

                var code = SecureRandom.GenerateCode(VerificationCodeLength);
                await verificationCodeRepository.SaveCodeAsync(newUser.Email, code, CodeType.Registration);

                _ = notificationService.SendVerificationCodeAsync(newUser.Email, code, CodeType.Registration);
                return new UserDTO
                {
                    Username = newUser.Username,
                    Email = newUser.Email
                };
            },
            nameof(RegisterAsync));
            callbackExecutor.Execute<IAuthCallback>(callback => callback.OnRegisterResult(response));
        }
    }
}
