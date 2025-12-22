using FluentAssertions;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Tests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.RepositoryTests
{
    public class StatisticsRepositoryTests : IDisposable
    {
        private readonly TestDbFactory dbFactory;
        private readonly StatisticsRepository statisticsRepository;

        public StatisticsRepositoryTests()
        {
            dbFactory = new TestDbFactory();
            var loggerMock = new Mock<ILogger<StatisticsRepository>>();
            statisticsRepository = new StatisticsRepository(dbFactory, loggerMock.Object);
        }

        [Fact]
        public async Task UpdateAsyncTestShouldInsertIfStatsDoNotExist()
        {
            int userId = 1;
            await CreateUserForTest(userId);
            var stats = new PlayerStatistics { UserId = userId, Wins = 5 };
            await statisticsRepository.UpdateStatisticsAsync(stats);
            var result = await statisticsRepository.GetStatisticsByUserIdAsync(userId);
            result.Wins.Should().Be(5);
        }

        [Fact]
        public async Task UpdateAsyncTestShouldUpdateIfStatsExist()
        {
            int userId = 1;
            await CreateUserForTest(userId);
            await statisticsRepository.UpdateStatisticsAsync(new PlayerStatistics
            {
                UserId = userId,
                Wins = 5
            });
            await statisticsRepository.UpdateStatisticsAsync(new PlayerStatistics
            {
                UserId = userId,
                Wins = 10
            });
            var result = await statisticsRepository.GetStatisticsByUserIdAsync(userId);
            result.Wins.Should().Be(10);
        }

        [Fact]
        public async Task GetByUserIdAsyncTestShouldReturnEmptyStatsIfUserHasNone()
        {
            var result = await statisticsRepository.GetStatisticsByUserIdAsync(999);
            result.Should().NotBeNull();
            result.Wins.Should().Be(0);
            result.UserId.Should().Be(999);
        }

        [Fact]
        public async Task GetTopWinsAsyncTestShouldReturnOrderedList()
        {
            await SeedStat(1, "Pro", "pro@example.com", 100);
            await SeedStat(2, "Avg", "avg@example.com", 50);
            await SeedStat(3, "Noob", "noob@example.com", 0);
            var top = await statisticsRepository.GetTopWinsAsync(10);
            top.Should().HaveCount(3);
            top[0].Wins.Should().Be(100);
            top[1].Wins.Should().Be(50);
            top[2].Wins.Should().Be(0);
        }

        [Fact]
        public async Task GetTopWinsAsyncTestShouldLimitResults()
        {
            await SeedStat(1, "A", "a@example.com", 10);
            await SeedStat(2, "B", "b@example.com", 20);
            await SeedStat(3, "C", "c@example.com", 30);
            var top = await statisticsRepository.GetTopWinsAsync(2);
            top.Should().HaveCount(2);
            top.First().Wins.Should().Be(30);
        }

        [Fact]
        public async Task GetTopWinsAsyncTestShouldIncludeUserData()
        {
            await SeedStat(1, "PlayerName", "player@example.com", 10);
            var top = await statisticsRepository.GetTopWinsAsync(1);
            top.First().User?.Username.Should().Be("PlayerName");
        }

        [Fact]
        public async Task GetTopWinsAsyncTestShouldBreakTiesByPoints()
        {
            await SeedStat(1, "Winner", "winner@example.com", 10, 500);
            await SeedStat(2, "Loser", "loser@example.com", 10, 200);
            var top = await statisticsRepository.GetTopWinsAsync(2);
            top[0].User?.Username.Should().Be("Winner");
            top[1].User?.Username.Should().Be("Loser");
        }

        private async Task SeedStat(int id, string name, string email, int wins, int points = 0)
        {
            using var context = dbFactory.CreateDbContext();
            context.Users.Add(new User 
            { 
                Id = id, 
                Username = name, 
                Email = email, 
                PasswordHash = "x", 
                FullName = "x" 
            });
            context.PlayerStatistics.Add(new PlayerStatistics 
            {
                UserId = id, 
                Wins = wins, 
                GlobalPoints = points 
            });
            await context.SaveChangesAsync();
        }

        private async Task CreateUserForTest(int userId)
        {
            using var context = dbFactory.CreateDbContext();
            if (!context.Users.Any(u => u.Id == userId))
            {
                context.Users.Add(new User
                {
                    Id = userId,
                    Username = $"User{userId}",
                    Email = $"user{userId}@test.com",
                    PasswordHash = "dummy",
                    FullName = "Test"
                });
                await context.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            dbFactory.Dispose();
        }
    }
}
