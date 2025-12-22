using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class PublicProfileDTO
    {
        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public string FullName { get; set; } = string.Empty;

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public int AvatarId { get; set; }

        [DataMember]
        public int GlobalPoints { get; set; }

        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public List<SocialNetworkDTO> SocialNetworks { get; set; } = new();
    }
}
