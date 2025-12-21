using ForestTails.Server.Data.Entities;

namespace ForestTails.Server.Data.Repositories
{
    public interface IReportRepository
    {
        Task CreateReportAsync(Report report);
        Task<List<Report>> GetPendingReportsAsync();
        Task MarkAsResolvedAsync(int reportId);
    }
}
