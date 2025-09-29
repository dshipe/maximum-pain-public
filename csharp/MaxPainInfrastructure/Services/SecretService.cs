using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using MaxPainInfrastructure.Code;

namespace MaxPainInfrastructure.Services
{
    public class SecretService : ISecretService
    {
        public SecretService()
        {
        }

        private async Task<string> ReadAWSSecret()
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

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw e;
            }

            return response.SecretString;
        }

        public async Task<string> GetValue(string key)
        {
            string json = await ReadAWSSecret();
            //string json = GetJson();

            var dict = DBHelper.Deserialize<Dictionary<string, string>>(json);
            if (dict.ContainsKey(key) == false)
            {
                string message = string.Format("The secret collection does not contain a key \"{0}\"", key);
                throw new ArgumentException(message);
            }
            return dict[key];
        }
    }
}
