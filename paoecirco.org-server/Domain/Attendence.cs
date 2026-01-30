using paoecirco.org_server.Responses.Attendence;
using paoecirco.org_server.Utils;
using System.ComponentModel.DataAnnotations.Schema;

namespace paoecirco.org_server.Domain
{
    [Table("attendences")]
    public class Attendence
    {
        [Column("id")]
        public Guid Id { get; init; }
        [Column("councilor_id")]
        public Guid CouncilorId { get; init; }
        [Column("month")]
        public required DateOnly Month { get; init; }
        [Column("status")]
        public required string Status { get; init; }
        public required Councilour Councilor { get; init; }

        public AttendenceResponse ToAttendenceResponse()
            => new(Id, CouncilorId, Month, Status);

        /// <summary>
        /// Needs the <see cref="Attendence.Councilor"/> navigation object included.
        /// </summary>
        public AllAttendencesResponse ToAllAttendencesResponse(int totalAttendences, int totalAbsences, int totalJustified)
            => new(Id, CouncilorId, Councilor.Name, Councilor.Party, totalAttendences, totalAbsences, totalJustified, Month, ConvertDateToDateLabel.Convert(Month));
    }
}
