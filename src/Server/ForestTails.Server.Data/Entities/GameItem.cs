using ForestTails.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("GameItems")]
    public class GameItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public int Rarity { get; set; }
        public ItemType Type { get; set; }

        public int BuyPrice { get; set; } = 0;
        public bool IsSellable { get; set; } = true;
        public int SellPrice { get; set; } = 0;
        public bool IsStackable { get; set; }
    }
}
