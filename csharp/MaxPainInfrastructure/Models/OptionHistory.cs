namespace MaxPainInfrastructure.Models
{
    public class OptionHistory
    {
        public string TK { get; set; }
        public DateTime M { get; set; }
        public string TY { get; set; }
        public decimal S { get; set; }
        public decimal OP { get; set; }
        public decimal SP { get; set; }
        public int OI { get; set; }
        public int V { get; set; }
        public float IV { get; set; }
        public DateTime D { get; set; }
    }

    public class OptionHistoryStraddle
    {
        public string TK { get; set; }
        public DateTime M { get; set; }
        public decimal S { get; set; }
        public decimal SP { get; set; }
        public DateTime D { get; set; }

        public decimal CP { get; set; }
        public int COI { get; set; }
        public int CV { get; set; }
        public float CIV { get; set; }

        public decimal PP { get; set; }
        public int POI { get; set; }
        public int PV { get; set; }
        public float PIV { get; set; }
    }
}
