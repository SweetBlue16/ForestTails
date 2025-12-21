using ForestTails.Server.Data.Entities;
using ForestTails.Server.Data.Enums;

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
}
