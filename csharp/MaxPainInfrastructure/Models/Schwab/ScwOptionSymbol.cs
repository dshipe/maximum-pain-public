namespace MaxPainInfrastructure.Models.Schwab
{
    public class ScwOptionSymbol
    {
        public string underlying { get; set; }
        public DateTime maturity { get; set; }
        public string maturityStr { get { return maturity.ToString("MM/dd/yy"); } }
        public string maturityYMD { get { return maturity.ToString("yy-MM-dd"); } }
        public string optionType { get; set; }
        public decimal price { get; set; }
        public string priceStr { get { return price.ToString("00000.000").Replace(".", string.Empty); } }
        public string ticker { get { return BuildTicker(); } }

        private string BuildTicker()
        {
            // 123456789 123456789
            // GOOG100917C00530000
            // 1-4: underlying
            // 5-10: y-m-d
            // 11: call/ put
            // 12: strike (5 real +  3 decimal)

            return string.Format("{0}{1}{2}{3}"
                , underlying.ToUpper()
                , maturity.ToString("yyMMdd")
                , optionType.ToUpper()
                , priceStr
            );
        }

    }
}
