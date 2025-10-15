using System.ComponentModel.DataAnnotations;

namespace MaxPainInfrastructure.Models
{
    public class DailyScan
    {
        [Key]
        public int Id { get; set; }
        public string? Ticker { get; set; }
        public string? Source { get; set; }
        public double CurrentPrice { get; set; }
        public double RSRating { get; set; }
        public double? Sma10Day { get; set; }
        public double? Sma20Day { get; set; }
        public double? Sma50Day { get; set; }
        public double? Sma150Day { get; set; }
        public double? Sma200Day { get; set; }
        public double Week52Low { get; set; }
        public double Week52High { get; set; }
        public int Volume { get; set; }
        public double Volume20 { get; set; }
        public double ADR { get; set; }
        public double BBUpper { get; set; }
        public double BBMiddle { get; set; }
        public double BBLower { get; set; }
        public double BBW { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? Base64 { get; set; }
        public double ProgressCurrentPrice { get; set; }
        public string? ProgressBase64 { get; set; }
        public DateTime? ProgressModifiedOn { get; set; }
        public bool? WatchFlag { get; set; }
    }
}
