using ForestTails.Shared.Enums;
using System.Runtime.Serialization;

namespace ForestTails.Shared.Dtos
{
    [DataContract]
    public class ReportRequestDTO
    {
        [DataMember]
        public string ReportedUsername { get; set; } = string.Empty;

        [DataMember]
        public ReportReason Reason { get; set; }

        [DataMember]
        public string Description { get; set; } = string.Empty;
    }
}
