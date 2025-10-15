using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MaxPainInfrastructure.Services
{
    public class UrlShortService : IUrlShortService
    {
        private readonly ISecretService _secretService;

        public UrlShortService(
            ISecretService secretService
        )
        {
            _secretService = secretService;

        }

        public async Task<string> Google(string url)
        {
            string finalURL = string.Empty;
            string shortUrl = url;

            string key = await _secretService.GetValue("GoogleUrlShortenerAPIKey");
            string googleUrl = $"https://www.googleapis.com/urlshortener/v1/url?key={key}";

            string json = string.Concat("{\"longUrl\": \"", url, "\"}");
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsync(googleUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                string pattern = "http://goo.gl[^\"]+";
                finalURL = Regex.Match(result, pattern).ToString();
            }
            catch (Exception)
            {
                // if Google's URL Shortener is down...
                //throw ex;
                finalURL = url;
            }

            return finalURL;
        }

        public async Task<string> Bitly(string url)
        {
            /*
             * https://www.fluxbytes.com/csharp/shortening-a-url-using-bit-bitly-api-in-c/
             * To reset your API key, sign into your Bitly account and go to 'settings' located in the top right corner drop down menu. 
             * Head to the 'advanced' tab and scroll down. Click on 'show legacy API key', and press the 'reset' button.
            */

            string bitlyUrl = "http://api.bitly.com/v3/shorten";

            string user = await _secretService.GetValue("BitlyUserName");
            string key = await _secretService.GetValue("BitlyAPIKey");

            string formData = $"login={user}&apiKey={key}&longUrl={HttpUtility.UrlEncode(url)}&format=xml";
            byte[] buffer = Encoding.UTF8.GetBytes(formData);
            ByteArrayContent content = new ByteArrayContent(buffer);

            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsync(bitlyUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(result);
                string? shortUrl = xmlDoc.GetElementsByTagName("url")?[0]?.InnerText;
                //string longUrl = xmlDoc.GetElementsByTagName("long_url")[0].InnerText;
                return shortUrl;
            }
            catch (Exception)
            {
                return url;
            }
        }
    }
}
