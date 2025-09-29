using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly IConfigurationService _configuration;
        private readonly IControllerService _controller;

        public SettingsController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            IMemoryCache memoryCache,
            IConfigurationService configurationService,
            IControllerService controllerService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;
            _cache = memoryCache;
            _configuration = configurationService;
            _controller = controllerService;
        }

        [HttpGet("Test")]
        public ActionResult<string> Test()
        {
            return "Test";
        }

        [HttpGet("ScheduledTask/{timestamp}")]
        public async Task<JsonResult> ScheduledTask(string timestamp, bool? debug)
        {
            if (debug == null) debug = true;
            List<string> result = await _controller.ScheduledTask(Convert.ToBoolean(debug));

            var jsonObj = new
            {
                server = System.Environment.MachineName,
                buildDate = GetBuildDate(Assembly.GetExecutingAssembly()),
                result = result
            };

            return Json(jsonObj);
        }

        [HttpGet("ReadJson")]
        public async Task<JsonResult> ReadJson()
        {
            string xml = await _configuration.ReadXml();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string json = JsonConvert.SerializeXmlNode(doc);
            return Json(json);
        }

        [HttpGet("ReadXml")]
        public async Task<ActionResult<string>> ReadXml()
        {
            return await _configuration.ReadXml();
        }

        [HttpGet("Get")]
        public async Task<ActionResult<string>> Get(string key)
        {
            return await _configuration.Get(key);
        }

        [HttpGet("Set")]
        public async Task<ActionResult<string>> Set(string key, string value, string pw)
        {
            return await _configuration.Set(key, value);
        }

        [HttpPost("Set")]
        public async Task<ActionResult<string>> SetPost(string key, string value, string pw)
        {
            return await _configuration.Set(key, value);
        }

        [HttpPost("SaveXml")]
        public async Task<bool> SaveXml(string xml, string pw)
        {
            await _configuration.SaveXml(xml);
            return true;
        }

        [HttpPost("SaveJson")]
        public async Task<bool> SaveJson(string json, string pw)
        {
            XmlDocument doc = JsonConvert.DeserializeXmlNode(json);
            return await _configuration.SaveXml(doc.OuterXml);
        }

        //https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm
        private DateTime GetBuildDate(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
            }

            return default;
        }

        [HttpGet("ServerDetail")]
        public async Task<JsonResult> ServerDetail()
        {
            /*
            string hostName = Dns.GetHostName();
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(hostName);
            var ipItem = ipHostInfo.AddressList.First(a => a.ToString().Contains("."));
            var ipAddress = ipItem.ToString();

            HttpContext context = HttpContext.Request.HttpContext;
            //var ipAddress = context.GetServerVariable("LOCAL_ADDR");

            var jsonObj = new
            {
                MachineName = System.Environment.MachineName,
                Thread = System.Environment.ProcessId,
                BuildDate = GetBuildDate(Assembly.GetExecutingAssembly()),
                IPAddress = ipAddress
            };
            */

            var jsonObj = new
            {
                MachineName = System.Environment.MachineName,
            };

            return Json(jsonObj);
        }

        private IPAddress GetRemoteIPAddress(bool allowForwarded = true)
        {
            HttpContext context = HttpContext.Request.HttpContext;

            if (allowForwarded)
            {
                string header = (context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault());
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip;
                }
            }
            return context.Connection.RemoteIpAddress;
        }

    }
}