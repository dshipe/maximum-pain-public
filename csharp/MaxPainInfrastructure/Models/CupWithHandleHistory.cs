using System.ComponentModel.DataAnnotations;

namespace MaxPainInfrastructure.Models
{
    public class CupWithHandleHistory
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Ticker { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RightPrice { get; set; }
        public decimal HandlePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public bool IsFailure { get; set; }
        public float Gamma { get; set; }
        public string? Base64 { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime Midnight { get; set; }
    }
}
