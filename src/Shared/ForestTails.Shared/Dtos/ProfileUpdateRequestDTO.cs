using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class ProfileUpdateRequestDTO
    {
        [DataMember]
        public string FullName { get; set; } = string.Empty;

        [DataMember]
        public string Email { get; set; } = string.Empty;

        [DataMember]
        public string Biography { get; set; } = string.Empty;

        [DataMember]
        public List<SocialNetworkDTO> SocialNetworks { get; set; } = new();
    }
}
