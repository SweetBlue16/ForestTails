using ForestTails.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public interface IReportRepository
    {
        Task CreateReportAsync(Report report);
        Task<List<Report>> GetPendingReportsAsync();
        Task MarkAsResolvedAsync(int reportId);
    }

    public class ReportRepository : BaseRepository<Report>, IReportRepository
    {
        public ReportRepository(IDbContextFactory<ForestTailsDbContext> contextFactory,
        ILogger<ReportRepository> logger) : base(contextFactory, logger) {}

        public async Task CreateReportAsync(Report report)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                report.CreatedAt = DateTime.UtcNow;
                report.IsResolved = false;

                await context.AddAsync(report);
                await context.SaveChangesAsync();
            },
            nameof(CreateReportAsync));
        }

        public async Task<List<Report>> GetPendingReportsAsync()
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var list = await context.Reports
                    .AsNoTracking()
                    .Include(r => r.Reporter)
                    .Include(r => r.Reported)
                    .Where(r => !r.IsResolved)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();
                return list ?? new List<Report>();
            },
            nameof(GetPendingReportsAsync));
        }

        public async Task MarkAsResolvedAsync(int reportId)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                await context.Reports
                    .Where(r => r.Id == reportId)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsResolved, true));
            },
            nameof(MarkAsResolvedAsync));
        }
    }
}
