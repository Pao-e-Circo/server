namespace paoecirco.org_server.Domain
{
    public class Councilour
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Name { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }
        public required string PhotoUrl { get; init; }
        public required string Party { get; init; }
        public ICollection<Attendence> Attendances { get; init; } = new List<Attendence>();
    }
}
