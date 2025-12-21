using ForestTails.Server.Data.Entities;

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
}
