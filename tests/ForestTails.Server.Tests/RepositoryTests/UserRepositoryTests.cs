using FluentAssertions;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.RepositoryTests
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly TestDbFactory dbFactory;
        private readonly UserRepository userRepository;

        public UserRepositoryTests()
        {
            dbFactory = new TestDbFactory();
            var loggerMock = new Mock<ILogger<UserRepository>>();
            userRepository = new UserRepository(dbFactory, loggerMock.Object);
        }

        [Fact]
        public async Task CreateAsyncTestShouldPersistUserWithCorrectData()
        {
            var user = new User
            {
                Username = "Hero",
                Email = "h@h.com",
                PasswordHash = "hash",
                FullName = "Hero Name"
            };
            var created = await userRepository.CreateUserAsync(user);
            created.Id.Should().BeGreaterThan(0);
            created.Statistics.Should().NotBeNull("should initialise statistics automatically");
        }

        [Fact]
        public async Task GetByUsernameAsyncTestShouldReturnUserWhenExists()
        {
            await SeedUserAsync("Finder", "f@f.com");
            var result = await userRepository.GetByUsernameAsync("Finder");
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByUsernameAsyncTestShouldReturnEmptyUserWhenNotExists()
        {
            var result = await userRepository.GetByUsernameAsync("Ghost");
            result.Should().NotBeNull("should return empty object");
            result.Id.Should().Be(-1);
            result.Username.Should().Contain("GHOST");
        }

        [Fact]
        public async Task GetByIdAsyncTestShouldReturnUserWhenExists()
        {
            var id = await SeedUserAsync("ById", "id@id.com");
            var result = await userRepository.GetByIdAsync(id);
            result.Username.Should().Be("ById");
        }

        [Fact]
        public async Task GetByIdAsyncTestShouldReturnEmptyWhenIdNotFound()
        {
            var result = await userRepository.GetByIdAsync(9999);
            result.Id.Should().Be(-1);
        }

        [Fact]
        public async Task GetByEmailAsyncTestShouldReturnUserWhenExists()
        {
            await SeedUserAsync("Emailer", "unique@mail.com");
            var result = await userRepository.GetByEmailAsync("unique@mail.com");
            result.Username.Should().Be("Emailer");
        }

        [Fact]
        public async Task ExistsAsyncTestShouldReturnTrueWhenUsernameExists()
        {
            await SeedUserAsync("ExistingUser", "e@e.com");
            var exists = await userRepository.UserExistsAsync("ExistingUser", "other@mail.com");
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsyncTestShouldReturnTrueWhenEmailExists()
        {
            await SeedUserAsync("UserA", "taken@mail.com");
            var exists = await userRepository.UserExistsAsync("UserB", "taken@mail.com");
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsyncTestShouldReturnFalseWhenNeitherExists()
        {
            var exists = await userRepository.UserExistsAsync("NewUser", "new@mail.com");
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsyncTestShouldModifyData()
        {
            var id = await SeedUserAsync("Updater", "u@u.com");
            var user = await userRepository.GetByIdAsync(id);
            user.FullName = "Changed Name";
            user.Level = 10;
            await userRepository.UpdateUserAsync(user);
            var updated = await userRepository.GetByIdAsync(id);
            updated.FullName.Should().Be("Changed Name");
            updated.Level.Should().Be(10);
        }

        [Fact]
        public async Task UpdateCurrencyAsyncTestShouldUpdateOnlyCoins()
        {
            var id = await SeedUserAsync("Richie", "r@r.com");
            await userRepository.UpdateCurrencyAsync(id, 5000);
            var user = await userRepository.GetByIdAsync(id);
            user.MichiCoins.Should().Be(5000);
        }

        private async Task<int> SeedUserAsync(string username, string email)
        {
            using var context = dbFactory.CreateDbContext();
            var user = new User 
            { 
                Username = username, 
                Email = email, 
                PasswordHash = "x", 
                FullName = "x" 
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        }

        public void Dispose()
        {
            dbFactory.Dispose();
        }
    }
}
