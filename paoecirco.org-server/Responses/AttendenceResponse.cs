namespace paoecirco.org_server.Responses
{
    public sealed record AttendenceResponse(Guid Id, Guid CouncilorId, DateOnly Date, string Status);
}