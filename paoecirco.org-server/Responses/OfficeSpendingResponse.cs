namespace paoecirco.org_server.Responses
{
    public record OfficeSpendingResponse(Guid Id,
        Guid CouncilorId,
        DateOnly Month,
        decimal Materials,
        decimal MobilePhone,
        decimal FixedPhone,
        decimal Paper,
        decimal AirlineTickets,
        decimal HotelRate,
        decimal Gasoline);
}
