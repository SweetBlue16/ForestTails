using ForestTails.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        public int Id { get; set; }

        public int ReporterUserId { get; set; }
        public int ReportedUserId { get; set; }

        [Required]
        public ReportReason Reason { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        [ForeignKey("ReporterUserId")]
        public virtual User? Reporter { get; set; }

        [ForeignKey("ReportedUserId")]
        public virtual User? Reported { get; set; }
    }
}
