using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class LeaderboardDTO
    {
        [DataMember]
        public int Rank { get; set; }

        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public int GlobalPoints { get; set; }

        [DataMember]
        public int AvatarId { get; set; }
    }
}
