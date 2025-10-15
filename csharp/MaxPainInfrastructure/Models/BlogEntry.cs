namespace MaxPainInfrastructure.Models
{
    public class BlogEntry
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public int Ordinal { get; set; }
        public bool IsActive { get; set; }
        public bool IsStockPick { get; set; }
        public bool ShowOnHome { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? Content { get; set; }
    }
}
