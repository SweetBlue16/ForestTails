using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("UnlockedCosmetics")]
    public class UnlockedCosmetic
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public int UserId { get; set; }

        [ForeignKey("CosmeticId")]
        public virtual Cosmetic? Cosmetic { get; set; }
        public int CosmeticId { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
