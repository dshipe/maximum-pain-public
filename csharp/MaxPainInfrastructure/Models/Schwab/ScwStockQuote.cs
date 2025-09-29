namespace MaxPainInfrastructure.Models.Schwab
{
    public class ScwStockQuote
    {
        public string assetMainType { get; set; }
        public string assetSubType { get; set; }
        public string quoteType { get; set; }
        public bool realtime { get; set; }
        public int ssid { get; set; }
        public string symbol { get; set; }
        public ScwFundamental fundamental { get; set; }
        public ScwQuote quote { get; set; }
        public ScwReference reference { get; set; }
        public ScwRegular regular { get; set; }
    }

    public class ScwFundamental
    {
        public float avg10DaysVolume { get; set; }
        public float avg1YearVolume { get; set; }
        public DateTime declarationDate { get; set; }
        public float divAmount { get; set; }
        public DateTime divExDate { get; set; }
        public int divFreq { get; set; }
        public float divPayAmount { get; set; }
        public DateTime divPayDate { get; set; }
        public float divYield { get; set; }
        public float eps { get; set; }
        public float fundLeverageFactor { get; set; }
        public DateTime lastEarningsDate { get; set; }
        public DateTime nextDivExDate { get; set; }
        public DateTime nextDivPayDate { get; set; }
        public float peRatio { get; set; }
    }

    public class ScwQuote
    {
        public float _52WeekHigh { get; set; }
        public float _52WeekLow { get; set; }
        public string askMICId { get; set; }
        public float askPrice { get; set; }
        public int askSize { get; set; }
        public long askTime { get; set; }
        public string bidMICId { get; set; }
        public float bidPrice { get; set; }
        public int bidSize { get; set; }
        public long bidTime { get; set; }
        public float closePrice { get; set; }
        public float highPrice { get; set; }
        public string lastMICId { get; set; }
        public float lastPrice { get; set; }
        public int lastSize { get; set; }
        public float lowPrice { get; set; }
        public float mark { get; set; }
        public float markChange { get; set; }
        public float markPercentChange { get; set; }
        public float netChange { get; set; }
        public float netPercentChange { get; set; }
        public float openPrice { get; set; }
        public float postMarketChange { get; set; }
        public float postMarketPercentChange { get; set; }
        public long quoteTime { get; set; }
        public string securityStatus { get; set; }
        public int totalVolume { get; set; }
        public long tradeTime { get; set; }
    }

    public class ScwReference
    {
        public string cusip { get; set; }
        public string description { get; set; }
        public string exchange { get; set; }
        public string exchangeName { get; set; }
        public bool isHardToBorrow { get; set; }
        public bool isShortable { get; set; }
        public int htbQuantity { get; set; }
        public float htbRate { get; set; }
    }

    public class ScwRegular
    {
        public float regularMarketLastPrice { get; set; }
        public int regularMarketLastSize { get; set; }
        public float regularMarketNetChange { get; set; }
        public float regularMarketPercentChange { get; set; }
        public long regularMarketTradeTime { get; set; }
    }

}
