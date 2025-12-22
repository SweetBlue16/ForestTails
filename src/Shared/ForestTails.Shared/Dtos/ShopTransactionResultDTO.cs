using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class ShopTransactionResultDTO
    {
        [DataMember]
        public int NewBalance { get; set; }

        [DataMember]
        public int ItemId { get; set; }

        [DataMember]
        public string TransactionId { get; set; } = string.Empty;
    }
}
