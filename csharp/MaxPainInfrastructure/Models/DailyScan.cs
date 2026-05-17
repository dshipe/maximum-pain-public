using System.ComponentModel.DataAnnotations;

namespace MaxPainInfrastructure.Models
{
    public class DailyScan
    {
        [Key]
        public int Id { get; set; }
        public double ADR { get; set; }
        public string? Base64 { get; set; }
        public double BBUpper { get; set; }
        public double BBMiddle { get; set; }
        public double BBLower { get; set; }
        public double BBW { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime Date { get; set; }
        public bool HasAlerted { get; set; }
        public bool FlagAtrDrop { get; set; }
        public bool FlagFlatChannel { get; set; }
        public bool FlagHigherLows { get; set; }
        public bool FlagMovingAverages { get; set; }
        public bool FlagPricePattern { get; set; }
        public bool FlagVolumeRequirements { get; set; }
        public bool FlagMarketCap { get; set; }
        public bool FlagAvoidGapDown { get; set; }
        public bool FlagRsiMomentum { get; set; }
        public string? Model { get; set; }
        public double Price { get; set; }
        public double ProgressCurrentPrice { get; set; }
        public string? ProgressBase64 { get; set; }
        public DateTime? ProgressModifiedOn { get; set; }
        public double RSI { get; set; }
        public string? Sector { get; set; }
        public string? Source { get; set; }
        public string? Ticker { get; set; }
        public Int64 Volume { get; set; }
        public double Volume20 { get; set; }
        public bool? WatchFlag { get; set; }

        // for alert only
        public double ADRPercent { get; set; }
        public double MarkPercentChange { get; set; }
        public double NetPercentChange { get; set; }
    }
}
