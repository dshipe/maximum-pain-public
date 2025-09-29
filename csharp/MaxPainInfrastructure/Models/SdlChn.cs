using MaxPainInfrastructure.Code;

namespace MaxPainInfrastructure.Models
{
    public class SdlChn
    {
        public string? Source { get; set; }
        public string? Stock { get; set; }
        public decimal StockPrice { get; set; }
        public float InterestRate { get; set; }
        public float Volatility { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<Sdl> Straddles { get; set; }
        public List<StkPrc> Prices { get; set; }

        public SdlChn()
        {
            CreatedOn = DateTime.UtcNow;
            Straddles = new List<Sdl>();
            Prices = new List<StkPrc>();
        }
    }

    public class Sdl
    {
        public string? ot { get; set; }
        public string? d { get; set; }

        public decimal clp { get; set; }
        public decimal ca { get; set; }
        public decimal cb { get; set; }
        public int coi { get; set; }
        public int cv { get; set; }

        public float civ { get; set; }
        public float cde { get; set; }
        public float cga { get; set; }
        public float cth { get; set; }
        public float cve { get; set; }
        public float crh { get; set; }

        public decimal plp { get; set; }
        public decimal pa { get; set; }
        public decimal pb { get; set; }
        public int poi { get; set; }
        public int pv { get; set; }

        public float piv { get; set; }
        public float pde { get; set; }
        public float pga { get; set; }
        public float pth { get; set; }
        public float pve { get; set; }
        public float prh { get; set; }

        public string? Ticker()
        {
            if (ot == null) return null;
            string fragment = ot.Substring(0, ot.Length - 15);
            return fragment;
        }

        public int Mint()
        {
            return Utility.DateToYMD(Maturity());
        }

        public DateTime Maturity()
        {
            if (ot == null) return DateTime.MinValue;


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

        public decimal Strike()
        {
            if (ot == null) return 0M;
            string fragment = ot.Substring(ot.Length - 8, 8);
            decimal money = Convert.ToDecimal(fragment) / 1000;
            return money;
        }

        public decimal IntrinsicValue(decimal stockPrice, bool isPut)
        {
            decimal value = stockPrice - Strike();
            if (isPut) value = 0 - (Strike() - stockPrice);
            if (value < 0) value = 0;
            return value;
        }

        public decimal TimeValue(decimal stockPrice, bool isPut)
        {
            decimal value = ca - IntrinsicValue(stockPrice, isPut);
            if (isPut) value = pa - IntrinsicValue(stockPrice, isPut);
            if (value < 0) value = 0;
            return value;
        }

        public float ga0()
        {
            return cga - pga;
        }

        public int DaysToExpiration()
        {
            DateTime current = DateTime.Now;
            current = Convert.ToDateTime(current.ToString("MM/dd/yyyy"));
            return Convert.ToInt32((Maturity() - current).TotalDays);
        }

        public double oneStdDev(decimal stockPrice, bool isPut)
        {
            double iv_double = Convert.ToDouble(civ) / 100.0;
            if (isPut) iv_double = Convert.ToDouble(piv) / 100.0;

            double sqroot = Math.Sqrt((DaysToExpiration() / 365.0));
            double price = Convert.ToDouble(stockPrice);
            return price * iv_double * sqroot;
        }


    }
}
