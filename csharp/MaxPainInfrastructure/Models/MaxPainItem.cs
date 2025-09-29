namespace MaxPainInfrastructure.Models
{
    public class MPChain
    {
        public string Stock { get; set; }
        public decimal StockPrice { get; set; }
        public DateTime Maturity { get; set; }
        public int Mint { get; set; }

        public decimal MaxPain { get; set; }
        public decimal HighCallOI { get; set; }
        public decimal HighPutOI { get; set; }
        public int TotalCallOI { get; set; }
        public int TotalPutOI { get; set; }
        public decimal PutCallRatio { get; set; }
        public decimal MinCash { get; set; }
        public decimal MaxCash { get; set; }

        public DateTime CreatedOn { get; set; }

        public List<MPItem> Items { get; set; }

        public MPChain()
        {
            Items = new List<MPItem>();
        }
    }

    public class MPItem
    {
        public decimal s { get; set; }

        public int coi { get; set; }
        public decimal cch { get; set; }
        public decimal cpd { get; set; }

        public int poi { get; set; }
        public decimal pch { get; set; }
        public decimal ppd { get; set; }

        public decimal pd { get; set; }

        public decimal TotalCash()
        {
            return cch + pch;
        }
    }
}
