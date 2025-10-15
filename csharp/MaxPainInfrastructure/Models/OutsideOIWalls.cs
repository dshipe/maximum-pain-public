namespace MaxPainInfrastructure.Models
{
    public class OutsideOIWalls
    {
        public int Id { get; set; }
        public string Ticker { get; set; }
        public string Maturity { get; set; }
        public bool IsMonthlyExp { get; set; }
        public int SumOI { get; set; }
        public int PutOI { get; set; }
        public int CallOI { get; set; }
        public decimal StockPrice { get; set; }
        public decimal PutStrike { get; set; }
        public decimal CallStrike { get; set; }
    }
}
