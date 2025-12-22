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
    public class SanctionRepositoryTests : IDisposable
    {
        private readonly TestDbFactory dbFactory;
        private readonly SanctionRepository sanctionRepository;

        public SanctionRepositoryTests()
        {
            dbFactory = new TestDbFactory();
            var loggerMock = new Mock<ILogger<SanctionRepository>>();
            sanctionRepository = new SanctionRepository(dbFactory, loggerMock.Object);
        }

        [Fact]
        public async Task ApplySanctionAsyncTestShouldSaveSanction()
        {
            await AddUser(1);
            var sanction = new Sanction
            { 
                UserId = 1, 
                Type = SanctionType.TemporaryBan, 
                Reason = "Toxic", 
                StartDate = DateTime.UtcNow, 
                EndDate = DateTime.UtcNow.AddDays(1)
            };
            await sanctionRepository.ApplySanctionAsync(sanction);
            using var context = dbFactory.CreateDbContext();
            var saved = await context.Sanctions.FirstOrDefaultAsync();
            saved.Should().NotBeNull();
            saved?.Reason.Should().Be("Toxic");
        }

        [Fact]
        public async Task GetActiveBanAsyncTestShouldReturnPermanentBan()
        {
            await AddUser(1);
            await AddSanction(1, SanctionType.PermanentBan, null);
            var ban = await sanctionRepository.GetActiveBanAsync(1);
            ban.Should().NotBeNull();
            ban!.Type.Should().Be(SanctionType.PermanentBan);
        }

        [Fact]
        public async Task GetActiveBanAsyncTestShouldReturnTemporaryBanIfActive()
        {
            await AddUser(1);
            await AddSanction(1, SanctionType.TemporaryBan, DateTime.UtcNow.AddHours(1));
            var ban = await sanctionRepository.GetActiveBanAsync(1);
            ban.Should().NotBeNull();
        }

        [Fact]
        public async Task GetActiveBanAsyncTestShouldReturnNullIfTemporaryBanExpired()
        {
            await AddUser(1);
            await AddSanction(1, SanctionType.TemporaryBan, DateTime.UtcNow.AddHours(-1));
            var ban = await sanctionRepository.GetActiveBanAsync(1);
            ban.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveBanAsyncTestShouldReturnNullIfNoSanctions()
        {
            await AddUser(1);
            var ban = await sanctionRepository.GetActiveBanAsync(1);
            ban.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveBanAsyncTestShouldReturnLatestIfMultipleSanctionsExist()
        {
            await AddUser(1);
            await AddSanction(1, SanctionType.TemporaryBan, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(-1));
            await AddSanction(1, SanctionType.PermanentBan, null, DateTime.UtcNow);
            var ban = await sanctionRepository.GetActiveBanAsync(1);
            ban!.Type.Should().Be(SanctionType.PermanentBan);
        }

        private async Task AddUser(int id)
        {
            using var context = dbFactory.CreateDbContext();
            context.Users.Add(new User
            {
                Id = id,
                Username = $"U{id}",
                Email = $"{id}",
                PasswordHash = ".",
                FullName = "."
            });
            await context.SaveChangesAsync();
        }

        private async Task AddSanction(int userId, SanctionType type, DateTime? end, DateTime? start = null)
        {
            using var context = dbFactory.CreateDbContext();
            context.Sanctions.Add(new Sanction
            {
                UserId = userId,
                Type = type,
                EndDate = end,
                StartDate = start ?? DateTime.UtcNow,
                Reason = "test"
            });
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            dbFactory.Dispose();
        }
    }
}
