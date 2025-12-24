using System.ServiceModel;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract]
    public interface ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnSessionStarted(string sessionId);

        [OperationContract(IsOneWay = true)]
        void OnSessionEnded(string sessionId);
    }
}
