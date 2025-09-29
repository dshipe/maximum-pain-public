namespace MaxPainInfrastructure.Models.Schwab
{
    public class ScwOptChn
    {
        public string stock { get; set; }
        public float price { get; set; }
        public float interestRate { get; set; }
        public float volatility { get; set; }
        public ScwOpt[] options { get; set; }
    }

    public class ScwOpt
    {
        //public string putCall { get; set; }
        public string symbol { get; set; }
        //public string description { get; set; }
        //public string exchangeName { get; set; }
        public float bid { get; set; }
        public float ask { get; set; }
        public float last { get; set; }
        //public float mark { get; set; }
        //public int bidSize { get; set; }
        //public int askSize { get; set; }
        //public string bidAskSize { get; set; }
        //public int lastSize { get; set; }
        //public float highPrice { get; set; }
        //public float lowPrice { get; set; }
        //public float openPrice { get; set; }
        //public float closePrice { get; set; }
        public int totalVolume { get; set; }
        //public long tradeTimeInLong { get; set; }
        //public long quoteTimeInLong { get; set; }
        public float netChange { get; set; }
        public float volatility { get; set; }
        public float delta { get; set; }
        public float gamma { get; set; }
        public float theta { get; set; }
        public float vega { get; set; }
        public float rho { get; set; }
        public int openInterest { get; set; }
        //public float timeValue { get; set; }
        //public float theoreticalOptionValue { get; set; }
        //public float theoreticalVolatility { get; set; }
        //public ScwOptiondeliverableslist[] optionDeliverablesList { get; set; }
        //public float strikePrice { get; set; }
        //public DateTime expirationDate { get; set; }
        //public int daysToExpiration { get; set; }
        //public string expirationType { get; set; }
        //public long lastTradingDay { get; set; }
        //public float multiplier { get; set; }
        //public string settlementType { get; set; }
        //public string deliverableNote { get; set; }
        //public float percentChange { get; set; }
        //public float markChange { get; set; }
        //public float markPercentChange { get; set; }
        //public float intrinsicValue { get; set; }
        //public float extrinsicValue { get; set; }
        //public string optionRoot { get; set; }
        //public string exerciseType { get; set; }
        //public float high52Week { get; set; }
        //public float low52Week { get; set; }
        //public bool nonStandard { get; set; }
        //public bool pennyPilot { get; set; }
        //public bool inTheMoney { get; set; }
        //public bool mini { get; set; }
    }

    public class ScwOptiondeliverableslist
    {
        public string symbol { get; set; }
        public string assetType { get; set; }
        public float deliverableUnits { get; set; }
    }

}