using ForestTails.Server.Data.Entities;

namespace ForestTails.Server.Data.Repositories
{
    public interface IStatisticsRepository
    {
        Task<PlayerStatistics> GetStatisticsByUserIdAsync(int userId);
        Task UpdateStatisticsAsync(PlayerStatistics statistics);
        Task<List<PlayerStatistics>> GetTopWinsAsync(int count);
    }
}
