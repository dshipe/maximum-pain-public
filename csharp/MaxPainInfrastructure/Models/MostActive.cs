using System.ComponentModel.DataAnnotations;

namespace MaxPainInfrastructure.Models
{
    public enum QueryType
    {
        ChangeOpenInterest = 1,
        ChangePrice = 2,
        ChangeVolume = 3,
        OpenInterest = 4,
        Volume = 5,
    }

    public class MostActive
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public int SortID { get; set; }

        [Required]
        public QueryType Type { get; set; }

        [Required]
        public string Ticker { get; set; }
        [Required]
        public DateTime Maturity { get; set; }
        [Required]
        public decimal Strike { get; set; }
        [Required]
        public string CallPut { get; set; }

        public int? OpenInterest { get; set; }
        public int? PrevOpenInterest { get; set; }

        public int? Volume { get; set; }
        public int? PrevVolume { get; set; }

        public decimal? Price { get; set; }
        public decimal? PrevPrice { get; set; }

        public double? IV { get; set; }
        public double? PrevIV { get; set; }

        public DateTime CreatedOn { get; set; }
        public bool NextMaturity { get; set; }
        public string QueryType { get; set; }

        public decimal ChangeOpenInterest { get; set; }
        public decimal ChangeVolume { get; set; }
        public decimal ChangePrice { get; set; }
        //public decimal ChangeIV { get; set; }

        public decimal GetChangeOpenInterest() { return CalcChange(OpenInterest, PrevOpenInterest); }
        public decimal GetChangeVolume() { return CalcChange(Volume, PrevVolume); }
        public decimal GetChangePrice() { return CalcChange(Price, PrevPrice); }
        //public decimal GetChangeIV() { return CalcChange2(IV, PrevIV); }

        public string GetQueryType() { return Type.ToString(); }

        public OptionTypes OptionType
        {
            get
            {
                string description = (string.Equals(this.CallPut.ToUpper(), "C")) ? "Call" : "Put";
                Enum.TryParse(description, out OptionTypes value);
                return value;
            }
        }

        private decimal CalcChange(decimal? value, decimal? prevValue)
        {
            if (!value.HasValue || !prevValue.HasValue) return 0M;

            if (prevValue.Value == 0) return 0M;

            decimal percent = ((value.Value - prevValue.Value) / prevValue.Value) * 100;
            return Math.Round(percent, 2);
        }
    }
}
