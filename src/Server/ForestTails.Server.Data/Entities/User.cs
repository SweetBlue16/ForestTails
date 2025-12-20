using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        public int MichiCoins { get; set; } = 0;
        public int SelectedAvatarId { get; set; } = 1;

        public int ExperiencePoints { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int CurrentHealth { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;

        [MaxLength(50)]
        public string CurrentMap { get; set; } = "MainVillage";
        public float PositionX { get; set; } = 0;
        public float PositionY { get; set; } = 0;

        public virtual PlayerStatistics? Statistics { get; set; }
        public virtual ICollection<SocialNetwork> SocialNetworks { get; set; } = new List<SocialNetwork>();
        public virtual ICollection<Sanction> Sanctions { get; set; } = new List<Sanction>();
        public virtual ICollection<InventorySlot> Inventory { get; set; } = new List<InventorySlot>();
        public virtual ICollection<UnlockedCosmetic> UnlockedCosmetics { get; set; } = new List<UnlockedCosmetic>();

        public virtual ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
        public virtual ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();

        public static readonly User Empty = new()
        {
            Id = -1,
            Username = "GHOST"
        };

        public bool IsEmpty => Id == -1;

    }
}
