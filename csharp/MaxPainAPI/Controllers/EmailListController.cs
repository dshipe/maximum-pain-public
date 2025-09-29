using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class EmailListController : Controller
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly IWebHostEnvironment _env;
        private readonly ILoggerService _logger;
        private readonly IControllerService _controller;
        private readonly IEmailService _email;

        public EmailListController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            ILoggerService loggerService,
            IControllerService controllerService,
            IEmailService emailService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;
            _logger = loggerService;
            _controller = controllerService;
            _email = emailService;
        }

        // GET: api/Option/Chain/aapl
        [HttpGet("Stats")]
        public async Task<JsonResult> Stats(string ticker)
        {
            return Json(await _awsContext.EmailStat.ToListAsync());
        }

        [HttpGet("Screener/{timestamp}")]
        public async Task<IActionResult> Screener(string timestamp, bool? debug, bool? useShortUrls, bool? runNow, string pw)
        {
            await _logger.InfoAsync($"Screener BEGIN debug={debug} useShortUrls={useShortUrls} runNow={runNow}", string.Empty);

            string passPhrase = "6#10oz";
            if (pw == null || !pw.Equals(passPhrase))
            {
                await _logger.InfoAsync($"Screener END password is incorrect", string.Empty);
                return Content("password is incorrect", "text /html", Encoding.UTF8);
            }

            if (debug == null) debug = true;
            if (useShortUrls == null) useShortUrls = true;
            if (runNow == null) runNow = false;

            string html = await _controller.ScreenerDistribute(debug.Value, useShortUrls.Value, runNow.Value);

            return Content(html, "text/html", Encoding.UTF8);
        }

        [HttpPost("Subscribe")]
        public async Task<IActionResult> Subscribe(string name, string email)
        {
            await _email.Subscribe(name, email);
            await _controller.EmailListUpdate(name, email, EmailStatus.Active);
            return Content(string.Format("Confirmation sent to {0}", email), "text/html", Encoding.UTF8);
        }

        [HttpGet("Confirm")]
        public async Task<IActionResult> Confirm(string email)
        {
            await _email.Confirm(string.Empty, email);
            await _controller.EmailListUpdate(string.Empty, email, EmailStatus.Confirmed);

            string message = string.Format("{0} has been confirmed", email);
            string html = _controller.EmaiListUnsubscribeHTML(message);
            return Content(html, "text/html", Encoding.UTF8);
        }

        [HttpGet("Unsubscribe")]
        public async Task<IActionResult> Unsubscribe(string email)
        {
            await _email.Unsubscribe(string.Empty, email);
            await _controller.EmailListUpdate(string.Empty, email, EmailStatus.Unsubscribed);

            string message = string.Format("{0} has been unsubscribed", email);
            string html = _controller.EmaiListUnsubscribeHTML(message);
            return Content(html, "text/html", Encoding.UTF8);
        }
    }
}