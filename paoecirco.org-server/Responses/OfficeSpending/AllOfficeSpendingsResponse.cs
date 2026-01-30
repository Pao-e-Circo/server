namespace paoecirco.org_server.Responses.OfficeSpending
{
    public sealed record AllOfficeSpendingsResponse(Guid Id,
        Guid CouncilorId,
        string CouncilorName,
        string CouncilorParty,
        decimal TotalSpent,
        decimal Materials,
        decimal MobilePhone,
        decimal FixedPhone,
        decimal Paper,
        decimal AirlineTickets,
        decimal HotelRate,
        decimal Gasoline,
        DateOnly Date,
        string Label
    );
}
