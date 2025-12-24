using ForestTails.Shared.Dtos;
using ForestTails.Shared.Models;
using System.ServiceModel;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IProfileCallback), SessionMode = SessionMode.Required)]
    public interface IProfileService
    {
        [OperationContract]
        Task RequestMyProfileAsync();

        [OperationContract]
        Task UpdateProfileDetailsAsync(ProfileUpdateRequestDTO request);

        [OperationContract]
        Task UpdateAvatarAsync(int cosmeticId);

        [OperationContract]
        Task ChangePasswordAsync(string currentPassword, string newPassword);

        [OperationContract]
        Task RequestUserProfileAsync(string username);

        [OperationContract]
        Task RequestUserProfileByIdAsync(int userId);
    }

    [ServiceContract]
    public interface IProfileCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMyProfileReceived(ServiceResponse<UserDTO> response);

        [OperationContract(IsOneWay = true)]
        void OnUserProfileReceived(ServiceResponse<PublicProfileDTO> response);

        [OperationContract(IsOneWay = true)]
        void OnProfileUpdateResult(ServiceResponse<bool> response);

        [OperationContract(IsOneWay = true)]
        void OnAvatarUpdateResult(ServiceResponse<bool> response);

        [OperationContract(IsOneWay = true)]
        void OnPasswordChangeResult(ServiceResponse<bool> response);

        [OperationContract(IsOneWay = true)]
        void OnCurrencyUpdated(ServiceResponse<int> newBalance);
    }
}
