using ForestTails.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("SocialNetworks")]
    public class SocialNetwork
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public SocialNetworkType NetworkType { get; set; }

        [Required, MaxLength(255)]
        public string Link { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
