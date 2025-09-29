using System.Web;

namespace MaxPainInfrastructure.Models
{
    public class State : EntityBase
    {
        public string? Ticker { get; set; }
        public DateTime Maturity { get; set; }
        public DateTime CreatedOn { get; set; }

        public string? DataRoute { get; set; }

        public string MaturityString { get { return Maturity.ToString("MM/dd/yyyy"); } }
        public string MaturityEncoded { get { return HttpUtility.UrlEncode(MaturityString); } }

        public string CreatedOnString { get { return CreatedOn.ToString("MM/dd/yy hh:mm tt"); } }

    }
}