using ForestTails.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<bool> UserExistsAsync(string username, string email);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task UpdateCurrencyAsync(int userId, int newAmount);
    }

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(IDbContextFactory<ForestTailsDbContext> contextFactory, 
        ILogger<UserRepository> logger) : base(contextFactory, logger) {}

        public async Task<User> CreateUserAsync(User user)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                user.Statistics ??= new PlayerStatistics();
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                return user;
            },
            nameof(CreateUserAsync));
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var user = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);
                return user ?? User.Empty;
            },
            nameof(GetByEmailAsync));
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var user = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Statistics)
                    .FirstOrDefaultAsync(u => u.Id == id);
                return user ?? User.Empty;
            },
            nameof(GetByIdAsync));
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var user = await context.Users
                    .AsNoTracking()
                    .Include(u => u.Statistics)
                    .Include(u => u.UnlockedCosmetics)
                    .FirstOrDefaultAsync(u => u.Username == username);
                return user ?? User.Empty;
            },
            nameof(GetByUsernameAsync));
        }

        public async Task UpdateCurrencyAsync(int userId, int newAmount)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                await context.Users.Where(u => u.Id == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.MichiCoins, newAmount));
            },
            nameof(UpdateCurrencyAsync));
        }

        public async Task UpdateUserAsync(User user)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            },
            nameof(UpdateUserAsync));
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                return await context.Users.AnyAsync(u => u.Username == username || u.Email == email);
            },
            nameof(UserExistsAsync));
        }
    }
}
