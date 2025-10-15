using System.Xml.Serialization;


namespace MaxPainInfrastructure.Models
{
    public class Mx
    {
        [XmlAttribute]
        public string t { get; set; }
        [XmlAttribute]
        public string m { get; set; }
        [XmlAttribute]
        public decimal sp { get; set; }
        [XmlAttribute]
        public decimal mp { get; set; }
        [XmlAttribute]
        public int coi { get; set; }
        [XmlAttribute]
        public int poi { get; set; }
        [XmlAttribute]
        public decimal hc { get; set; }
        [XmlAttribute]
        public decimal hp { get; set; }

        public Mx()
        {
        }

        public Mx(string ticker, string maturity, decimal stockPrice, decimal maxPain, int totalCallOI, int totalPutOI, decimal highCallOI, decimal highPutOI)
        {
            t = ticker;
            m = maturity;
            sp = stockPrice;
            mp = maxPain;
            coi = totalCallOI;
            poi = totalPutOI;
            hc = highCallOI;
            hp = highPutOI;
        }

        public decimal TotalOI()
        {
            return coi + poi;
        }

        public decimal Difference()
        {
            return System.Math.Abs(mp - sp);
        }

        public decimal PercentDifference()
        {
            return Difference() / sp;
        }
    }

    public class ImportMaxPainXml
    {
        public long ID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Content { get; set; }
    }
}
