using paoecirco.org_server.Domain;

namespace paoecirco.org_server
{
    public static class RankingCalculator
    {
        public static IList<Councilour> Rank(this Dictionary<Guid, (int Attendences, int Absences, decimal OfficeSpending)> extension, IEnumerable<Councilour> councilors)
        {
            List<(decimal Points, Councilour Councilor)> rankedList = [];

            foreach (var i in councilors)
            {
                var variables = extension.Where(x => x.Key == i.Id).First();
                decimal points = (variables.Value.Attendences / (variables.Value.OfficeSpending == 0 ? 1 : variables.Value.OfficeSpending)  - (variables.Value.Absences)) * 100;

                rankedList.Add((points, i));
            }

            return rankedList.OrderByDescending(x => x.Points)
                .Select(x => x.Councilor)
                .ToList();
        }
    }
}
