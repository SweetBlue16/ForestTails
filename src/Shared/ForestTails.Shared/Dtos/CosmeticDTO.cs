using ForestTails.Shared.Enums;
using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class CosmeticDTO
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public string ResourcePath { get; set; } = string.Empty;

        [DataMember]
        public int Price { get; set; }

        [DataMember]
        public CosmeticType Type { get; set; }

        [DataMember]
        public bool IsOwned { get; set; }
    }
}
