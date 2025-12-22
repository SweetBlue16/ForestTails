using FluentAssertions;
using ForestTails.Server.Logic.Utils;

namespace ForestTails.Server.Tests.UtilsTests
{
    public class SecureRandomTests
    {
        [Theory]
        [InlineData(6)]
        [InlineData(10)]
        [InlineData(128)]
        public void GenerateCodeTestShouldReturnStringOfExactLength(int length)
        {
            string code = SecureRandom.GenerateCode(length);
            code.Should().HaveLength(length);
            code.Should().NotContain(" ", "The code must not contain spaces.");
        }

        [Fact]
        public void GenerateCodeTestShouldNotUseNonAlphanumericCharacters()
        {
            string code = SecureRandom.GenerateCode(100);
            string notAllowedChars = "!\"§$%&/()=?`´^°<>;:,.|\\'~#-+*_";
            foreach (char c in notAllowedChars)
            {
                code.Should().NotContain(c.ToString());
            }
        }

        [Fact]
        public void GenerateSaltTestShouldReturnByteArrayOfCorrectSize()
        {
            byte[] salt = SecureRandom.GenerateSalt(32);
            salt.Should().HaveCount(32);
            salt.Should().NotBeNull();
        }

        [Fact]
        public void NextTestShouldReturnValuesWithinRange()
        {
            int minValue = 10;
            int maxValue = 20;
            for (int i = 0; i < 100; i++)
            {
                int randomValue = SecureRandom.Next(minValue, maxValue);
                randomValue.Should().BeGreaterThanOrEqualTo(minValue);
                randomValue.Should().BeLessThan(maxValue);
            }
        }
    }
}
