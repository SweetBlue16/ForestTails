using System.ServiceModel;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Models;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IAuthCallback), SessionMode = SessionMode.Required)]
    public interface IAuthService
    {
        [OperationContract]
        Task LoginAsync(LoginRequestDTO request);

        [OperationContract]
        Task RegisterAsync(RegisterRequestDTO request);

        [OperationContract]
        Task LogoutAsync();
    }

    [ServiceContract]
    public interface IAuthCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLoginResult(ServiceResponse<UserDTO> response);

        [OperationContract(IsOneWay = true)]
        void OnRegisterResult(ServiceResponse<UserDTO> response);

        [OperationContract(IsOneWay = true)]
        void OnLogoutResult(ServiceResponse<bool> response);

        [OperationContract(IsOneWay = true)]
        void OnForceLogout(ServiceResponse<string> reason);
    }
}
