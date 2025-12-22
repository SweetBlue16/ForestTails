using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class FriendStatusUpdateDTO
    {
        [DataMember]
        public int FriendId { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }
    }
}
