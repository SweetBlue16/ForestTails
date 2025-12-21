using ForestTails.Server.Data.Entities;

namespace ForestTails.Server.Data.Repositories
{
    public interface ISanctionRepository
    {
        Task<Sanction?> GetActiveBanAsync(int userId);
        Task ApplySanctionAsync(Sanction sanction);
    }
}
