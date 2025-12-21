using ForestTails.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public class CosmeticRepository : BaseRepository<Cosmetic>, ICosmeticRepository
    {
        public CosmeticRepository(IDbContextFactory<ForestTailsDbContext> contextFactory,
        ILogger<CosmeticRepository> logger) : base(contextFactory, logger) {}

        public async Task<Cosmetic> GetCosmeticByIdAsync(int cosmeticId)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var item = await context.Cosmetics.FindAsync(cosmeticId);
                return item ?? new Cosmetic();
            },
            nameof(GetCosmeticByIdAsync));
        }

        public async Task<List<Cosmetic>> GetCosmeticsCatalogAsync(bool onlyActive = true)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var query = context.Cosmetics.AsNoTracking();
                if (onlyActive) query = query.Where(c => c.IsActive);

                var list = await query.ToListAsync();
                return list ?? new List<Cosmetic>();
            },
            nameof(GetCosmeticsCatalogAsync));
        }

        public async Task<List<UnlockedCosmetic>> GetUnlockedCosmeticsByUserAsync(int userId)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var list = await context.UnlockedCosmetics
                    .AsNoTracking()
                    .Include(u => u.Cosmetic)
                    .Where(u => u.UserId == userId)
                    .ToListAsync();
                return list ?? new List<UnlockedCosmetic>();
            },
            nameof(GetUnlockedCosmeticsByUserAsync));
        }

        public async Task<bool> IsCosmeticUnlockedAsync(int userId, int cosmeticId)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                return await context.UnlockedCosmetics
                    .AnyAsync(u => u.UserId == userId && u.CosmeticId == cosmeticId);
            },
            nameof(IsCosmeticUnlockedAsync));
        }

        public async Task UnlockForUserAsync(int userId, int cosmeticId)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                var unlock = new UnlockedCosmetic
                {
                    UserId = userId,
                    CosmeticId = cosmeticId,
                    UnlockedAt = DateTime.UtcNow
                };
                await context.UnlockedCosmetics.AddAsync(unlock);
                await context.SaveChangesAsync();
            },
            nameof(UnlockForUserAsync));
        }
    }
}
