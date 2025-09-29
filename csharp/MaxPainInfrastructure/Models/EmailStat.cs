namespace MaxPainInfrastructure.Models
{
    public class EmailStat
    {
        public DateTime? Date { get; set; }
        public int? Active { get; set; }
        public int? Confirmed { get; set; }
        public int? Unsubscribed { get; set; }
        public int? HoneyPot { get; set; }
    }
}
