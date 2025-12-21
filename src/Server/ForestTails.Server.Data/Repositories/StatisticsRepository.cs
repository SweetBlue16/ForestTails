using ForestTails.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public interface IStatisticsRepository
    {
        Task<PlayerStatistics> GetStatisticsByUserIdAsync(int userId);
        Task UpdateStatisticsAsync(PlayerStatistics statistics);
        Task<List<PlayerStatistics>> GetTopWinsAsync(int count);
    }

    public class StatisticsRepository : BaseRepository<PlayerStatistics>, IStatisticsRepository
    {
        public StatisticsRepository(IDbContextFactory<ForestTailsDbContext> contextFactory,
        ILogger<StatisticsRepository> logger) : base(contextFactory, logger) {}

        public async Task<PlayerStatistics> GetStatisticsByUserIdAsync(int userId)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var stats = await context.PlayerStatistics
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == userId);
                return stats ?? new PlayerStatistics { UserId = userId };
            },
            nameof(GetStatisticsByUserIdAsync));
        }

        public async Task<List<PlayerStatistics>> GetTopWinsAsync(int count)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var list = await context.PlayerStatistics
                    .AsNoTracking()
                    .Include(s => s.User)
                    .OrderByDescending(s => s.Wins)
                    .ThenByDescending(s => s.GlobalPoints)
                    .Take(count)
                    .ToListAsync();
                return list ?? new List<PlayerStatistics>();
            },
            nameof(GetTopWinsAsync));
        }

        public async Task UpdateStatisticsAsync(PlayerStatistics statistics)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                var exists = await context.PlayerStatistics.AnyAsync(s => s.UserId == statistics.UserId);
                if (exists)
                {
                    context.PlayerStatistics.Update(statistics);
                }
                else
                {
                    await context.PlayerStatistics.AddAsync(statistics);
                }
                await context.SaveChangesAsync();
            },
            nameof(UpdateStatisticsAsync));
        }
    }
}
