namespace MaxPainInfrastructure.Models
{
    public enum EmailStatus
    {
        Active = 1,
        Unsubscribed = 2,
        Confirmed = 3,
        Honeypot = 4,
    }

    public class EmailAccount
    {
        public long Id { get; set; }

        public string Email { get; set; }
        public string? Name { get; set; }
        public int EmailStatusID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? LastEmailSent { get; set; }
    }
}
