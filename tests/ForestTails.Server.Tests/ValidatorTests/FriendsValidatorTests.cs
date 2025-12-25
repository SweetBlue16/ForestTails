using FluentAssertions;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Validators;
using ForestTails.Shared.Enums;

namespace ForestTails.Server.Tests.ValidatorTests
{
    public class FriendsValidatorTests
    {
        private readonly FriendsValidator friendsValidator = new();

        [Fact]
        public void ValidateTargetUsernameTestShouldPassWhenValid()
        {
            Action act = () => friendsValidator.ValidateTargetUsername("ValidUser");
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidateTargetUsernameTestShouldThrowWhenEmpty(string username)
        {
            Action act = () => friendsValidator.ValidateTargetUsername(username);
            act.Should().Throw<ValidationException>().Where(e => e.Code == MessageCode.MissingRequiredField);
        }

        [Fact]
        public void ValidateNotSelfTestShouldPassWhenIdsDiffer()
        {
            Action act = () => friendsValidator.ValidateNotSelf(1, 2);
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateNotSelfTestShouldThrowWhenIdsAreEqual()
        {
            Action act = () => friendsValidator.ValidateNotSelf(10, 10);
            act.Should().Throw<ValidationException>().Where(e => e.Code == MessageCode.Conflict);
        }

        [Fact]
        public void ValidateStateTestShouldPassWhenStatusIsNull()
        {
            Action act = () => friendsValidator.ValidateRelationshipStateFromRequest(null);
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateStateTestShouldThrowWhenAlreadyFriends()
        {
            Action act = () => friendsValidator.ValidateRelationshipStateFromRequest(FriendRequestStatus.Accepted);
            act.Should().Throw<ConflictException>().Where(e => e.Code == MessageCode.AlreadyFriends);
        }

        [Fact]
        public void ValidateStateTestShouldThrowWhenPending()
        {
            Action act = () => friendsValidator.ValidateRelationshipStateFromRequest(FriendRequestStatus.Pending);
            act.Should().Throw<ConflictException>().Where(e => e.Code == MessageCode.FriendRequestAlreadySent);
        }

        [Fact]
        public void ValidateStateTestShouldThrowWhenBlocked()
        {
            Action act = () => friendsValidator.ValidateRelationshipStateFromRequest(FriendRequestStatus.Blocked);
            act.Should().Throw<ConflictException>().Where(e => e.Code == MessageCode.UserBlocked);
        }
    }
}
