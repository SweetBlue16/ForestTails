using ForestTails.Server.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("Friendships")]
    public class Friendship
    {
        public int RequesterId { get; set; }
        public int AddresseeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        public virtual User? Requester { get; set; }
        public virtual User? Addressee { get; set; }
    }
}
