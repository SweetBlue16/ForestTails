using ForestTails.Server.Data.Entities;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Utils;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Enums;
using System.Text.RegularExpressions;

namespace ForestTails.Server.Logic.Validators
{
    public interface IAuthValidator
    {
        void ValidateLogin(LoginRequestDTO loginRequest);
        void ValidateRegister(RegisterRequestDTO registerRequest);
        void ValidateUserFound(User? user);
        void ValidatePassword(User user, string inputPassword);
        void ValidateSanctions(Sanction? sanction);
    }

    public partial class AuthValidator : IAuthValidator
    {
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")]
        private static partial Regex PasswordRegex();

        [GeneratedRegex(@"^[a-zA-Z0-9_]{3,20}$")]
        private static partial Regex UsernameRegex();

        public void ValidateLogin(LoginRequestDTO loginRequest)
        {
            if (loginRequest == null)
                throw new ValidationException("Request cannot be null");
            if (string.IsNullOrWhiteSpace(loginRequest.Username))
                throw new ValidationException("Username required");
            if (string.IsNullOrWhiteSpace(loginRequest.Password))
                throw new ValidationException("Password required");
        }

        public void ValidatePassword(User user, string inputPassword)
        {
            if (PasswordHelper.VerifyPassword(user.PasswordHash, inputPassword))
                throw new AuthException("Password mismatch", MessageCode.InvalidCredentials);
        }

        public void ValidateRegister(RegisterRequestDTO registerRequest)
        {
            if (registerRequest == null)
                throw new ValidationException("Request cannot be null");
            if (!UsernameRegex().IsMatch(registerRequest.Username))
                throw new ValidationException("Invalid username format", MessageCode.InvalidUsernameFormat);
            if (!EmailRegex().IsMatch(registerRequest.Email))
                throw new ValidationException("Invalid email format", MessageCode.InvalidEmailFormat);
            if (!PasswordRegex().IsMatch(registerRequest.Password))
                throw new ValidationException("Invalid password format", MessageCode.InvalidPasswordFormat);
            if (string.IsNullOrWhiteSpace(registerRequest.FullName))
                throw new ValidationException("Full name required", MessageCode.MissingRequiredField);
        }

        public void ValidateSanctions(Sanction? sanction)
        {
            if (sanction != null)
            {
                var code = sanction.Type == SanctionType.PermanentBan
                    ? MessageCode.AccountBanned
                    : MessageCode.AccountLocked;
                throw new AuthException($"Active sanction found: {sanction.Reason}", code);
            }
        }

        public void ValidateUserFound(User? user)
        {
            if (user == null || user.Id == 0)
                throw new AuthException("User not found during login", MessageCode.InvalidCredentials);
        }
    }
}
