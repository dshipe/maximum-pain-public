namespace MaxPainInfrastructure.Models.Schwab
{
    public class ScwToken
    {
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string refresh_token { get; set; }
        public string access_token { get; set; }
        public string id_token { get; set; }
        public DateTime access_created_on_utc { get; set; }
        public DateTime refresh_created_on_utc { get; set; }
    }
}
