namespace MaxPainInfrastructure.Models
{
    public class Spread
    {
        public string? Ticker { get; set; }
        public DateTime Maturity { get; set; }
        public OptionTypes OptionType { get; set; }
        public DateTime ModifiedOn { get; set; }

        public decimal LongStrike { get; set; }
        public decimal LongPrice { get; set; }
        public decimal LongBid { get; set; }
        public decimal LongAsk { get; set; }
        public decimal ShortStrike { get; set; }
        public decimal ShortPrice { get; set; }
        public decimal ShortBid { get; set; }
        public decimal ShortAsk { get; set; }

        public string Description
        {
            get
            {
                return string.Format("{0}/{1}", LongStrike.ToString("#,##0.00"), ShortStrike.ToString("#,##0.00"));
            }
        }

        public decimal Cost
        {
            get
            {
                return (LongPrice - ShortPrice) * 100;
            }
        }

        public decimal Value
        {
            get
            {
                if (OptionType == OptionTypes.Call)
                {
                    return (ShortStrike - LongStrike) * 100;
                }
                else
                {
                    return (LongStrike - ShortStrike) * 100;
                }
            }
        }

        public decimal Profit
        {
            get
            {
                return Value - Cost;
            }
        }

        public decimal ROI
        {
            get
            {
                return (Cost > 0) ? Profit / Cost : 0;
            }
        }

        public decimal BreakEven
        {
            get
            {
                return (Cost / 100) + LongStrike;
            }
        }
    }
}


