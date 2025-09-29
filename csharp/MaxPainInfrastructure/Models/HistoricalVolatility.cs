using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MaxPainInfrastructure.Models
{
    public class HistoricalVolatility
    {
        public string Ticker { get; set; }
        public decimal ClosePrice { get; set; }
        [Key]
        [Column(TypeName = "smalldatetime")]
        public DateTime Date { get; set; }
        public double Day20 { get; set; }
        public double Day40 { get; set; }
        public double Day60 { get; set; }
        public double Day120 { get; set; }
        public double Day240 { get; set; }
    }
}