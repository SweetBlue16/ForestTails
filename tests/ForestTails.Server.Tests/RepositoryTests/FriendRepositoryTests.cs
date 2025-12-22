using FluentAssertions;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Tests.Common;
using ForestTails.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.RepositoryTests
{
    public class FriendRepositoryTests : IDisposable
    {
        private readonly TestDbFactory dbFactory;
        private readonly FriendRepository friendRepository;

        public FriendRepositoryTests()
        {
            dbFactory = new TestDbFactory();
            var loggerMock = new Mock<ILogger<FriendRepository>>();
            friendRepository = new FriendRepository(dbFactory, loggerMock.Object);
        }

        [Fact]
        public async Task SendRequestAsyncTestShouldCreatePendingRequest()
        {
            await SetupUsers(1, 2);
            await friendRepository.SendRequestAsync(1, 2);
            var status = await friendRepository.GetStatusAsync(1, 2);
            status.Should().Be(FriendRequestStatus.Pending);
        }

        [Fact]
        public async Task GetStatusAsyncTestShouldReturnNullWhenNoRelationship()
        {
            await SetupUsers(1, 2);
            var status = await friendRepository.GetStatusAsync(1, 2);
            status.Should().BeNull();
        }

        [Fact]
        public async Task GetStatusAsyncTestShouldWorkBidirectionally()
        {
            await SetupUsers(1, 2);
            await friendRepository.SendRequestAsync(1, 2);
            (await friendRepository.GetStatusAsync(1, 2)).Should().Be(FriendRequestStatus.Pending);
            (await friendRepository.GetStatusAsync(2, 1)).Should().Be(FriendRequestStatus.Pending);
        }

        [Fact]
        public async Task UpdateStatusAsyncTestShouldChangeStatusToAccepted()
        {
            await SetupUsers(1, 2);
            await friendRepository.SendRequestAsync(1, 2);
            await friendRepository.UpdateStatusAsync(1, 2, FriendRequestStatus.Accepted);
            var status = await friendRepository.GetStatusAsync(1, 2);
            status.Should().Be(FriendRequestStatus.Accepted);
        }

        [Fact]
        public async Task UpdateStatusAsyncTestShouldChangeStatusToBlocked()
        {
            await SetupUsers(1, 2);
            await friendRepository.SendRequestAsync(1, 2);
            await friendRepository.UpdateStatusAsync(1, 2, FriendRequestStatus.Blocked);
            (await friendRepository.GetStatusAsync(1, 2)).Should().Be(FriendRequestStatus.Blocked);
        }

        [Fact]
        public async Task GetFriendshipsAsyncTestShouldReturnOnlyAcceptedWhenRequested()
        {
            await SetupUsers(1, 2);
            await SetupUsers(1, 3);
            await friendRepository.SendRequestAsync(1, 2);
            await friendRepository.SendRequestAsync(3, 1);
            await friendRepository.UpdateStatusAsync(3, 1, FriendRequestStatus.Accepted);
            var friends = await friendRepository.GetFriendshipsAsync(1, FriendRequestStatus.Accepted);
            friends.Should().HaveCount(1);
            friends.First().RequesterId.Should().Be(3);
        }

        [Fact]
        public async Task GetFriendshipsAsyncTestShouldReturnPendingIncomingAndOutgoing()
        {
            await SetupUsers(1, 2);
            await friendRepository.SendRequestAsync(1, 2);
            var pending = await friendRepository.GetFriendshipsAsync(1, FriendRequestStatus.Pending);
            pending.Should().HaveCount(1);
        }

        [Fact]
        public async Task RemoveFriendshipAsyncTestShouldDeleteRecord()
        {
            await SetupUsers(1, 2);
            await friendRepository.SendRequestAsync(1, 2);
            await friendRepository.UpdateStatusAsync(1, 2, FriendRequestStatus.Accepted);
            await friendRepository.RemoveFriendshipAsync(1, 2);
            var status = await friendRepository.GetStatusAsync(1, 2);
            status.Should().BeNull();
        }

        [Fact]
        public async Task RemoveFriendshipAsyncTestShouldNotFailIfRelationshipDoesNotExist()
        {
            await SetupUsers(1, 2);
            Func<Task> act = async () => await friendRepository.RemoveFriendshipAsync(1, 2);
            await act.Should().NotThrowAsync();
        }

        private async Task SetupUsers(int id1, int id2)
        {
            using var context = dbFactory.CreateDbContext();
            if (!context.Users.Any(u => u.Id == id1))
            {
                context.Users.Add(new User
                {
                    Id = id1,
                    Username = $"U{id1}",
                    Email = $"{id1}",
                    PasswordHash = ".",
                    FullName = "."
                });
            }
            if (!context.Users.Any(u => u.Id == id2))
            {
                context.Users.Add(new User
                {
                    Id = id2,
                    Username = $"U{id2}",
                    Email = $"{id2}",
                    PasswordHash = ".",
                    FullName = "."
                });
            }
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            dbFactory.Dispose();
        }
    }
}
