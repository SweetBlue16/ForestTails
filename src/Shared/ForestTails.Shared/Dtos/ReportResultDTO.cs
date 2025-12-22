using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class ReportResultDTO
    {
        [DataMember]
        public string ReportedUsername { get; set; } = string.Empty;

        [DataMember]
        public int TicketId { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}
