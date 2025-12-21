using ForestTails.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("Sanctions")]
    public class Sanction
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public SanctionType Type { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
