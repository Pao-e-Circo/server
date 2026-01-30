namespace paoecirco.org_server.Responses.OfficeSpending
{
    public record OfficeSpendingByIdResponse(Guid Id,
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
