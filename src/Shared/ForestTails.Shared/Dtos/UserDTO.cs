using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class UserDTO
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public string SessionToken { get; set; } = string.Empty;
    }
}
