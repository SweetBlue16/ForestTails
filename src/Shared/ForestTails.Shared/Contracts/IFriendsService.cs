using ForestTails.Shared.Dtos;
using ForestTails.Shared.Models;
using System.ServiceModel;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IFriendsCallback), SessionMode = SessionMode.Required)]
    public interface IFriendsService
    {
        [OperationContract]
        Task RequestFriendsListAsync();

        [OperationContract]
        Task SendFriendRequestAsync(string targetUsername);

        [OperationContract]
        Task RespondToRequestAsync(int requesterId, bool accept);

        [OperationContract]
        Task RemoveFriendAsync(int friendId);
    }

    [ServiceContract]
    public interface IFriendsCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnFriendsListReceived(ServiceResponse<List<FriendDTO>> response);

        [OperationContract(IsOneWay = true)]
        void OnFriendRequestSentResult(ServiceResponse<bool> response);

        [OperationContract(IsOneWay = true)]
        void OnFriendResponseResult(ServiceResponse<bool> response);

        [OperationContract(IsOneWay = true)]
        void OnRemoveFriendResult(ServiceResponse<bool> response);

        [OperationContract(IsOneWay = true)]
        void OnFriendRequestReceived(ServiceResponse<FriendDTO> request);

        [OperationContract(IsOneWay = true)]
        void OnFriendStatusChanged(ServiceResponse<FriendStatusUpdateDTO> status);

        [OperationContract(IsOneWay = true)]
        void OnFriendRequestResponse(ServiceResponse<FriendRequestResponseDTO> response);
    }
}
