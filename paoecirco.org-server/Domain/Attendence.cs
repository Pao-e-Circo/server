using paoecirco.org_server.Responses;
using System.ComponentModel.DataAnnotations.Schema;

namespace paoecirco.org_server.Domain
{
    [Table("attendences")]
    public class Attendence
    {
        [Column("id")]
        public Guid Id { get; init; }
        [Column("councilour_id")]
        public Guid CouncilorId { get; init; }
        [Column("month")]
        public required string Month { get; init; }
        [Column("status")]
        public required string Status { get; init; }
        public required Councilour Councilour { get; init; }

        public AttendenceResponse ToResponse()
            => new(Id, CouncilorId, Month, Status);
    }
}
