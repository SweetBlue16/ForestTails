using ForestTails.Server.Logic.Exceptions;
using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Validators
{
    public interface IFriendsValidator
    {
        void ValidateTargetUsername(string targetUsername);
        void ValidateNotSelf(int currentUserId, int targetUserId);
        void ValidateRelationshipStateFromRequest(FriendRequestStatus? currentStatus);
    }

    public class FriendsValidator : IFriendsValidator
    {
        public void ValidateNotSelf(int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId)
                throw new ValidationException("Cannot send friend requests to yourself", MessageCode.Conflict);
        }

        public void ValidateRelationshipStateFromRequest(FriendRequestStatus? currentStatus)
        {
            if (currentStatus.HasValue)
            {
                switch (currentStatus.Value)
                {
                    case FriendRequestStatus.Accepted:
                        throw new ConflictException("You are already friends with this user", MessageCode.AlreadyFriends);
                    case FriendRequestStatus.Pending:
                        throw new ConflictException("A friend request is already pending with this user", MessageCode.FriendRequestAlreadySent);
                    case FriendRequestStatus.Blocked:
                        throw new ConflictException("You have blocked this user", MessageCode.UserBlocked);
                }
            }
        }

        public void ValidateTargetUsername(string targetUsername)
        {
            if (string.IsNullOrWhiteSpace(targetUsername))
                throw new ValidationException("Target username is required", MessageCode.MissingRequiredField);
        }
    }
}
