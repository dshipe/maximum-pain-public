namespace MaxPainInfrastructure.Services
{
    public interface IConfigurationService
    {
        public Task<string> Get(string key);

        public Task<string> Set(string key, string value);

        public Task<string> ReadXml();

        public Task<bool> SaveXml(string xml);

        #region Connection
        public string GetConnectionAWS();

        public string GetConnectionHome();
        #endregion
    }
}
