using paoecirco.org_server.Utils;

namespace paoecirco.org_server.Responses.Councilor
{
    public class CouncilorsRankingDropdown
    {
        public List<CouncilorsRankingDropdownItem> Items { get; set; } = [];
    }
    public class CouncilorsRankingDropdownItem(DateOnly date)
    {
        public int Year { get; set; } = date.Year;
        public int Month { get; set; } = date.Month;
        public string Label { get; set; } = ConvertDateToDateLabel.Convert(date);
    }
}
