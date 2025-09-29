namespace MaxPainInfrastructure.Models.Schwab.Raw
{
    using System;
    using System.Collections.Generic;
    public class OptionChain
    {
        public string Symbol { get; set; }
        public string Status { get; set; }
        public string Strategy { get; set; }
        public double Interval { get; set; }
        public bool IsDelayed { get; set; }
        public bool IsIndex { get; set; }
        public double InterestRate { get; set; }
        public double UnderlyingPrice { get; set; }
        public double Volatility { get; set; }
        public double DaysToExpiration { get; set; }
        public int NumberOfContracts { get; set; }
        public string AssetMainType { get; set; }
        public string AssetSubType { get; set; }
        public bool IsChainTruncated { get; set; }
        public Dictionary<string, Dictionary<string, List<Option>>> CallExpDateMap { get; set; }
        public Dictionary<string, Dictionary<string, List<Option>>> PutExpDateMap { get; set; }
    }

    public class Option
    {
        public string PutCall { get; set; }
        public string Symbol { get; set; }
        public string Description { get; set; }
        public string ExchangeName { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Last { get; set; }
        public double Mark { get; set; }
        public int BidSize { get; set; }
        public int AskSize { get; set; }
        public string BidAskSize { get; set; }
        public int LastSize { get; set; }
        public double HighPrice { get; set; }
        public double LowPrice { get; set; }
        public double OpenPrice { get; set; }
        public double ClosePrice { get; set; }
        public int TotalVolume { get; set; }
        public long TradeTimeInLong { get; set; }
        public long QuoteTimeInLong { get; set; }
        public double NetChange { get; set; }
        public double Volatility { get; set; }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Theta { get; set; }
        public double Vega { get; set; }
        public double Rho { get; set; }
        public int OpenInterest { get; set; }
        public double TimeValue { get; set; }
        public double TheoreticalOptionValue { get; set; }
        public double TheoreticalVolatility { get; set; }
        //public List<OptionDeliverable> OptionDeliverablesList { get; set; }
        public double StrikePrice { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int DaysToExpiration { get; set; }
        public string ExpirationType { get; set; }
        public long LastTradingDay { get; set; }
        public double Multiplier { get; set; }
        public string SettlementType { get; set; }
        public string DeliverableNote { get; set; }
        public double PercentChange { get; set; }
        public double MarkChange { get; set; }
        public double MarkPercentChange { get; set; }
        public double IntrinsicValue { get; set; }
        public double ExtrinsicValue { get; set; }
        public string OptionRoot { get; set; }
        public string ExerciseType { get; set; }
        public double High52Week { get; set; }
        public double Low52Week { get; set; }
        public bool NonStandard { get; set; }
        public bool InTheMoney { get; set; }
        public bool Mini { get; set; }
        public bool PennyPilot { get; set; }
    }

    public class OptionDeliverable
    {
        public string Symbol { get; set; }
        public string AssetType { get; set; }
        public double DeliverableUnits { get; set; }
    }
}
