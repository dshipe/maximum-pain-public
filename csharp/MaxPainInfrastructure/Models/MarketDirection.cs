namespace MaxPainInfrastructure.Models
{
    public class MarketDirection
    {
        public string Ticker { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Base64 { get; set; }
        public string CandlestickBase64 { get; set; }
    }
}
