using ForestTails.Server.Data.Entities;

namespace ForestTails.Server.Data.Repositories
{
    public interface ICosmeticRepository
    {
        Task<List<Cosmetic>> GetCosmeticsCatalogAsync(bool onlyActive = true);
        Task<Cosmetic> GetCosmeticByIdAsync(int cosmeticId);
        Task<List<UnlockedCosmetic>> GetUnlockedCosmeticsByUserAsync(int userId);
        Task UnlockForUserAsync(int userId, int cosmeticId);
        Task<bool> IsCosmeticUnlockedAsync(int userId, int cosmeticId);
    }
}
