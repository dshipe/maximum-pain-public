namespace MaxPainInfrastructure.Models.Schwab
{
    public class ScwMarket
    {
        public ScwOption option { get; set; }
    }

    public class ScwOption
    {
        public ScwEQO EQO { get; set; }
        public ScwIND IND { get; set; }
    }

    public class ScwEQO
    {
        public string date { get; set; }
        public string marketType { get; set; }
        public string product { get; set; }
        public string productName { get; set; }
        public bool isOpen { get; set; }
        public ScwSessionhours sessionHours { get; set; }
    }

    public class ScwSessionhours
    {
        public ScwRegularmarket[] regularMarket { get; set; }
    }

    public class ScwRegularmarket
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }

    public class ScwIND
    {
        public string date { get; set; }
        public string marketType { get; set; }
        public string product { get; set; }
        public string productName { get; set; }
        public bool isOpen { get; set; }
        public ScwSessionhours sessionHours { get; set; }
    }
}
