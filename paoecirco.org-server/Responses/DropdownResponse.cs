namespace paoecirco.org_server.Responses
{
    public class DropdownResponse
    {
        public required int Year { get; init; }
        public required IEnumerable<DateResponse> Dates { get; init; }
    }

    public class DateResponse
    {
        public required DateOnly Date { get; init; }
        public required string Label { get; init; }
    }
}
