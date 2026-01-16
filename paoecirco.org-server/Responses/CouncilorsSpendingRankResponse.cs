namespace paoecirco.org_server.Responses
{
    public record CouncilorsSpendingRankResponse(Guid Id,
        string Name,
        string? Phone,
        string? Email,
        string PhotoUrl,
        string Party,
        decimal TotalSpent,
        bool Winner);
}
