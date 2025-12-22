using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class GameItemDTO
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public int Rarity { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public int BuyPrice { get; set; } = 0;

        [DataMember]
        public bool IsSellable { get; set; } = true;

        [DataMember]
        public int SellPrice { get; set; } = 0;

        [DataMember]
        public bool IsStackable { get; set; }
    }
}
