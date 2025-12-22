using FluentAssertions;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.RepositoryTests
{
    public class CosmeticRepositoryTests : IDisposable
    {
        private readonly TestDbFactory dbFactory;
        private readonly CosmeticRepository cosmeticRepository;

        public CosmeticRepositoryTests()
        {
            dbFactory = new TestDbFactory();
            var loggerMock = new Mock<ILogger<CosmeticRepository>>();
            cosmeticRepository = new CosmeticRepository(dbFactory, loggerMock.Object);
        }

        [Fact]
        public async Task GetCatalogAsyncTestShouldReturnAllWhenFlagIsFalse()
        {
            await AddCosmetic(1, "Active", true);
            await AddCosmetic(2, "Inactive", false);
            var all = await cosmeticRepository.GetCosmeticsCatalogAsync(onlyActive: false);
            all.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCatalogAsyncTestShouldReturnOnlyActiveWhenFlagIsTrue()
        {
            await AddCosmetic(1, "Active", true);
            await AddCosmetic(2, "Inactive", false);
            var active = await cosmeticRepository.GetCosmeticsCatalogAsync(onlyActive: true);
            active.Should().HaveCount(1);
            active.First().Name.Should().Be("Active");
        }

        [Fact]
        public async Task GetCosmeticByIdAsyncTestShouldReturnItem()
        {
            await AddCosmetic(10, "Hat", true);
            var item = await cosmeticRepository.GetCosmeticByIdAsync(10);
            item.Should().NotBeNull();
            item.Name.Should().Be("Hat");
        }

        [Fact]
        public async Task GetCosmeticByIdAsyncTestShouldReturnEmptyObjectIfNotFound()
        {
            var item = await cosmeticRepository.GetCosmeticByIdAsync(999);
            item.Should().BeEquivalentTo(new Cosmetic());
        }

        [Fact]
        public async Task UnlockForUserAsyncTestShouldCreateUnlockRecord()
        {
            await AddUser(1);
            await AddCosmetic(10, "Skin", true);
            await cosmeticRepository.UnlockForUserAsync(1, 10);
            var unlocked = await cosmeticRepository.IsCosmeticUnlockedAsync(1, 10);
            unlocked.Should().BeTrue();
        }

        [Fact]
        public async Task IsUnlockedAsyncTestShouldReturnFalseBeforePurchase()
        {
            await AddUser(1);
            await AddCosmetic(10, "Skin", true);
            var unlocked = await cosmeticRepository.IsCosmeticUnlockedAsync(1, 10);
            unlocked.Should().BeFalse();
        }

        [Fact]
        public async Task GetUnlockedByUserAsyncTestShouldReturnList()
        {
            await AddUser(1);
            await AddCosmetic(10, "A", true);
            await AddCosmetic(11, "B", true);
            await cosmeticRepository.UnlockForUserAsync(1, 10);
            await cosmeticRepository.UnlockForUserAsync(1, 11);
            var list = await cosmeticRepository.GetUnlockedCosmeticsByUserAsync(1);
            list.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUnlockedByUserAsyncTestShouldReturnEmptyForNewUser()
        {
            await AddUser(2);
            var list = await cosmeticRepository.GetUnlockedCosmeticsByUserAsync(2);
            list.Should().BeEmpty();
        }

        private async Task AddCosmetic(int id, string name, bool active)
        {
            using var context = dbFactory.CreateDbContext();
            context.Cosmetics.Add(new Cosmetic
            {
                Id = id,
                Name = name,
                IsActive = active, 
                Price = 10, 
                Description = ".", 
                ResourcePath = "." 
            });
            await context.SaveChangesAsync();
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

        public void Dispose()
        {
            dbFactory.Dispose();
        }
    }
}
