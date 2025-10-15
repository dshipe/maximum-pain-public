using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using MaxPainInfrastructure.Code;
using System.Reflection;

namespace MaxPainInfrastructure.Services
{
    public class SecretService : ISecretService
    {
        public SecretService()
        {
        }

        private async Task<string> ReadAWSSecret()
        {
            try
            {
                string secretName = "maximum-pain";
                string region = "us-east-1";

                var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

                GetSecretValueRequest request = new GetSecretValueRequest
                {
                    SecretId = "maximum-pain",
                    VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
                };

                GetSecretValueResponse response;

                response = await client.GetSecretValueAsync(request);

                return response.SecretString;
            }
            catch (Exception e)
            {
                // do nothing
            }

            return null;
        }

        public async Task<string> GetValue(string key)
        {
            string json = await ReadAWSSecret();
            if (String.IsNullOrEmpty(json))
            {
                json = GetEmbeddedFile("secret.json");
            }
            if (String.IsNullOrEmpty(json))
            {
                throw new ApplicationException("Cannot locate AWS secrets or embedded json");
            }


            var dict = DBHelper.Deserialize<Dictionary<string, string>>(json);
            if (dict.ContainsKey(key) == false)
            {
                string message = string.Format("The secret collection does not contain a key \"{0}\"", key);
                throw new ArgumentException(message);
            }
            return dict[key];
        }

        private string GetEmbeddedFile(string filename)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                  .Single(str => str.EndsWith(filename));

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                // do nothing
            }
            return null;
        }
    }
}
