namespace MaxPainInfrastructure.Models
{

    public class Daily
    {
        public string? Ticker { get; set; }
        public string? Source { get; set; }
        public DateTime? Date { get; set; }
        public double? Open { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Close { get; set; }
        public double? AdjClose { get; set; }
        public long Volume { get; set; }
        public double? Dividends { get; set; }
        public double? StockSplits { get; set; }
    }
}
