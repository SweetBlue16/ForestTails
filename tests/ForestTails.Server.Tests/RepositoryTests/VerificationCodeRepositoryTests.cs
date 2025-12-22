using FluentAssertions;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Tests.Common;
using ForestTails.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.RepositoryTests
{
    public class VerificationCodeRepositoryTests : IDisposable
    {
        private readonly TestDbFactory dbFactory;
        private readonly VerificationCodeRepository repository;

        public VerificationCodeRepositoryTests()
        {
            dbFactory = new TestDbFactory();
            var loggerMock = new Mock<ILogger<VerificationCodeRepository>>();
            repository = new VerificationCodeRepository(dbFactory, loggerMock.Object);
        }

        [Fact]
        public async Task SaveCodeAsyncTestShouldSaveNewCode()
        {
            await repository.SaveCodeAsync("new@test.com", "123456", CodeType.Registration);
            using var context = dbFactory.CreateDbContext();
            var code = await context.VerificationCodes.FirstAsync();
            code.Code.Should().Be("123456");
            code.Email.Should().Be("new@test.com");
        }

        [Fact]
        public async Task SaveCodeAsyncTestShouldDeleteOldCodesBeforeSavingNew()
        {
            await repository.SaveCodeAsync("reuse@test.com", "OLD111", CodeType.Registration);
            await repository.SaveCodeAsync("reuse@test.com", "NEW222", CodeType.Registration);
            using var context = dbFactory.CreateDbContext();
            var codes = await context.VerificationCodes.Where(x => x.Email == "reuse@test.com").ToListAsync();
            codes.Should().HaveCount(1);
            codes.First().Code.Should().Be("NEW222");
        }

        [Fact]
        public async Task SaveCodeAsyncTestShouldNotDeleteCodesOfDifferentType()
        {
            await repository.SaveCodeAsync("multi@test.com", "REG123", CodeType.Registration);
            await repository.SaveCodeAsync("multi@test.com", "REC456", CodeType.PasswordRecovery);
            using var context = dbFactory.CreateDbContext();
            var count = await context.VerificationCodes.CountAsync();
            count.Should().Be(2);
        }

        [Fact]
        public async Task ValidateCodeAsyncShouldReturnTrueForValidCode()
        {
            await repository.SaveCodeAsync("valid@test.com", "999999", CodeType.Registration);
            var result = await repository.ValidateCodeAsync("valid@test.com", "999999", CodeType.Registration);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateCodeAsyncShouldConsumeCodeAfterValidation()
        {
            await repository.SaveCodeAsync("once@test.com", "111111", CodeType.Registration);
            await repository.ValidateCodeAsync("once@test.com", "111111", CodeType.Registration);
            var secondTry = await repository.ValidateCodeAsync("once@test.com", "111111", CodeType.Registration);
            secondTry.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateCodeAsyncShouldReturnFalseForWrongCode()
        {
            await repository.SaveCodeAsync("wrong@test.com", "123456", CodeType.Registration);
            var result = await repository.ValidateCodeAsync("wrong@test.com", "000000", CodeType.Registration);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateCodeAsyncShouldReturnFalseForWrongType()
        {
            await repository.SaveCodeAsync("type@test.com", "123456", CodeType.Registration);
            var result = await repository.ValidateCodeAsync("type@test.com", "123456", CodeType.PasswordRecovery);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateCodeAsyncShouldReturnFalseIfExpired()
        {
            using (var context = dbFactory.CreateDbContext())
            {
                context.VerificationCodes.Add(new VerificationCode
                {
                    Email = "expired@test.com",
                    Code = "EXP123",
                    Type = CodeType.Registration,
                    ExpirationDate = DateTime.UtcNow.AddMinutes(-10)
                });
                await context.SaveChangesAsync();
            }
            var result = await repository.ValidateCodeAsync("expired@test.com", "EXP123", CodeType.Registration);
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            dbFactory.Dispose();
        }
    }
}
