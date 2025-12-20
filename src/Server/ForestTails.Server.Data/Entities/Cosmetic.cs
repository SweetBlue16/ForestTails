using ForestTails.Server.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("Cosmetics")]
    public class Cosmetic
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string ResourcePath { get; set; } = string.Empty;

        public CosmeticType Type { get; set; }
        public int Price { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
