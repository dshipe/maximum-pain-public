namespace MaxPainInfrastructure.Models
{
    public class Message
    {
        /*
        public string subject { get; set; }
        public string body { get; set; }
        public DateTime? createdOn { get; set; }
        */

        public long Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
