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
            //string json = await ReadAWSSecret();
            string json = GetJson();

            var dict = DBHelper.Deserialize<Dictionary<string, string>>(json);
            if (dict.ContainsKey(key) == false)
            {
                string message = string.Format("The secret collection does not contain a key \"{0}\"", key);
                throw new ArgumentException(message);
            }
            return dict[key];
        }

        private string GetJson()
        {
            return @"
{
  ""TestKey"": ""TestValue"",
  ""CONNSTR_AWS"": ""Data Source=ec2-34-197-72-20.compute-1.amazonaws.com,1433;Initial Catalog=MaxPainAPI;User ID=SuperUser;Password=Database6#10oz;Trusted_Connection=False;MultipleActiveResultSets=true;Encrypt=False;"",
  ""CONNSTR_HOME"": ""Data Source=ec2-34-197-72-20.compute-1.amazonaws.com,1433;Initial Catalog=Fin;User ID=sa;Password=6#10oz;Trusted_Connection=False;MultipleActiveResultSets=true;Encrypt=False;"",
  ""SchwabAppKey"": ""8cEyjYrjEdoGPPePNbuZ5m1qmEgSbI3a"",
  ""SchwabSecret"": ""i6bRkP2EBS1U3oSu"",
  ""SchwabResponseUrl"": ""https://localhost/?code=BOByxqYla9czosYOp5TVa%2Fw6WYOadeMV8szdzMuBqnWZewcXC%2Bnq10m8PpVyJNysNW%2BCHdbxwPQyFJNK2dSepfF5fN28LGXEI5LilQ%2FpseAyAAfaj5oH4s8xSaa0AQfDxxRlQL5JSjp1crOYylZnX0M5SQzxiNnIVjhjAwaJalNgSMl02blTslLOB5PCn%2BdwPnajhr8P0miQkBE4OL%2BxSB03uswScoU5N%2FoCrY6m2iLPVe1upbojyzvjeMlOmVMQcChW1npVe%2BzIVGOs5eJExIKnBjSlHdfyvxCkghpvLGFkhj42r5wbcBAuwbz84DvVMTgQBUUPllsADJduQfXMdPKo4jjT5lyCzZMpHYxbRx5MLd7NPoMVFEE9lc0M64HZzXdeoRUgaEWUDMUhF7jfo9qkRgmbmsRGO%2BXMLht%2FxbCoxR3r55oPjqFFXHS100MQuG4LYrgoVi%2FJHHvl1uCEln5fW0%2Fv3Gy3vC2sq3G4pTbxIxXb3aSxwVdzkPpIxGFPZ6akRbsBzInwYt1az8sqwrUkMMCXur3WzBGjNAdX8g%2F2A8KQyNGrZY7hhjjrdJvcPy8%2BNngHTYt3uiqzG6r4OzIbTtfM5Kx4sxPw43LUMoZbFuWeIa6u8Y3SGV%2BM%2FMDwMLwcM0jHVtgmIZ1SI0CcrobnjTOd%2BUSD3cdtPnK7j9cfZRJy5mE93sxasrk7lCAzYRoCThVdi6wtJM4QW9KdMWfdjIokuBjySzxM7YwaQXV%2Fzns7xDq9JqNKmY3LcmWC6Xikp6TuSXFj4gxkIGnEJi4LDF6SsGvzIfCyM%2FFNCd7%2FaFKoj16sxqj%2FLveqU2sJVXLqV1NRYKkc%2FCmKWM7ooO%2FAHeMv8hzxgcm2K6Rbp91AQu2MH24nMr06l%2F2uIrhbGEjFxbY1Bc4%3D212FD3x19z9sWBHDJACbC00B75E"",
  ""TwitterScreenName"": ""optioncharts"",
  ""TwitterConsumerKey"": ""oe2D89deuCZUK3v9445iyt7tR"",
  ""TwitterConsumerSecret"": ""oxmzmqixtNXzetK3EvPgCuQP1IxxG44Pqv1v1LgoK0p6PeiIqu"",
  ""TwitterTokenKey"": ""716737614771044352-9ApPMhFcFvb6n0RRCXIBUU0a0UXDwyW"",
  ""TwitterTokenSecret"": ""7SobrhG3ZBbWsLsDSi8AEtd427QQMVQwauLSyk7PBkHef"",
  ""StockTwitsAccountName"": ""OptionsTech"",
  ""StockTwitsPassword"": ""stocktwits107"",
  ""AWSSMTPUsername"": ""AKIAXCEL5V2NZJP6MFTZ"",
  ""AWSSMTPPassword"": ""BGh+2kuw9jtL5n7IhWSiWRjZJnttSaQ3rXh6NgXQq4IO"",
  ""AWSSMTPHost"": ""email-smtp.us-east-1.amazonaws.com"",
  ""AWSSMTPPort"": ""587"",
  ""MailerLiteAPIKey"": ""efd45f04dee20ee1a9a3da123eb60a1d"",
  ""MailerLiteGroupIDDebug"": ""8932662"",
  ""MailerLiteGroupIDMain"": ""9040904"",
  ""GoogleUrlShortenerAPIKey"": ""AIzaSyB0ZECeLZAB-GzGWx0K-JjhkBPkp8KbpxA"",
  ""BitlyUserName"": ""danshipe"",
  ""BitlyAPIKey"": ""R_b1d0c2680fe8424a8e50b83db864bbcd"",
  ""ClickSendUsername"": ""dan.shipe@yahoo.com"",
  ""ClickSendAPIKey"": ""7BCC0017-D2C2-206C-ED5D-A4B91C16DF7A"",
  ""ConstEmail"": ""info@maximum-pain.com"",
  ""ConstLambdaUrl"": ""hcapr4ndhwksq5dq7ird3yujpq0edbbt.lambda-url.us-east-1.on.aws"",
  ""ConstChartDomain"": ""maximum-pain.com:83""
}

            ";
        }

    }
}
