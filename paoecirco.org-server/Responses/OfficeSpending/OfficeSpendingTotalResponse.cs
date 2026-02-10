using System.Reflection.Metadata;

namespace paoecirco.org_server.Responses.OfficeSpending
{
    public record OfficeSpendingTotalResponse(string CouncilorName,
        string CouncilorParty,
        decimal TotalSpent,
        decimal TotalMaterials,
        decimal TotalMobilePhone,
        decimal FixedPhone,
        decimal TotalPaper,
        decimal TotalAirlineTickets,
        decimal TotalHotelRate,
        decimal TotalGasoline);
}
