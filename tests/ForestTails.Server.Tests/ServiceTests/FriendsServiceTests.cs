using CoreWCF;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Services;
using ForestTails.Server.Logic.Utils;
using ForestTails.Server.Logic.Validators;
using ForestTails.Shared.Contracts;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Enums;
using ForestTails.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.ServiceTests
{
    public class FriendsServiceTests
    {
        private readonly Mock<IFriendRepository> friendRepositoryMock = new();
        private readonly Mock<IUserRepository> userRepositoryMock = new();
        private readonly Mock<IFriendsValidator> validatorMock = new();

        private readonly SessionManager sessionManager;
        private readonly Mock<CallbackExecutor> callbackExecutor = new(new Mock<ILogger<CallbackExecutor>>().Object);
        private readonly ServiceExecutor serviceExecutor;
        private readonly Mock<ILogger<FriendsService>> loggerMock = new();

        private readonly Mock<IFriendsCallback> myCallback;
        private readonly Mock<IFriendsCallback> targetCallback;

        private readonly FriendsService service;
        private readonly User me = new() { Id = 1, Username = "Me" };

        public FriendsServiceTests()
        {
            var logger = new Mock<ILogger<ServiceExecutor>>().Object;
            serviceExecutor = new ServiceExecutor(logger);
            sessionManager = new SessionManager(new Mock<ILogger<SessionManager>>().Object);
            myCallback = CreateActiveCallbackMock();
            targetCallback = CreateActiveCallbackMock();
            callbackExecutor.Setup(x => x.Execute(It.IsAny<Action<IFriendsCallback>>()))
                .Callback<Action<IFriendsCallback>>(act => act(myCallback.Object));
            service = new FriendsService(
                friendRepositoryMock.Object,
                userRepositoryMock.Object,
                sessionManager,
                serviceExecutor,
                callbackExecutor.Object,
                validatorMock.Object,
                loggerMock.Object
            );
            service.SetCurrentUser(me);
        }

        [Fact]
        public async Task SendRequestTestShouldNotifyTargetIfOnline()
        {
            var target = new User { Id = 2, Username = "Target" };
            userRepositoryMock.Setup(x => x.GetByUsernameAsync("Target")).ReturnsAsync(target);
            sessionManager.AddSession("Target", targetCallback.Object);
            await service.SendFriendRequestAsync("Target");
            friendRepositoryMock.Verify(x => x.SendRequestAsync(1, 2), Times.Once);
            callbackExecutor.Verify(x => x.Execute(It.IsAny<Action<IFriendsCallback>>()), Times.Once);
            await Task.Delay(100);
            targetCallback.Verify(x => x.OnFriendRequestReceived(It.IsAny<ServiceResponse<FriendDTO>>()),
                Times.Once
            );
        }

        [Fact]
        public async Task SendRequestTestShouldFailIfUserNotFound()
        {
            userRepositoryMock.Setup(x => x.GetByUsernameAsync("Ghost")).ReturnsAsync(User.Empty);
            await service.SendFriendRequestAsync("Ghost");
            friendRepositoryMock.Verify(x => x.SendRequestAsync(It.IsAny<int>(), It.IsAny<int>()), 
                Times.Never
            );
        }

        [Fact]
        public async Task SendRequestTestShouldFailIfValidatorThrows()
        {
            validatorMock.Setup(x => x.ValidateTargetUsername(It.IsAny<string>())).Throws(
                new ValidationException("Error")
            );
            await service.SendFriendRequestAsync("Bad");
            userRepositoryMock.Verify(x => x.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RequestListTestShouldMarkOnlineUsers()
        {
            var friend = new User { Id = 2, Username = "FriendOnline" };
            var list = new List<Friendship> 
            { 
                new Friendship
                {
                    RequesterId = 1, 
                    Addressee = friend,
                    Status = FriendRequestStatus.Accepted 
                } 
            };
            friendRepositoryMock.Setup(x => x.GetFriendshipsAsync(1, FriendRequestStatus.Accepted))
                .ReturnsAsync(list);
            sessionManager.AddSession("FriendOnline", targetCallback.Object);
            await service.RequestFriendsListAsync();
            myCallback.Verify(cb => cb.OnFriendsListReceived(It.Is<ServiceResponse<List<FriendDTO>>>(
                r => r.Data != null && r.Data.Count > 0 &&
                     r.Data[0].IsOnline && r.Data[0].Username == "FriendOnline"
            )), Times.Once);
        }
        
        [Fact]
        public async Task RespondRequestTestShouldAcceptAndNotifyRequester()
        {
            var requester = new User { Id = 2, Username = "Requester" };
            friendRepositoryMock.Setup(x => x.GetStatusAsync(2, 1))
                .ReturnsAsync(FriendRequestStatus.Pending);
            userRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(requester);
            sessionManager.AddSession("Requester", targetCallback.Object);
            await service.RespondToRequestAsync(2, true);
            friendRepositoryMock.Verify(x => x.UpdateStatusAsync(2, 1, FriendRequestStatus.Accepted), 
                Times.Once
            );
            await Task.Delay(100);
            targetCallback.Verify(x => x.OnFriendRequestResponse(It.Is<ServiceResponse<FriendRequestResponseDTO>>(
                r => r.Data != null && r.Data.WasAccepted == true
            )), Times.Once);
        }

        [Fact]
        public async Task RespondRequestTestShouldRejectAndDeleteRelation()
        {
            friendRepositoryMock.Setup(x => x.GetStatusAsync(2, 1)).ReturnsAsync(FriendRequestStatus.Pending);
            await service.RespondToRequestAsync(2, false);
            friendRepositoryMock.Verify(x => x.RemoveFriendshipAsync(2, 1), Times.Once);
        }

        [Fact]
        public async Task RespondRequestTestShouldFailIfNoRequestPending()
        {
            friendRepositoryMock.Setup(x => x.GetStatusAsync(2, 1)).ReturnsAsync((FriendRequestStatus?)null);
            await service.RespondToRequestAsync(2, true);
            friendRepositoryMock.Verify(x => x.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<int>(), 
                It.IsAny<FriendRequestStatus>()), Times.Never
            );
        }

        [Fact]
        public async Task RemoveFriendTestShouldCallRepo()
        {
            await service.RemoveFriendAsync(5);
            friendRepositoryMock.Verify(x => x.RemoveFriendshipAsync(1, 5), Times.Once);
        }

        [Fact]
        public async Task ServiceTestShouldThrowAuthExceptionIfSessionNotSet()
        {
            service.SetCurrentUser(null!);
            await Assert.ThrowsAsync<AuthException>(async () =>
            {
                await service.RequestFriendsListAsync();
            });
        }

        private static Mock<IFriendsCallback> CreateActiveCallbackMock()
        {
            var mock = new Mock<IFriendsCallback>();
            var channel = mock.As<ICommunicationObject>();
            channel.Setup(c => c.State).Returns(CommunicationState.Opened);
            return mock;
        }
    }
}
