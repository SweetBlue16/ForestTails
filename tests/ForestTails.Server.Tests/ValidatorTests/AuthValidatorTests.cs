using FluentAssertions;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Validators;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Enums;

namespace ForestTails.Server.Tests.ValidatorTests
{
    public class AuthValidatorTests
    {
        private readonly AuthValidator validator = new();

        [Fact]
        public void ValidateLoginTestShouldThrowWhenRequestIsNull()
        {
            Action act = () => validator.ValidateLogin(null!);
            act.Should().Throw<ValidationException>().WithMessage("*null*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidateLoginTestShouldThrowWhenUsernameIsMissing(string username)
        {
            var request = new LoginRequestDTO { Username = username, Password = "123" };
            Action act = () => validator.ValidateLogin(request);
            act.Should().Throw<ValidationException>().WithMessage("*required*");
        }

        [Fact]
        public void ValidateLoginTestShouldPassWhenDataIsValid()
        {
            var request = new LoginRequestDTO { Username = "ValidUser", Password = "ValidPass123!" };
            Action act = () => validator.ValidateLogin(request);
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateRegisterTestShouldThrowWhenRequestIsNull()
        {
            Action act = () => validator.ValidateRegister(null!);
            act.Should().Throw<ValidationException>();
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("User Name")]
        [InlineData("User!@#")]
        public void ValidateRegisterTestShouldThrowWhenUsernameFormatIsInvalid(string badUsername)
        {
            var request = new RegisterRequestDTO 
            {
                Username = badUsername, 
                Email = "test@test.com", 
                Password = "Pass1!",
                FullName = "Name" 
            };
            Action act = () => validator.ValidateRegister(request);
            act.Should().Throw<ValidationException>()
               .Where(e => e.Code == MessageCode.InvalidUsernameFormat);
        }

        [Theory]
        [InlineData("plainaddress")]
        [InlineData("@missingusername.com")]
        [InlineData("username@.com")]
        public void ValidateRegisterTestShouldThrowWhenEmailFormatIsInvalid(string badEmail)
        {
            var request = new RegisterRequestDTO 
            { 
                Username = "User1", 
                Email = badEmail, 
                Password = "Pass1!", 
                FullName = "Name" 
            };
            Action act = () => validator.ValidateRegister(request);

            act.Should().Throw<ValidationException>()
               .Where(e => e.Code == MessageCode.InvalidEmailFormat);
        }

        [Theory]
        [InlineData("nocaps123")]
        [InlineData("NONUMBERS")]
        [InlineData("Short1")]
        public void ValidateRegisterTestShouldThrowWhenPasswordIsWeak(string weakPass)
        {
            var request = new RegisterRequestDTO 
            {
                Username = "User1", 
                Email = "a@b.com", 
                Password = weakPass, 
                FullName = "Name"
            };
            Action act = () => validator.ValidateRegister(request);
            act.Should().Throw<ValidationException>()
               .Where(e => e.Code == MessageCode.InvalidPasswordFormat);
        }

        [Fact]
        public void ValidateRegisterTestShouldPassWhenAllFieldsAreValid()
        {
            var request = new RegisterRequestDTO 
            { 
                Username = "Hero123", 
                Email = "hero@game.com", 
                Password = "StrongPass1!", 
                FullName = "Hero Name" 
            };
            Action act = () => validator.ValidateRegister(request);
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateUserFoundTestShouldThrowAuthExceptionWhenUserIsNull()
        {
            Action act = () => validator.ValidateUserFound(null);
            act.Should().Throw<AuthException>()
               .Where(e => e.Code == MessageCode.InvalidCredentials);
        }

        [Fact]
        public void ValidateUserFoundTestShouldThrowAuthExceptionWhenUserIdIsZero()
        {
            Action act = () => validator.ValidateUserFound(new User { Id = 0 });
            act.Should().Throw<AuthException>()
               .Where(e => e.Code == MessageCode.InvalidCredentials);
        }

        [Fact]
        public void ValidateSanctionsTestShouldThrowWhenPermanentBanExists()
        {
            var ban = new Sanction { Type = SanctionType.PermanentBan, Reason = "Hacks" };
            Action act = () => validator.ValidateSanctions(ban);
            act.Should().Throw<AuthException>()
               .Where(e => e.Code == MessageCode.AccountBanned);
        }

        [Fact]
        public void ValidateSanctionsTestShouldThrowWhenTemporaryBanExists()
        {
            var ban = new Sanction { Type = SanctionType.TemporaryBan, Reason = "Toxic" };
            Action act = () => validator.ValidateSanctions(ban);
            act.Should().Throw<AuthException>()
               .Where(e => e.Code == MessageCode.AccountLocked);
        }

        [Fact]
        public void ValidateSanctionsTestShouldPassWhenNoSanction()
        {
            Action act = () => validator.ValidateSanctions(null);
            act.Should().NotThrow();
        }
    }
}
