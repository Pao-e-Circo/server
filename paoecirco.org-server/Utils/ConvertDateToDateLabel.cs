namespace paoecirco.org_server.Utils
{
    public static class ConvertDateToDateLabel
    {
        public static string Convert(DateOnly date)
        {
            Dictionary<int, string> MonthsLabels = new()
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

            return $"{MonthsLabels[date.Month]} de {date.Year}";
        } 
    }
}
