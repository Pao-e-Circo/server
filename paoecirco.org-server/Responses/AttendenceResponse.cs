namespace paoecirco.org_server.Responses
{
    public sealed record AttendenceResponse(Guid Id, Guid CouncilorId, string Month, string Status);
}