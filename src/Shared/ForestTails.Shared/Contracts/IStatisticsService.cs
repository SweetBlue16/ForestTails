using ForestTails.Shared.Dtos;
using ForestTails.Shared.Models;
using System.ServiceModel;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IStatisticsCallback), SessionMode = SessionMode.Required)]
    public interface IStatisticsService
    {
        [OperationContract]
        Task RequestTopPlayersAsync();

        [OperationContract]
        Task RequestPlayerStatsAsync(string username);
    }

    [ServiceContract]
    public interface IStatisticsCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnTopPlayersReceived(ServiceResponse<List<LeaderboardDTO>> response);

        [OperationContract(IsOneWay = true)]
        void OnPlayerStatsReceived(ServiceResponse<UserDTO> response);
    }
}
