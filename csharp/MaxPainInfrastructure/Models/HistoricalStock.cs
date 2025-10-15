namespace MaxPainInfrastructure.Models
{
    /*
    <ArrayOfHistoricalStock xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
	    <HistoricalStock>
		    <symbol>L</symbol>
		    <lastPrice>55.39</lastPrice>
		    <openPrice>0</openPrice>
		    <highPrice>0</highPrice>
		    <lowPrice>0</lowPrice>
		    <closePrice>55.39</closePrice>
	    </HistoricalStock>
	    <HistoricalStock>
		    <symbol>a</symbol>
		    <lastPrice>55.39</lastPrice>
		    <openPrice>0</openPrice>
		    <highPrice>0</highPrice>
		    <lowPrice>0</lowPrice>
		    <closePrice>55.39</closePrice>
	    </HistoricalStock>
    </ArrayOfHistoricalStock>
	*/


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ArrayOfHistoricalStock
    {

        [System.Xml.Serialization.XmlElementAttribute("HistoricalStock")]
        public HistoricalStock[] HistoricalStock { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class HistoricalStock
    {
        public string symbol { get; set; }
        public decimal lastPrice { get; set; }
        public decimal openPrice { get; set; }
        public decimal highPrice { get; set; }
        public decimal lowPrice { get; set; }
        public decimal closePrice { get; set; }
        public DateTime createdOn { get; set; }
    }

    public class StkPrc
    {
        public string d { get; set; }
        public decimal p { get; set; }
    }


}
