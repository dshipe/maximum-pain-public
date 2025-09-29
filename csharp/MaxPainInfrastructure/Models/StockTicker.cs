namespace MaxPainInfrastructure.Models
{
    public class StockTicker
    {
        public int StockTickerID { get; set; }
        public string? Ticker { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
