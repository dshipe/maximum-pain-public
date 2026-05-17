using MaxPainInfrastructure.Code;

namespace MaxPainInfrastructure.Models
{
    public class HistoricalOptionQuote
    {
        public long ID { get; set; }
        public string? Ticker { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? Content { get; set; }
    }

    public class HistoricalOptionQuoteXML
    {
        public long ID { get; set; }
        public string? Ticker { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? Content { get; set; }
    }

    public class ImportStaging
    {
        public long ID { get; set; }
        public string? Ticker { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? Content { get; set; }
    }

    public class ImportCache
    {
        public long ID { get; set; }
        public string? Ticker { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ImportDate { get; set; }
        //public DateTime? CreatedOnEST { get { return Utility.GMTToEST(this.CreatedOn.Value); } }
        public DateTime? CreatedOnEST { get; set; }
        //public int Hour { get { return CreatedOnEST.Value.Hour; } }
        public int Hour { get; set; }
        public string? Content { get; set; }
    }

    public class Transform
    {
        public long ID { get; set; }
        public long RefID { get; set; }
        public string? Ticker { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? Content { get; set; }
    }
}
