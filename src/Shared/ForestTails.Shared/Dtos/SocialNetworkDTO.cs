using ForestTails.Shared.Enums;
using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class SocialNetworkDTO
    {
        [DataMember]
        public SocialNetworkType NetworkType { get; set; }

        [DataMember]
        public string Link { get; set; } = string.Empty;
    }
}
