namespace MaxPainInfrastructure.Models.Schwab
{
    public class ScwExpirationList
    {
        public ScwExpiration[] expirations { get; set; }
    }

    public class ScwExpiration
    {
        public string expirationDate { get; set; }
        public int daysToExpiration { get; set; }
        public string expirationType { get; set; }
        public string settlementType { get; set; }
        public string optionRoots { get; set; }
        public bool standard { get; set; }
    }

}
