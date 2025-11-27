namespace paoecirco.org_server.Domain
{
    public class OfficeSpending
    {
        public Guid Id { get; init; }
        public Guid CouncilorId { get; init; }
        public DateTime Month { get; init; }
        public decimal Materials { get; init; }
        public decimal MobilePhone { get; init; }
        public decimal FixedPhone { get; init; }
        public decimal Paper { get; init; }
        public decimal AirlineTickets { get; init; }
        public decimal HotelRate { get; init; }
        public decimal Gasoline { get; init; }
        public required Councilour Councilour { get; init; }
    }
}
