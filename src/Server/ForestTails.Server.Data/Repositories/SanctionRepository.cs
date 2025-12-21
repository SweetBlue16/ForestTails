using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public interface ISanctionRepository
    {
        Task<Sanction?> GetActiveBanAsync(int userId);
        Task ApplySanctionAsync(Sanction sanction);
    }

    public class SanctionRepository : BaseRepository<Sanction>, ISanctionRepository
    {
        public SanctionRepository(IDbContextFactory<ForestTailsDbContext> contextFactory,
        ILogger<SanctionRepository> logger) : base(contextFactory, logger) {}

        public async Task ApplySanctionAsync(Sanction sanction)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                await context.Sanctions.AddAsync(sanction);
                await context.SaveChangesAsync();
            },
            nameof(ApplySanctionAsync));
        }

        public async Task<Sanction?> GetActiveBanAsync(int userId)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                return await context.Sanctions
                    .AsNoTracking()
                    .Where(s => s.UserId == userId &&
                                (s.Type == SanctionType.PermanentBan || s.Type == SanctionType.TemporaryBan))
                    .Where(s => s.EndDate == null || s.EndDate > DateTime.UtcNow)
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefaultAsync();
            },
            nameof(GetActiveBanAsync));
        }
    }
}
