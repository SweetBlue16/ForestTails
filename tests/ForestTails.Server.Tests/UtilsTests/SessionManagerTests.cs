using CoreWCF;
using FluentAssertions;
using ForestTails.Server.Logic.Utils;
using ForestTails.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForestTails.Server.Tests.UtilsTests
{
    public class SessionManagerTests
    {
        private readonly SessionManager sessionManager;
        private readonly Mock<ISessionCallback> callbackMock;

        public SessionManagerTests()
        {
            var loggerMock = new Mock<ILogger<SessionManager>>();
            sessionManager = new SessionManager(loggerMock.Object);
            callbackMock = new Mock<ISessionCallback>();
            var channelMock = callbackMock.As<ICommunicationObject>();
            channelMock.Setup(c => c.State).Returns(CommunicationState.Opened);
        }

        [Fact]
        public void AddSessionTestShouldAddUserWhenValid()
        {
            sessionManager.AddSession("User1", callbackMock.Object);
            sessionManager.IsOnline("User1").Should().BeTrue();
        }

        [Fact]
        public void AddSessionTestShouldIgnoreWhenUsernameIsEmpty()
        {
            sessionManager.AddSession("", callbackMock.Object);
            sessionManager.GetOnlineUsers().Should().BeEmpty();
        }

        [Fact]
        public void AddSessionTestShouldIgnoreWhenCallbackIsNull()
        {
            sessionManager.AddSession("User1", null!);
            sessionManager.IsOnline("User1").Should().BeFalse();
        }

        [Fact]
        public void AddSessionTestShouldReplaceWhenUserAlreadyLogged()
        {
            var cb1 = CreateActiveCallback();
            var cb2 = CreateActiveCallback();
            sessionManager.AddSession("User1", cb1);
            sessionManager.AddSession("User1", cb2);
            var current = sessionManager.GetCallback<ISessionCallback>("User1");
            current.Should().NotBeNull();
            current.Should().Be(cb2);
            sessionManager.GetOnlineUsers().Should().HaveCount(1);
        }

        [Fact]
        public void RemoveSessionTestShouldRemoveUser()
        {
            sessionManager.AddSession("User1", callbackMock.Object);
            sessionManager.RemoveSession("User1");
            sessionManager.IsOnline("User1").Should().BeFalse();
        }

        [Fact]
        public void RemoveSessionTestShouldDoNothingIfUserNotExists()
        {
            sessionManager.AddSession("User1", callbackMock.Object);
            sessionManager.RemoveSession("User2");
            sessionManager.IsOnline("User1").Should().BeTrue();
        }

        [Fact]
        public void GetCallbackTestShouldReturnNullIfUserOffline()
        {
            var cb = sessionManager.GetCallback<ISessionCallback>("Ghost");
            cb.Should().BeNull();
        }

        [Fact]
        public void GetCallbackTestShouldReturnNullIfTypeMismatch()
        {
            sessionManager.AddSession("User1", callbackMock.Object);
            var result = sessionManager.GetCallback<IAuthCallback>("User1");
            result.Should().BeNull();
        }

        [Fact]
        public void IsOnlineTestShouldReturnFalseForUnknownUser()
        {
            sessionManager.IsOnline("Unknown").Should().BeFalse();
        }

        [Fact]
        public void GetOnlineUsersTestShouldReturnAllKeys()
        {
            sessionManager.AddSession("A", callbackMock.Object);
            sessionManager.AddSession("B", callbackMock.Object);
            var list = sessionManager.GetOnlineUsers();
            list.Should().Contain("A");
            list.Should().Contain("B");
            list.Should().HaveCount(2);
        }

        private ISessionCallback CreateActiveCallback()
        {
            var mock = new Mock<ISessionCallback>();
            var channel = mock.As<ICommunicationObject>();
            channel.Setup(c => c.State).Returns(CommunicationState.Opened);
            return mock.Object;
        }
    }
}
