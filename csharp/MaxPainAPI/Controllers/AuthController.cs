using FinanceDataConsumer.Models.Schwab;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationService _configuration;
        private readonly ISecretService _secret;

        private Token _token = null;
        private FinanceDataConsumer.Schwab _schwab;

        public AuthController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            IConfigurationService configurationService,
            ISecretService secretService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;

            _configuration = configurationService;
            _secret = secretService;

        }

        /// <summary>
        /// This method calls ASYNC functions that cannot be called from a SYNC constructor
        /// call this methid from public method
        /// </summary>
        /// <returns></returns>
        private async Task Initialize()
        {
            if (_token == null)
            {
                string json = await _configuration.Get("SchwabTokens");
                _token = JsonConvert.DeserializeObject<Token>(json);
            }

            if (_schwab == null)
            {
                string appKey = _secret.GetValue("SchwabAppKey");
                string secret = _secret.GetValue("SchwabSecret");

                _schwab = new FinanceDataConsumer.Schwab(appKey, secret);
            }
        }

        [HttpGet("LoginSchwab")]
        public async Task LoginSchwab()
        {
            await Initialize();

            string callbackUrl = await _configuration.Get("SchwabCallbackUrl");
            string url = await _schwab.GetSchwabLoginUrl(callbackUrl);
            Response.Redirect(url);
        }

        [HttpGet("CallbackSchwab")]
        public async Task<ActionResult> CallbackSchwab(string code)
        {
            await Initialize();

            string result = await SchwabCallback(code);
            return Content(result);
        }

        [HttpPost("CallbackSchwabUrl")]
        public async Task<ActionResult> CallbackSchwabUrl(string responseUrl)
        {
            await Initialize();

            var uri = new Uri(responseUrl);
            string queryString = uri.Query;
            NameValueCollection queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
            var code = queryDictionary["code"];

            string result = await SchwabCallback(code);
            return Content(result);
        }

        private async Task<string> SchwabCallback(string code)
        {
            try
            {
                if (code.Length == 0)
                {
                    return "expected code querystring parameter";
                }

                // generate the token
                string callbackUrl = await _configuration.Get("SchwabCallbackUrl");
                FinanceDataConsumer.Models.Schwab.Token token = await _schwab.GetRefreshToken(code, callbackUrl);

                // save then token
                string json = JsonConvert.SerializeObject(token);
                string content = await _configuration.Set("SchwabTokens", json);

                // turn on Schwab
                // content = await SettingsHelper.Set(_awsContext, "UseSchwab", "true");

                // save the responseUrl
                var location = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
                var responseUrl = location.AbsoluteUri;
                content = await _configuration.Set("SchwabResponseUrl", responseUrl);

                TruncateOptions();

                return content;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }
        }

        #region truncate
        [HttpGet("Truncate")]
        public void Truncate()
        {
            TruncateOptions();
        }

        private void TruncateOptions()
        {
            _awsContext.Database.ExecuteSqlRaw("TRUNCATE TABLE OptionChainJson");
        }
        #endregion
    }
}
