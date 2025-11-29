using paoecirco.org_server.Responses;
using System.ComponentModel.DataAnnotations.Schema;

namespace paoecirco.org_server.Domain
{
    [Table("office_spendings")]
    public class OfficeSpending
    {
        [Column("id")]
        public Guid Id { get; init; }
        [Column("councilor_id")]
        public Guid CouncilorId { get; init; }
        [Column("month")]
        public DateOnly Month { get; init; }
        [Column("materials")]
        public decimal Materials { get; init; }
        [Column("mobile_phone")]
        public decimal MobilePhone { get; init; }
        [Column("fixed_phone")]
        public decimal FixedPhone { get; init; }
        [Column("paper")]
        public decimal Paper { get; init; }
        [Column("airline_tickets")]
        public decimal AirlineTickets { get; init; }
        [Column("hotel_rate")]
        public decimal HotelRate { get; init; }
        [Column("gasoline")]
        public decimal Gasoline { get; init; }
        public required Councilour Councilour { get; init; }

        public OfficeSpendingResponse ToResponse()
            => new(Id, CouncilorId, Month, Materials, MobilePhone, FixedPhone, Paper, AirlineTickets, HotelRate, Gasoline);
    }
}
