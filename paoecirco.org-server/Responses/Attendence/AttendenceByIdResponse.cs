namespace paoecirco.org_server.Responses.Attendence
{
    public sealed record AttendenceResponse(Guid Id, Guid CouncilorId, DateOnly Date, string Status);
}