using System.Reflection.Metadata;

namespace paoecirco.org_server.Responses.Attendence
{
    public record AttendencesTotalResponse(string CouncilorName,
        string CouncilorParty,
        int TotalAttendences,
        int TotalAbsences,
        int TotalJustified);
}
