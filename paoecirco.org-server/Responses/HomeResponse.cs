namespace paoecirco.org_server.Responses
{
    public class HomeResponse
    {
        public required MonthHighlight MonthHighlight { get; init; }
        public required IEnumerable<CouncilorHome> Rank { get; init; }
    }

    public class MonthHighlight
    {
        public required CouncilorHome TheColeest { get; init; }
        public required CouncilorHome TheSpender { get; init; }
    }

    public class CouncilorHome
    {
        public required string PhotoUrl { get; init; }
        public required string Name { get; init; }
        public required string PoliticalParty { get; init; }
        public string? Phone { get; init; }
        public required decimal OfficeExpenseThisMonth { get; init; }
    }
}
