namespace MaxPainInfrastructure.Models.Schwab
{
    public class ScwOptionChainCSV
    {
        public string symbol { get; set; }
        public string status { get; set; }
        public object underlying { get; set; }
        public string strategy { get; set; }
        public float interval { get; set; }
        public bool isDelayed { get; set; }
        public bool isIndex { get; set; }
        public float interestRate { get; set; }
        public float underlyingPrice { get; set; }
        public float volatility { get; set; }
        public float daysToExpiration { get; set; }
        public int numberOfContracts { get; set; }
        public List<ScwOptionCSV> options { get; set; }
    }

    public class ScwOptionCSV
    {
        public string ticker { get; set; }
        public string stock { get; set; }
        public string maturity { get; set; }
        public string maturityYMD { get; set; }
        public string type { get; set; }
        public decimal strike { get; set; }

        public string putCall { get; set; }
        public string symbol { get; set; }
        public string description { get; set; }
        public string exchangeName { get; set; }
        public float bid { get; set; }
        public float ask { get; set; }
        public float last { get; set; }
        public float mark { get; set; }
        public int bidSize { get; set; }
        public int askSize { get; set; }
        public string bidAskSize { get; set; }
        public int lastSize { get; set; }
        public float highPrice { get; set; }
        public float lowPrice { get; set; }
        public float openPrice { get; set; }
        public float closePrice { get; set; }
        public int totalVolume { get; set; }
        public object tradeDate { get; set; }
        public long tradeTimeInLong { get; set; }
        public string tradeTimeUTC { get; set; }
        public long quoteTimeInLong { get; set; }
        public string quoteTimeUTC { get; set; }
        public float netChange { get; set; }
        public float volatility { get; set; }
        public float delta { get; set; }
        public float gamma { get; set; }
        public float theta { get; set; }
        public float vega { get; set; }
        public float rho { get; set; }
        public int openInterest { get; set; }
        public float timeValue { get; set; }
        public float theoreticalOptionValue { get; set; }
        public float theoreticalVolatility { get; set; }
        public object optionDeliverablesList { get; set; }
        public float strikePrice { get; set; }
        //public long expirationDate { get; set; }
        public int daysToExpiration { get; set; }
        public string expirationType { get; set; }
        //public long lastTradingDay { get; set; }
        public float multiplier { get; set; }
        public string settlementType { get; set; }
        public string deliverableNote { get; set; }
        public object isIndexOption { get; set; }
        public float percentChange { get; set; }
        public float markChange { get; set; }
        public float markPercentChange { get; set; }
        public bool mini { get; set; }
        public bool inTheMoney { get; set; }
        public bool nonStandard { get; set; }
    }
}


