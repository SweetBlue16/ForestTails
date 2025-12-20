using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForestTails.Server.Data.Entities
{
    [Table("InventorySlots")]
    public class InventorySlot
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public int UserId { get; set; }

        [ForeignKey("ItemId")]
        public virtual GameItem? GameItem { get; set; }
        public int ItemId { get; set; }

        public int Quantity { get; set; }
        public int SlotPosition { get; set; }
    }
}
