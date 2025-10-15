namespace MaxPainInfrastructure.Models
{
    public class Hop
    {
        public long Id { get; set; }
        public string? Destination { get; set; }
        public string? Referrer { get; set; }
        public string? UserAgent { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class HopSummary
    {
        public DateTime? CreatedOn { get; set; }
        public string? Destination { get; set; }
        public int Hops { get; set; }
    }

    public class HopAgent
    {
        public string? UserAgent { get; set; }
        public int Hops { get; set; }
    }

}
