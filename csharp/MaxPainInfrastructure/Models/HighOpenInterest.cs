using System.ComponentModel.DataAnnotations;


namespace MaxPainInfrastructure.Models
{
    public class HighOpenInterest
    {
        [Key]
        public int SortID { get; set; }
        public string Ticker { get; set; }
        public string Maturity { get; set; }
        public int? Weekdy { get; set; }
        public string OptionType { get; set; }
        public string CreatedOn { get; set; }
        public int? OpenInterest { get; set; }
    }

    public class HighOpenInterestJson
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
    }
}
