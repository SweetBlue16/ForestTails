using FluentAssertions;
using ForestTails.Server.Logic.Utils;

namespace ForestTails.Server.Tests.UtilsTests
{
    public class PasswordHelperTests
    {
        [Fact]
        public void HashPasswordTestShouldReturnFormattedStringWithThreeParts()
        {
            string password = "TestPassword123";
            string hash = PasswordHelper.HashPassword(password);
            hash.Should().NotBeNullOrEmpty();
            var parts = hash.Split('.');
            parts.Should().HaveCount(3, "the format must be Iterations.Salt.Hash.");
            int.TryParse(parts[0], out _).Should().BeTrue("the first part should be the iterations (int)");
        }

        [Fact]
        public void VerifyPasswordTestShouldReturnTrueWhenPasswordIsCorrect()
        {
            string password = "MySecretPassword";
            string hash = PasswordHelper.HashPassword(password);
            bool result = PasswordHelper.VerifyPassword(hash, password);
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPasswordTestShouldReturnFalseWhenPasswordIsDifferent()
        {
            string hash = PasswordHelper.HashPassword("CorrectPassword");
            bool result = PasswordHelper.VerifyPassword(hash, "WrongPassword");
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPasswordTestShouldReturnFalseWhenHashIsTampered()
        {
            string password = "Password";
            string validHash = PasswordHelper.HashPassword(password);
            string tamperedHash = validHash.Replace('A', 'B');
            bool result = PasswordHelper.VerifyPassword(tamperedHash, password);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void VerifyPasswordTestShouldReturnFalseWhenInputsAreEmpty(string emptyInput)
        {
            PasswordHelper.VerifyPassword("valid.hash.string", emptyInput).Should().BeFalse();
            PasswordHelper.VerifyPassword(emptyInput, "password").Should().BeFalse();
        }

        [Fact]
        public void VerifyPasswordTestShouldReturnFalseWhenHashFormatIsInvalid()
        {
            string invalidHash = "NotValidHash";
            bool result = PasswordHelper.VerifyPassword(invalidHash, "password");
            result.Should().BeFalse();
        }
    }
}
