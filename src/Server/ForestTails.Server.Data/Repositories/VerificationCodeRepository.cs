using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public interface IVerificationCodeRepository
    {
        Task SaveCodeAsync(string email, string code, CodeType type);
        Task<bool> ValidateCodeAsync(string email, string code, CodeType type);
    }

    public class VerificationCodeRepository : BaseRepository<VerificationCode>, IVerificationCodeRepository
    {
        public VerificationCodeRepository(IDbContextFactory<ForestTailsDbContext> contextFactory,
        ILogger<VerificationCodeRepository> logger) : base(contextFactory, logger) {}

        public async Task SaveCodeAsync(string email, string code, CodeType type)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                var oldCodes = await context.VerificationCodes
                    .Where(x => x.Email == email && x.Type == type)
                    .ToListAsync();
                context.VerificationCodes.RemoveRange(oldCodes);

                var newCode = new VerificationCode
                {
                    Email = email,
                    Code = code,
                    Type = type,
                    ExpirationDate = DateTime.UtcNow.AddMinutes(15)
                };
                await context.VerificationCodes.AddAsync(newCode);
                await context.SaveChangesAsync();
            },
            nameof(SaveCodeAsync));
        }

        public async Task<bool> ValidateCodeAsync(string email, string code, CodeType type)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var record = await context.VerificationCodes
                    .FirstOrDefaultAsync(x => x.Email == email && x.Code == code && x.Type == type);
                if (record == null) return false;

                context.VerificationCodes.Remove(record);
                await context.SaveChangesAsync();

                return record.ExpirationDate > DateTime.UtcNow;
            },
            nameof(ValidateCodeAsync));
        }
    }
}
