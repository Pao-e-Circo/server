using paoecirco.org_server.Responses;
using paoecirco.org_server.Responses.Councilor;
using System.ComponentModel.DataAnnotations.Schema;

namespace paoecirco.org_server.Domain
{
    [Table("councilours")]
    public class Councilour
    {
        [Column("id")]
        public Guid Id { get; init; } = Guid.NewGuid();
        [Column("name")]
        public required string Name { get; init; }
        [Column("phone")]
        public string? Phone { get; init; }
        [Column("email")]
        public string? Email { get; init; }
        [Column("photo_url")]
        public required string PhotoUrl { get; init; }
        [Column("party")]
        public required string Party { get; init; }
        public ICollection<Attendence> Attendances { get; init; } = new List<Attendence>();
        public ICollection<OfficeSpending> OfficeSpendings { get; init; } = new List<OfficeSpending>();

        public CounciloursResponse ToResponse() 
            => new(Id, Name, Phone, Email, PhotoUrl, Party);
    }
}
