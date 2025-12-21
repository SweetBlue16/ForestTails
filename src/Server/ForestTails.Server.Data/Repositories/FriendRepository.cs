using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public interface IFriendRepository
    {
        Task<List<Friendship>> GetFriendshipsAsync(int userId, FriendRequestStatus status);
        Task<FriendRequestStatus?> GetStatusAsync(int requesterId, int addresseeId);
        Task SendRequestAsync(int requesterId, int addresseeId);
        Task UpdateStatusAsync(int requesterId, int addresseeId, FriendRequestStatus newStatus);
        Task RemoveFriendshipAsync(int requesterId, int addresseeId);
    }

    public class FriendRepository : BaseRepository<Friendship>, IFriendRepository
    {
        public FriendRepository(IDbContextFactory<ForestTailsDbContext> contextFactory,
        ILogger<FriendRepository> logger) : base(contextFactory, logger) {}

        public async Task<List<Friendship>> GetFriendshipsAsync(int userId, FriendRequestStatus status)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var list = await context.Friendships
                    .AsNoTracking()
                    .Include(f => f.Requester)
                    .Include(f => f.Addressee)
                    .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == status)
                    .ToListAsync();
                return list ?? new List<Friendship>();
            },
            nameof(GetFriendshipsAsync));
        }

        public async Task<FriendRequestStatus?> GetStatusAsync(int requesterId, int addresseeId)
        {
            return await ExecuteSafeAsync(async (context) =>
            {
                var friendship = await context.Friendships
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f =>
                    (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                    (f.AddresseeId == requesterId && f.RequesterId == addresseeId));
                return friendship?.Status;
            },
            nameof(GetStatusAsync));
        }

        public async Task RemoveFriendshipAsync(int requesterId, int addresseeId)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                await context.Friendships
                    .Where(f => (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                                (f.RequesterId == addresseeId && f.AddresseeId == requesterId))
                    .ExecuteDeleteAsync();
            },
            nameof(RemoveFriendshipAsync));
        }

        public async Task SendRequestAsync(int requesterId, int addresseeId)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                var friendship = new Friendship
                {
                    RequesterId = requesterId,
                    AddresseeId = addresseeId,
                    Status = FriendRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Friendships.AddAsync(friendship);
                await context.SaveChangesAsync();
            },
            nameof(SendRequestAsync));
        }

        public async Task UpdateStatusAsync(int requesterId, int addresseeId, FriendRequestStatus newStatus)
        {
            await ExecuteSafeAsync(async (context) =>
            {
                var friendship = await context.Friendships
                    .FirstOrDefaultAsync(f =>
                    (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                    (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

                if (friendship != null)
                {
                    friendship.Status = newStatus;
                    await context.SaveChangesAsync();
                }
            },
            nameof(UpdateStatusAsync));
        }
    }
}
