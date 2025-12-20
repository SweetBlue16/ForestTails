using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("PlayerStatistics")]
    public class PlayerStatistics
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int GlobalPoints { get; set; }
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }
        public long TotalPlayTimeSeconds { get; set; }

        public virtual User? User { get; set; }
    }
}
