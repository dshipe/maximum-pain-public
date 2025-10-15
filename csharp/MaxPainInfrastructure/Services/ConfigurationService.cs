using MaxPainInfrastructure.Models;
using Microsoft.Extensions.Configuration;
using System.Xml;

namespace MaxPainInfrastructure.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly AwsContext _awsContext;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _configuration;

        public ConfigurationService(
            AwsContext awsContext,
            ILoggerService logger,
            IConfiguration configuration)
        {
            _awsContext = awsContext;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> Get(string key)
        {
            string xml = await ReadXml();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode? nde = doc.SelectSingleNode($"/Settings/{key}");
            return nde?.InnerText ?? string.Empty;
        }

        public async Task<string> Set(string key, string value)
        {
            string xml = await ReadXml();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode? nde = doc.SelectSingleNode($"/Settings/{key}");
            XmlElement elm = nde as XmlElement ?? doc.CreateElement(key);
            if (nde == null)
            {
                doc.DocumentElement?.AppendChild(elm);
            }
            elm.InnerText = value;

            await SaveXml(doc.OuterXml);
            return await ReadXml();
        }

        public async Task<string> ReadXml()
        {
            return await _awsContext.SettingsRead();
        }

        public async Task<bool> SaveXml(string xml)
        {
            await _awsContext.SettingsPost(xml);
            return true;
        }

        #region Connection
        public string GetConnectionAWS()
        {
            return GetConnection("AWSConnection");
        }

        public string GetConnectionHome()
        {
            return GetConnection("HomeConnection");
        }

        private string GetConnection(string key)
        {
            return _configuration.GetConnectionString(key);
        }
        #endregion
    }
}
