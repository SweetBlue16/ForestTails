using CoreWCF;
using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Repositories;
using ForestTails.Server.Logic.Exceptions;
using ForestTails.Server.Logic.Utils;
using ForestTails.Server.Logic.Validators;
using ForestTails.Shared.Contracts;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Enums;
using ForestTails.Shared.Models;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Logic.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FriendsService : IFriendsService
    {
        private readonly IFriendRepository friendRepository;
        private readonly IUserRepository userRepository;
        private readonly SessionManager sessionManager;
        private readonly ServiceExecutor serviceExecutor;
        private readonly CallbackExecutor callbackExecutor;
        private readonly IFriendsValidator friendsValidator;
        private readonly ILogger<FriendsService> logger;

        private User? currentUser;

        public FriendsService(
            IFriendRepository friendRepository,
            IUserRepository userRepository,
            SessionManager sessionManager,
            ServiceExecutor serviceExecutor,
            CallbackExecutor callbackExecutor,
            IFriendsValidator friendsValidator,
            ILogger<FriendsService> logger)
        {
            this.friendRepository = friendRepository;
            this.userRepository = userRepository;
            this.sessionManager = sessionManager;
            this.serviceExecutor = serviceExecutor;
            this.callbackExecutor = callbackExecutor;
            this.friendsValidator = friendsValidator;
            this.logger = logger;
        }

        public void SetCurrentUser(User user)
        {
            currentUser = user;
        }

        public async Task RemoveFriendAsync(int friendId)
        {
            if (currentUser == null) throw new AuthException("User is not authenticated", MessageCode.SessionExpired);

            var response = await serviceExecutor.ExecuteSafeAsync(async () =>
            {
                await friendRepository.RemoveFriendshipAsync(currentUser.Id, friendId);
                return true;
            },
            nameof(RemoveFriendAsync));
            callbackExecutor.Execute<IFriendsCallback>(callback => callback.OnRemoveFriendResult(response));
        }

        public async Task RequestFriendsListAsync()
        {
            if (currentUser == null) throw new AuthException("User is not authenticated", MessageCode.SessionExpired);

            var response = await serviceExecutor.ExecuteSafeAsync(async () =>
            {
                var friends = await friendRepository.GetFriendshipsAsync(currentUser.Id, FriendRequestStatus.Accepted);
                return friends.Select(f =>
                {
                    var isRequester = f.RequesterId == currentUser.Id;
                    var friendUser = isRequester ? f.Addressee : f.Requester;

                    if (friendUser == null || friendUser.IsEmpty)
                        throw new NotFoundException("Friend user not found", MessageCode.FriendNotFound);

                    return new FriendDTO
                    {
                        Id = friendUser.Id,
                        Username = friendUser.Username,
                        SelectedAvatarId = friendUser.SelectedAvatarId,
                        IsOnline = sessionManager.IsOnline(friendUser.Username),
                        Status = FriendRequestStatus.Accepted
                    };
                }).ToList();
            },
            nameof(RequestFriendsListAsync));
            callbackExecutor.Execute<IFriendsCallback>(callback => callback.OnFriendsListReceived(response));
        }

        public async Task RespondToRequestAsync(int requesterId, bool accept)
        {
            if (currentUser == null) throw new AuthException("User is not authenticated", MessageCode.SessionExpired);

            var response = await serviceExecutor.ExecuteSafeAsync(async () =>
            {
                var status = await friendRepository.GetStatusAsync(requesterId, currentUser.Id);
                if (status != FriendRequestStatus.Pending)
                    throw new NotFoundException("Request does not exists", MessageCode.NotFound);

                if (accept)
                    await friendRepository.UpdateStatusAsync(requesterId, currentUser.Id, FriendRequestStatus.Accepted);
                else
                    await friendRepository.RemoveFriendshipAsync(requesterId, currentUser.Id);

                var requester = await userRepository.GetByIdAsync(requesterId);
                if (requester != null)
                {
                    var requesterCallback = sessionManager.GetCallback<IFriendsCallback>(requester.Username);
                    if (requesterCallback != null)
                    {
                        await Task.Run(async () => NotifyFriendRequestResponseAsync(requester.Username, requesterCallback, accept));
                    }
                }
                return true;
            },
            nameof(RespondToRequestAsync));
            callbackExecutor.Execute<IFriendsCallback>(callback => callback.OnFriendResponseResult(response));
        }

        public async Task SendFriendRequestAsync(string targetUsername)
        {
            if (currentUser == null) throw new AuthException("User is not authenticated", MessageCode.SessionExpired);

            var response = await serviceExecutor.ExecuteSafeAsync(async () =>
            {
                friendsValidator.ValidateTargetUsername(targetUsername);
                var targetUser = await userRepository.GetByUsernameAsync(targetUsername);
                if (targetUser == null || targetUser.IsEmpty)
                    throw new NotFoundException("Target user not found", MessageCode.NotFound);

                friendsValidator.ValidateNotSelf(currentUser.Id, targetUser.Id);

                var existingStatus = await friendRepository.GetStatusAsync(currentUser.Id, targetUser.Id);
                friendsValidator.ValidateRelationshipStateFromRequest(existingStatus);

                await friendRepository.SendRequestAsync(currentUser.Id, targetUser.Id);

                var targetCallback = sessionManager.GetCallback<IFriendsCallback>(targetUser.Username);
                if (targetCallback != null)
                {
                    await Task.Run(async () => await NotifyFriendRequestReceivedAsync(targetUser, targetCallback));
                }
                return true;
            },
            nameof(SendFriendRequestAsync));
            callbackExecutor.Execute<IFriendsCallback>(callback => callback.OnFriendRequestSentResult(response));
        }

        private async Task NotifyFriendRequestReceivedAsync(User targetUser, IFriendsCallback targetCallback)
        {
            if (currentUser == null)
            {
                logger.LogWarning("Attempted to notify friend request without an authenticated current user.");
                return;
            }

            try
            {
                logger.LogDebug("Notifying user {TargetUsername} about a new friend request from {CurrentUsername}.",
                    targetUser.Username, currentUser.Username
                );

                targetCallback.OnFriendRequestReceived(new ServiceResponse<FriendDTO>
                {
                    IsSuccess = true,
                    Data = new FriendDTO
                    {
                        Id = currentUser.Id,
                        Username = currentUser.Username,
                        Status = FriendRequestStatus.Pending
                    }
                });
                logger.LogInformation("Notification delivered to {TargetUsername}.", targetUser.Username);
            }
            catch (CommunicationException commException)
            {
                logger.LogWarning(commException, "CommunicationException while notifying {TargetUsername}. Removing session.", targetUser.Username);
                sessionManager.RemoveSession(targetUser.Username);
            }
            catch (TimeoutException timeoutException)
            {
                logger.LogWarning(timeoutException, "TimeoutException while notifying {TargetUsername}. Removing session.", targetUser.Username);
                sessionManager.RemoveSession(targetUser.Username);
            }
            catch (ObjectDisposedException objDisposedException)
            {
                logger.LogDebug(objDisposedException, "Target callback channel for {TargetUsername} was disposed. Removing session.", targetUser.Username);
                sessionManager.RemoveSession(targetUser.Username);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                logger.LogWarning(invalidOperationException, "InvalidOperationException while notifying {TargetUsername}. Removing session.", targetUser.Username);
                sessionManager.RemoveSession(targetUser.Username);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unexpected error while notifying {TargetUsername}. Removing session.", targetUser.Username);
                sessionManager.RemoveSession(targetUser.Username);
            }
            await Task.CompletedTask;
        }

        private async Task NotifyFriendRequestResponseAsync(string username, IFriendsCallback requesterCallback, bool accept)
        {
            if (currentUser == null)
            {
                logger.LogWarning("Attempted to notify friend request without an authenticated current user.");
                return;
            }

            try
            {
                requesterCallback.OnFriendRequestResponse(new ServiceResponse<FriendRequestResponseDTO>
                {
                    IsSuccess = true,
                    Data = new FriendRequestResponseDTO
                    {
                        RequesterId = currentUser.Id,
                        RequesterUsername = currentUser.Username,
                        WasAccepted = accept
                    }
                });
            }
            catch (CommunicationException commException)
            {
                logger.LogWarning(commException, "CommunicationException while notifying {TargetUsername}. Removing session.", username);
                sessionManager.RemoveSession(username);
            }
            catch (TimeoutException timeoutException)
            {
                logger.LogWarning(timeoutException, "TimeoutException while notifying {TargetUsername}. Removing session.", username);
                sessionManager.RemoveSession(username);
            }
            catch (ObjectDisposedException objDisposedException)
            {
                logger.LogDebug(objDisposedException, "Target callback channel for {TargetUsername} was disposed. Removing session.", username);
                sessionManager.RemoveSession(username);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                logger.LogWarning(invalidOperationException, "InvalidOperationException while notifying {TargetUsername}. Removing session.", username);
                sessionManager.RemoveSession(username);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unexpected error while notifying {TargetUsername}. Removing session.", username);
                sessionManager.RemoveSession(username);
            }
            await Task.CompletedTask;
        }
    }
}
