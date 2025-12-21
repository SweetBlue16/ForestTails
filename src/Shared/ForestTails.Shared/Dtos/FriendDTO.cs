using ForestTails.Shared.Enums;
using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class FriendDTO
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public int SelectedAvatarId { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }

        [DataMember]
        public FriendRequestStatus Status { get; set; }
    }
}
