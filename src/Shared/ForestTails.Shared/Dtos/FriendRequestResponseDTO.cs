using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class FriendRequestResponseDTO
    {
        [DataMember]
        public int RequesterId { get; set; }

        [DataMember]
        public string RequesterUsername { get; set; } = string.Empty;

        [DataMember]
        public bool WasAccepted { get; set; }
    }
}
