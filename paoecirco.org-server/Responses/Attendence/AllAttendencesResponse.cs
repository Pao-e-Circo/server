namespace paoecirco.org_server.Responses.Attendence
{
    public sealed record AllAttendencesResponse(Guid Id,
        Guid CouncilorId,
        string CouncilorName,
        string CouncilorParty,
        int TotalAttendences,
        int TotalAbsences,
        int TotalJustified,
        DateOnly Date,
        string Label
    );
}
