using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class TwitterController : Controller
    {
        private readonly HomeContext _homeContext;
        private readonly AwsContext _awsContext;
        private readonly IWebHostEnvironment _env;
        private readonly ILoggerService _logger;

        public TwitterController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            ILoggerService loggerService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;
            _logger = loggerService;
        }

        /*

        [HttpGet("SendTweet")]
        public async Task<JsonResult> SendTweet()
        {
            TwitterMessage message = await Send(string.Empty, false, false);
            return Json(message);
        }

        [HttpGet("SendTweetTest")]
        public async Task<JsonResult> SendTweetTest(string ticker, bool post, bool debug)
        {
            TwitterMessage message = await Send(ticker, post, debug);
            return Json(message);
        }

        private async Task<TwitterMessage> Send(string ticker, bool post, bool debug)
        {
            //string webRoot = _env.WebRootPath;
            //string file = Path.Combine(_env.WebRootPath, @"xml\twitter.xml");

            TwitterHelper helper = new TwitterHelper();
            if (ticker.Length != 0) helper.Ticker = ticker;
            helper.Post = post;
            helper.DebugMode = debug;

            return await helper.Execute();
        }

        [HttpGet("GetTweets")]
        public async Task<JsonResult> GetTweets()
        {
            TwitterHelper helper = new TwitterHelper();
            SocialBots.TweetList tweetList = await helper.GetTweets(DateTime.UtcNow, false, 10);
            return Json(tweetList);
        }

        [HttpGet("Execute/{timestamp}")]
        public async Task<JsonResult> Execute(string timestamp)
        {
            TwitterHelper helper = new TwitterHelper(_awsContext, _homeContext, new SdlChn(), string.Empty);
            TwitterMessage msg = await helper.Execute();
            await _logger.InfoAsync("Twitter", Utility.SerializeXml<TwitterMessage>(msg));
            return Json(msg);
        }

        [HttpGet("UserTweets/{timestamp}")]
        public async Task<JsonResult> UserTweets(string timestamp, string userNames, int count)
        {
			if (string.IsNullOrEmpty(userNames)) userNames = "super_trades";
		
			TwitterHelper helper = new TwitterHelper(_awsContext, _homeContext, new SdlChn(), string.Empty);
            List<UserTweet> uts = await helper.GetTweetsFromUser(userNames, count);
            return Json(uts);
        }

        [HttpGet("GetConfig")]
        public async Task<ActionResult> GetConfig()
        {
            TwitterHelper helper = new TwitterHelper(_awsContext, _homeContext, new SdlChn(), string.Empty);
            string xml = await helper.GetXml();
            return Content(xml, "text/html", Encoding.UTF8);
        }

        [HttpPost("SaveConfig")]
        public async Task<ActionResult> SaveConfig([FromForm]string xml)
        {
            TwitterHelper helper = new TwitterHelper(_awsContext, _homeContext, new SdlChn(), string.Empty);
            bool result = await helper.SaveXml(xml);
            string xml2 = await helper.GetXml();
            return Content(xml2, "text/html", Encoding.UTF8);
        }
        */
    }
}
