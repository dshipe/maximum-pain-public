using Microsoft.Extensions.Configuration;
using System.IO;

namespace UnitTestProject1
{
    public class Config
    {
        private IConfigurationRoot _config;
        public Config(string filename = "appsettings.json")
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(filename, false)
                .AddEnvironmentVariables();
            _config = configurationBuilder.Build();
        }

        public T Get<T>(string name = null)
        {
            var tName = name ?? typeof(T).Name;
            return _config.GetSection(tName).Get<T>();
        }
    }
}
