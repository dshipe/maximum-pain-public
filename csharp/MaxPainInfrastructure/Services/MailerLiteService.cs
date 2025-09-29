using MaxPainInfrastructure.Code;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Web;

namespace MaxPainInfrastructure.Services
{
    public class MailerLiteService : IMailerLiteService
    {
        private bool _debugMode = false;
        public bool DebugMode
        {
            get { return _debugMode; }
            set
            {
                _debugMode = value;
                GroupID = GroupIDMain;
                if (_debugMode) GroupID = GroupIDDebug;
            }
        }

        private readonly ILoggerService _loggerService;
        private readonly ISecretService _secretService;

        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string APIKey { get; set; }
        public string GroupID { get; set; }
        public string GroupIDMain { get; set; }
        public string GroupIDDebug { get; set; }

        private static HttpClient? _client;

        private string _baseUrl;

        public MailerLiteService(
            ISecretService secretService,
            ILoggerService loggerService
        )
        {
            _loggerService = loggerService;
            _secretService = secretService;
        }

        private async Task Initialize()
        {
            APIKey = await _secretService.GetValue("MailerLiteAPIKey"); // your API key here 
            GroupIDDebug = await _secretService.GetValue("MailerLiteGroupIDDebug");
            GroupIDMain = await _secretService.GetValue("MailerLiteGroupIDMain");
            GroupID = GroupIDMain;

            DebugMode = false;
            FromName = "Dan";
            FromEmail = await _secretService.GetValue("ConstEmail");

            _client = new HttpClient();
            _baseUrl = "https://api.mailerlite.com/api/v2";
        }

        #region subscribe
        public async Task<bool> Subscribe(string emailAddr, string name)
        {
            try
            {
                await Initialize();
                await AddSubscriber(emailAddr, name);
            }
            catch (Exception ex)
            {
                string content = $"MailerLiteHelper.cs Subscribe {emailAddr}";
                await _loggerService.InfoAsync(content, ex.ToString());
            }
            return true;
        }

        public async Task<bool> Unsubscribe(string emailAddr)
        {
            try
            {
                await Initialize();
                await RemoveSubscriber(emailAddr);
            }
            catch (Exception ex)
            {
                string content = $"MailerLiteHelper.cs Unubscribe {emailAddr}";
                await _loggerService.InfoAsync(content, ex.ToString());
            }
            return true;
        }

        /// <summary>
        /// JSON data may contain fields:
        /// email, name, fields collection (custom fields), resubscribe, autoresponders, type
        /// </summary>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<string> AddSubscriber(string email, string name)
        {
            var url = $"{_baseUrl}/groups/{GroupID}/subscribers";

            var obj = new
            {
                email,
                type = "active",
                resubscribe = "true"
            };
            string json = DBHelper.Serialize(obj);

            return await GenericPost(HttpMethod.Post, url, json);
        }

        private async Task<string> RemoveSubscriber(string email)
        {
            string subscriberId = await GetSubscriberID(email);

            var url = $"{_baseUrl}/groups/{GroupID}/subscribers/{subscriberId}";

            var obj = new
            {
                type = "unsubscribed"
            };
            string json = DBHelper.Serialize(obj);

            return await GenericPost(HttpMethod.Put, url, json);
        }

        private async Task<string> GetGroups()
        {
            var url = $"{_baseUrl}/groups";
            return await GenericGet(url);
        }

        private async Task<string> GetSubscribers()
        {
            var url = $"{_baseUrl}/groups/{GroupID}/subscribers";
            return await GenericGet(url);
        }

        private async Task<string> GetSubscriber(string email)
        {
            //api.mailerlite.com/api/v1/subscribers/
            var url = $"{_baseUrl}/subscribers/{HttpUtility.UrlEncode(email)}";
            return await GenericGet(url);
        }

        private async Task<string> GetSubscriberID(string email)
        {
            string result = await GetSubscriber(email);
            JObject jobj = JObject.Parse(result);
            string subscriberId = Convert.ToString(jobj["id"]).Replace("\"", string.Empty);
            return subscriberId;
        }
        #endregion

        #region Campaign
        public async Task<bool> SendEmail(string subject, string html)
        {
            try
            {
                await Initialize();
                await AddCampaign(subject, html, FromEmail, FromName);
            }
            catch (Exception ex)
            {
                await _loggerService.InfoAsync("MailerLiteHelper.cs SendEmail", ex.ToString());
            }
            return true;
        }

        private async Task<string> AddCampaign(string subject, string html, string fromEmail, string fromName)
        {
            var url = $"{_baseUrl}/campaigns";

            // {"subject": "Regular campaign subject", "groups": [2984475, 3237221], "type": "regular"}  
            var objCampaign = new
            {
                type = "regular",
                subject = subject,
                groups = new[] { GroupID }
            };
            string json = DBHelper.Serialize(objCampaign);

            string result = await GenericPost(HttpMethod.Post, url, json);

            string campaignId = string.Empty;
            try
            {
                json = result;
                JObject jobj = JObject.Parse(json);
                campaignId = Convert.ToString(jobj["id"]).Replace("\"", string.Empty);
            }
            catch (Exception)
            {
                throw new Exception($"error parsing JSON CampaignID\r\n{result}");
            }

            url = $"{_baseUrl}/campaigns/{campaignId}/content";

            //{
            //    "html": "<h1>Title</h1><p>Content</p><p><small><a href=\"{$unsubscribe}\">Unsubscribe</a></small></p>",
            //    "plain": "Your email client does not support HTML emails. Open newsletter here: {$url}. If you do not want to receive emails from us, click here: {$unsubscribe}"
            //}
            string unsubscribeLink = @"<p><small><a href='{$unsubscribe}'>Unsubscribe</a></small></p>";
            html = string.Concat(html, unsubscribeLink);
            string plain = "Your email client does not support HTML emails. Open newsletter here: {$url}. If you do not want to receive emails from us, click here: {$unsubscribe}";
            var objContent = new
            {
                html,
                plain
            };
            json = DBHelper.Serialize(objContent);

            /*
            string json2 = "«\"html\":\"{0}\",\"plain\":\"{1}\"»";
            json2 = string.Format(json2, html, plain);
            json2 = json2.Replace("«", "{").Replace("»", "}");
            */

            result = await GenericPost(HttpMethod.Put, url, json);

            url = $"{_baseUrl}/campaigns/{campaignId}/actions/send";

            var objSchedule = new
            {
                type = "1",
                analytics = "0"
            };
            json = DBHelper.Serialize(objSchedule);

            result = await GenericPost(HttpMethod.Post, url, json);

            return result;
        }
        #endregion

        #region API
        private async Task<string> GenericGet(string url)
        {
            string result = string.Empty;
            string statusCode = string.Empty;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-MailerLite-ApiKey", APIKey);
            HttpResponseMessage response = await _client.SendAsync(request);
            statusCode = response.StatusCode.ToString();
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        private async Task<string> GenericPost(HttpMethod method, string url, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string result = string.Empty;
            string statusCode = string.Empty;

            HttpRequestMessage request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-MailerLite-ApiKey", APIKey);
            if (json.Length != 0) request.Content = content;
            HttpResponseMessage response = await _client.SendAsync(request);
            statusCode = response.StatusCode.ToString();
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"{response.StatusCode} {response.ReasonPhrase}");
            }
            return result;
        }
        #endregion

    }
}
