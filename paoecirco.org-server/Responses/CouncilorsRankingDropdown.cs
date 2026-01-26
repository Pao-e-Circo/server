namespace paoecirco.org_server.Responses
{
    public class CouncilorsRankingDropdown
    {
        public List<CouncilorsRankingDropdownItem> Items { get; set; } = [];
    }
    public class CouncilorsRankingDropdownItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Label { get; set; }

        private readonly Dictionary<int, string> MonthsLabels = new()
        {
            { 1, "Janeiro" },
            { 2, "Fevereiro" },
            { 3, "Março" },
            { 4, "Abril" },
            { 5, "Maio" },
            { 6, "Junho" },
            { 7, "Julho" },
            { 8, "Agosto" },
            { 9, "Setembro" },
            { 10, "Outubro" },
            { 11, "Novembro" },
            { 12, "Dezembro" }
        };


        public CouncilorsRankingDropdownItem(DateOnly date)
        {
            Year = date.Year;
            Month = date.Month;
            Label = $"{MonthsLabels[date.Month]} de {date.Year}";
        }
    }
}
