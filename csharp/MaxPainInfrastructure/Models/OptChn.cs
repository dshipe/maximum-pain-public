using MaxPainInfrastructure.Code;
using System.Xml.Serialization;


namespace MaxPainInfrastructure.Models
{
    [Serializable]
    public class OptChn
    {
        [XmlAttribute]
        public string Source { get; set; }

        [XmlAttribute]
        public string Stock { get; set; }

        [XmlAttribute]
        public decimal StockPrice { get; set; }

        [XmlAttribute]
        public float InterestRate { get; set; }

        [XmlAttribute]
        public float Volatility { get; set; }

        [XmlAttribute]
        public DateTime CreatedOn { get; set; }

        public List<Opt> Options { get; set; }

        public List<StkPrc> Prices { get; set; }

        [XmlAttribute]
        public string HttpStatusCode { get; set; }

        public OptChn()
        {
            this.Source = "TDA";
            this.CreatedOn = DateTime.UtcNow;
            this.HttpStatusCode = "200";
            Options = new List<Opt>();
            Prices = new List<StkPrc>();
        }
    }

    [Serializable]
    public class Opt
    {
        [XmlAttribute]
        public string ot { get; set; }

        [XmlAttribute]
        public string d { get; set; }

        [XmlAttribute]
        public decimal p { get; set; }

        [XmlAttribute]
        public float c { get; set; }

        [XmlAttribute]
        public decimal b { get; set; }

        [XmlAttribute]
        public decimal a { get; set; }

        [XmlAttribute]
        public int oi { get; set; }

        [XmlAttribute]
        public int v { get; set; }


        [XmlAttribute]
        public float iv { get; set; }

        [XmlAttribute]
        public float de { get; set; }

        [XmlAttribute]
        public float ga { get; set; }

        [XmlAttribute]
        public float th { get; set; }

        [XmlAttribute]
        public float ve { get; set; }

        [XmlAttribute]
        public float rh { get; set; }

        public string Ticker()
        {
            string fragment = ot.Substring(0, ot.Length - 15);
            return fragment;
        }

        public int Mint()
        {
            return Utility.DateToYMD(Maturity());
        }

        public DateTime Maturity()
        {
            string y = ot.Substring(ot.Length - 15, 2);
            y = string.Concat("20", y);
            string m = ot.Substring(ot.Length - 13, 2);
            string d = ot.Substring(ot.Length - 11, 2);

            return new DateTime(Convert.ToInt32(y), Convert.ToInt32(m), Convert.ToInt32(d));
        }

        public string Mstr()
        {
            return Maturity().ToString("MM/dd/yyyy");
        }

        public string Type()
        {
            string fragment = ot.Substring(ot.Length - 9, 1);
            return fragment.ToUpper();
        }

        public OptionTypes OptionType()
        {
            string description = (string.Equals(this.Type(), "C")) ? "Call" : "Put";
            Enum.TryParse(description, out OptionTypes value);
            return value;
        }

        public decimal Strike()
        {
            string fragment = ot.Substring(ot.Length - 8, 8);
            decimal money = Convert.ToDecimal(fragment) / 1000;
            return money;
        }


        #region Constructor
        public Opt()
        {
        }

        public Opt(string optionTicker, double lastPrice, double bid, double ask, int openInterest, int volume, DateTime modifiedOn)
        {
            this.ot = optionTicker;
            this.p = Convert.ToDecimal(lastPrice);
            this.b = Convert.ToDecimal(bid);
            this.a = Convert.ToDecimal(ask);
            this.oi = openInterest;
            this.v = volume;
        }
        #endregion
    }
}