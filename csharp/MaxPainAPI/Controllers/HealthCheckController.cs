using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class HealthCheckController : Controller
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly IControllerService _controller;

        public HealthCheckController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            IMemoryCache memoryCache,
            IControllerService controllerService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;
            _cache = memoryCache;
            _controller = controllerService;
        }

        [HttpGet("{timestamp}")]
        public async Task<ActionResult> Get(string timestamp, bool? debug)
        {
            if (debug == null) debug = true;

            string xml = await _awsContext.SettingsRead();
            XmlDocument xmlSettings = new XmlDocument();
            xmlSettings.LoadXml(xml);

            bool status = await _controller.HealthCheck(xmlSettings, Convert.ToBoolean(debug));

            await _awsContext.SettingsPost(xmlSettings.OuterXml);

            return Content(status.ToString(), "text/html", Encoding.UTF8);
        }
    }
}
