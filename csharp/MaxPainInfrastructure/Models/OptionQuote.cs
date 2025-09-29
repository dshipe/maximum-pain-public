using MaxPainInfrastructure.Code;


namespace MaxPainInfrastructure.Models
{
    public enum OptionTypes
    {
        Call = 1,
        Put = 2
    }

    public class OptionQuote
    {
        public long Id { get; set; }

        public string OptionTicker { get; set; }
        public decimal LastPrice { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public int OpenInterest { get; set; }
        public int Volume { get; set; }
        public double ImpliedVolatility { get; set; }
        public double Delta { get; set; }
        public double Gamma { get; set; }
        public double Theta { get; set; }
        public double Vega { get; set; }
        public double Rho { get; set; }
        public DateTime ModifiedOn { get; set; }

        public DateTime DateOnly
        {
            get
            {
                DateTime dt = ModifiedOn;
                return new DateTime(dt.Year, dt.Month, dt.Day);
            }
        }

        public DateTime DateOnlyEST
        {
            get
            {
                DateTime dt = Utility.GMTToEST(ModifiedOn);
                return new DateTime(dt.Year, dt.Month, dt.Day);
            }
        }

        public string Ticker
        {
            get
            {
                string fragment = OptionTicker.Substring(0, OptionTicker.Length - 15);
                return fragment;
            }
        }

        public DateTime Maturity
        {
            get
            {
                string y = OptionTicker.Substring(OptionTicker.Length - 15, 2);
                y = string.Concat("20", y);
                string m = OptionTicker.Substring(OptionTicker.Length - 13, 2);
                string d = OptionTicker.Substring(OptionTicker.Length - 11, 2);
                return new DateTime(Convert.ToInt32(y), Convert.ToInt32(m), Convert.ToInt32(d));
            }
        }

        public string Type
        {
            get
            {
                string fragment = OptionTicker.Substring(OptionTicker.Length - 9, 1);
                return fragment;
            }
        }

        public OptionTypes OptionType
        {
            get
            {
                string description = (string.Equals(this.Type.ToUpper(), "C")) ? "Call" : "Put";
                Enum.TryParse(description, out OptionTypes value);
                return value;
            }
        }

        public decimal Strike
        {
            get
            {
                string fragment = OptionTicker.Substring(OptionTicker.Length - 8, 8);
                decimal money = Convert.ToDecimal(fragment) / 1000;
                return money;
            }
        }

        #region Constructor
        public OptionQuote()
        {
            this.ModifiedOn = DateTime.UtcNow;
        }

        public OptionQuote(string optionTicker, double lastPrice, double bid, double ask, int openInterest, int volume, double impliedVolatility, DateTime modifiedOn)
        {
            this.OptionTicker = optionTicker;
            this.LastPrice = Convert.ToDecimal(lastPrice);
            this.Bid = Convert.ToDecimal(bid);
            this.Ask = Convert.ToDecimal(ask);
            this.OpenInterest = openInterest;
            this.Volume = volume;
            this.ImpliedVolatility = impliedVolatility;
            this.ModifiedOn = modifiedOn;
        }
        #endregion
    }

    /*
    public class OptionQuoteSmall : EntityBase
    {
        public string OT { get; set; }
        public decimal LP { get; set; }
        public decimal B { get; set; }
        public decimal A { get; set; }
        public int OI { get; set; }
        public int V { get; set; }
        public double IV { get; set; }
    }
    */
}