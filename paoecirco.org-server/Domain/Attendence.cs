namespace paoecirco.org_server.Domain
{
    public class Attendence
    {
        public Guid Id { get; init; }
        public Guid CouncilorId { get; init; }
        public required string Month { get; init; }
        public required string Status { get; init; }
        public required Councilour Councilour { get; init; }
    }
}
