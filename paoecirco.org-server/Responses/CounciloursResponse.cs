namespace paoecirco.org_server.Responses
{
    public record CounciloursResponse(Guid Id,
        string Name,
        string? Phone,
        string? Email,
        string PhotoUrl,
        string Party);
}
