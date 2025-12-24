using ForestTails.Shared.Dtos;
using ForestTails.Shared.Models;
using System.ServiceModel;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IReportCallback), SessionMode = SessionMode.Required)]
    public interface IReportService
    {
        [OperationContract]
        Task SubmitReportAsync(ReportRequestDTO report);
    }

    [ServiceContract]
    public interface IReportCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnReportSubmittedResult(ServiceResponse<ReportResultDTO> response);
    }
}
