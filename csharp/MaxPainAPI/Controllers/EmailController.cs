using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class EmailController : Controller
    {
        private readonly ILoggerService _logger;
        private readonly IEmailService _email;
        private readonly IControllerService _controller;

        public EmailController(
            ILoggerService loggerService,
            IEmailService emailService,
            IControllerService controllerService
            )
        {
            _logger = loggerService;
            _email = emailService;
            _controller = controllerService;
        }

        // POST: api/Email/Send
        [HttpPost("Send")]
        public async Task<JsonResult> Send([FromForm] string json)
        {
            EmailMessage msg = JsonConvert.DeserializeObject<EmailMessage>(json);
            string result = await _email.SendEmail(msg.From, msg.To, msg.CC, msg.BCC, msg.Subject, msg.Body, msg.AttachmentCSV, msg.IsHtml);
            return Json(result);
        }

        // POST: api/Email/Send
        [HttpGet("Image")]
        public async Task<ActionResult> Image(string ticker)
        {
            if (string.IsNullOrEmpty(ticker)) ticker = "SPX";

            byte[] buffer = await _controller.GetEmailImage(ticker);
            return base.File(buffer, "image/png");
        }
    }
}