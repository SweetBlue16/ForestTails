using System.ServiceModel;
using ForestTails.Shared.Dtos;
using ForestTails.Shared.Models;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IAuthCallback), SessionMode = SessionMode.Required)]
    public interface IAuthService
    {
        [OperationContract]
        Task<ServiceResponse<UserDTO>> LoginAsync(LoginRequestDTO request);

        [OperationContract]
        Task<ServiceResponse<UserDTO>> RegisterAsync(LoginRequestDTO request);

        [OperationContract]
        Task<ServiceResponse<bool>> LogoutAsync();
    }

    [ServiceContract]
    public interface IAuthCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnForceLogout(string reason);
    }
}
