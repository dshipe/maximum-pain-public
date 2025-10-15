using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;


namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class FinImportController : Controller
    {
        private readonly HomeContext _homeContext;
        private readonly AwsContext _awsContext;
        private readonly IWebHostEnvironment _env;
        private readonly ILoggerService _logger;
        private readonly ICalculationService _calculation;
        private readonly IConfigurationService _configuration;
        private readonly IControllerService _controller;
        private readonly IEmailService _email;
        private readonly IFinDataService _finData;
        private readonly IFinImportService _finImport;
        private readonly IHistoryService _history;

        public FinImportController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            ILoggerService loggerService,
            ICalculationService calculationService,
            IConfigurationService configurationService,
            IControllerService controllerService,
            IEmailService emailService,
            IFinDataService finDataService,
            IFinImportService finImportService,
            IHistoryService historyService
        )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;
            _logger = loggerService;
            _calculation = calculationService;
            _configuration = configurationService;
            _controller = controllerService;
            _email = emailService;
            _finImport = finImportService;
            _finData = finDataService;
            _history = historyService;
        }

        [HttpGet("RunImport")]
        public async Task<string> RunImport(bool debug, DateTime utc, bool sendEmail, string pw)
        {
            try
            {
                await _logger.InfoAsync($"FinImportController.RunImport BEGIN debug={debug} utc={utc} sendEmail={sendEmail}", string.Empty);

                string passPhrase = "6#10oz";
                if (pw == null || !pw.Equals(passPhrase))
                {
                    await _logger.InfoAsync($"FinImportController.RunImport END password is incorrect", string.Empty);
                    return $"FinImportController.RunImport BEGIN debug={debug} utc={utc} sendEmail={sendEmail}\r\npassword is incorrect";
                }

                _finImport.UseMessage = true;
                _finImport.IsDebug = debug;
                _finImport.ImportDateUTC = utc;

                string log = await _finImport.RunImport();

                /*
				DateTime utc = DateTime.UtcNow;
				DateTime est = Utility.GMTToEST(utc);
				int currentTime = Convert.ToInt32(est.ToString("HHmm"));
				*/

                if (sendEmail && _finImport.IsMarketOpen)
                {
                    string xml = await _awsContext.SettingsRead();
                    XmlDocument xmlSettings = new XmlDocument();
                    xmlSettings.LoadXml(xml);

                    await _logger.InfoAsync($"FinImportController.RunImport SEND EMAIL sendEmail={sendEmail} isMarketOpen={_finImport.IsMarketOpen} debug={debug}", string.Empty);
                    string imageTicker = await _configuration.Get("ScreenerImageTicker");
                    string html = await _email.ScreenerGenerate(true, true, string.Empty, false);
                    await _logger.InfoAsync($"FinImportController.RunImport END", string.Empty);
                }

                return log;
            }
            catch (Exception ex)
            {
                await _logger.InfoAsync($"FinImportController.RunImport ERROR", ex.ToString());
                return ex.ToString();
            }

        }

        [HttpGet("SendEmail")]
        public async Task<string> SendEmail()
        {
            try
            {
                string xml = await _awsContext.SettingsRead();
                XmlDocument xmlSettings = new XmlDocument();
                xmlSettings.LoadXml(xml);

                await _logger.InfoAsync($"FinImportController.SendEmail SEND EMAIL", string.Empty);
                string imageTicker = await _configuration.Get("ScreenerImageTicker");
                string html = await _email.ScreenerGenerate(true, true, string.Empty, false);
                await _logger.InfoAsync($"FinImportController.SendEmail END", string.Empty);

                return html;
            }
            catch (Exception ex)
            {
                await _logger.InfoAsync($"FinImportController.SendEmail ERROR", ex.ToString());
                return ex.ToString();
            }

        }


        [HttpGet("ImportOptions")]
        public async Task<string> ImportOptions(bool debug, bool saveMessage, DateTime utc, string tickersCSV, string pw)
        {
            _finImport.IsDebug = debug;
            _finImport.UseMessage = saveMessage;
            _finImport.ImportDateUTC = utc;

            if (!string.IsNullOrEmpty(tickersCSV)) _finImport.TickersCSV = tickersCSV;

            DateTime? dt = await _finImport.FetchMarketDate();
            if (!dt.HasValue)
            {
                await _finImport.ImportOptions(dt.Value);
            }
            return _finImport.GetLog();
        }

        [HttpGet("ImportStocks")]
        public async Task<string> ImportStocks(DateTime dt, bool debug, bool saveMessage, string pw)
        {
            string log = await _finImport.ImportStocks(dt);
            return log;
        }

        [HttpGet("GetMarketCalendar")]
        public async Task<List<MarketCalendar>> GetMarketCalendar(DateTime min, DateTime max)
        {
            List<DateTime> cal = new List<DateTime>();
            DateTime loopDate = min;
            while (loopDate <= max)
            {
                bool result = await _finData.IsMarketOpen(loopDate);
                if (result)
                {
                    cal.Add(loopDate);
                }
                loopDate = loopDate.AddDays(1);
            }

            return await _homeContext.MarketCalendar.Where(mc => mc.Date >= min && mc.Date <= max).ToListAsync();
        }

        [HttpGet("ShowMarketCalendar")]
        public async Task<List<MarketCalendar>> ShowMarketCalendar(bool top30)
        {
            if (top30)
            {
                return await _homeContext.MarketCalendar
                    .OrderByDescending(c => c.Date)
                    .Take(30)
                    .ToListAsync();
            }
            else
            {
                return await _homeContext.MarketCalendar
                    .ToListAsync();
            }
        }

        [HttpGet("ShowHistoryDate")]
        public async Task<JsonResult> ShowHistoryDate()
        {
            HistoryDate history = await _history.GetHistoryDate();
            return Json(history);
        }

        [HttpGet("ImportMaxPain")]
        public async Task<JsonResult> ImportMaxPain()
        {
            List<Mx> pains = await _homeContext.ImportMaxPainRecentRead();
            return Json(pains);
        }

        [HttpGet("Transform")]
        public async Task<string> Transform(DateTime start, string pw)
        {
            string xml = await _finImport.DoTransform(start);
            return xml;
        }

        [HttpGet("StockTickers")]
        public async Task<JsonResult> StockTickers()
        {
            List<PythonTicker>? python = await _awsContext.GetPythonTicker();
            string json = DBHelper.Serialize(python);
            List<StockTicker> tickers = DBHelper.Deserialize<List<StockTicker>>(json);
            return Json(tickers);
        }


        [HttpGet("Stocks")]
        public async Task<JsonResult> Stocks()
        {
            List<MaxPainInfrastructure.Models.Stock> stocks = await _controller.GetStocks();
            return Json(stocks);
        }

        [HttpGet("ImportDateCount")]
        public async Task<ActionResult<string>> ImportDateCount(int count)
        {
            string sql = @"
				SELECT CreatedOn, COUNT(*) AS Records 
				FROM HistoricalOptionQuoteXML WITH(NOLOCK)
				WHERE CreatedOn >DATEADD(dd, -30, GETUTCDATE())
				GROUP BY CreatedOn
				ORDER BY CreatedOn DESC
			";
            string json = await _homeContext.FetchJson(sql, null, 30);
            return Content(json, "application/json");
        }

        [HttpGet("CacheDateCount")]
        public async Task<ActionResult<string>> CacheDateCount(int count)
        {
            string sql = @"
				SELECT CreatedOn, COUNT(*) AS Records 
				FROM ImportCache WITH(NOLOCK)
				WHERE CreatedOn >DATEADD(dd, -15, GETUTCDATE())
				GROUP BY CreatedOn
				ORDER BY CreatedOn DESC
			";
            string json = await _homeContext.FetchJson(sql, null, 30);
            return Content(json, "application/json");
        }

        [HttpGet("ImportLog")]
        public async Task<JsonResult> ImportLog(int count)
        {
            List<ImportLog> logs = await _homeContext.ImportLog.ToListAsync();
            logs = logs.OrderByDescending(x => x.ID).Take(count).ToList();
            return Json(logs);
        }

        [HttpGet("RebuildPains")]
        // http://maximum-pain.com/api/finimport/RebuildPains?begin=1/1/2020&end=12/31/22
        public async Task<JsonResult> RebuildPains(DateTime begin, DateTime end, string pw)
        {
            List<Mx> pains = await _finImport.RebuildPains(begin, end);
            return Json(pains);
        }

        /*
		[HttpGet("PythonData")]
		public async Task<JsonResult> PythonData(DateTime currentDate)
		{
			//ImportMaxPainXml mpx = await _homeContext.ImportMaxPainXml.Where>(x => x.CreatedOn == currentDate);
			//return Json(mpx);

			//List<StockTicker> tickers = _awsContext.StockTicker.ToList();
		}
		*/

        [HttpGet("PatchVolume")]
        public async Task<JsonResult> PatchVolume(DateTime importDate, string ticker, string pw)
        {
            DateTime start = DateTime.UtcNow;
            int updated = await _finImport.PatchVolume(importDate, ticker);
            DateTime complete = DateTime.UtcNow;

            var jsonObj = new
            {
                importDate = importDate,
                start = start,
                complete = complete,
                count = updated
            };

            return Json(jsonObj);
        }

        [HttpGet("MostActive")]
        public async Task<JsonResult> MostActive(DateTime utc, bool debug)
        {
            HistoryDate history = await _history.GetHistoryDate();
            DateTime currentDate = history.CurrentDate;
            DateTime previousDate = history.PreviousDate;

            if (utc != DateTime.MinValue)
            {
                currentDate = utc;
                previousDate = await _history.PreviousMarketCalendar(currentDate);
            }

            List<OptChn> currentList = await _history.ChainGetByDate(currentDate);

            _finImport.IsDebug = debug;
            _finImport.UseMessage = true;
            List<MostActive> actives = await _finImport.MostActive(currentList, previousDate);

            return Json(actives);
        }

        [HttpGet("OutsideOIWalls")]
        public async Task<JsonResult> OutsideOIWalls(DateTime utc, bool debug)
        {
            HistoryDate history = await _history.GetHistoryDate();
            DateTime currentDate = history.CurrentDate;
            DateTime previousDate = history.PreviousDate;

            if (utc != DateTime.MinValue)
            {
                currentDate = utc;
                previousDate = await _history.PreviousMarketCalendar(currentDate);
            }

            List<SdlChn> straddles = new List<SdlChn>();
            List<OptChn> options = await _history.ChainGetByDate(currentDate);
            foreach (OptChn oc in options)
            {
                straddles.Add(_calculation.BuildStraddle(oc));
            }

            _finImport.IsDebug = debug;
            _finImport.UseMessage = true;
            List<OutsideOIWalls> walls = await _finImport.OutsideOIWalls(straddles);

            return Json(walls);
        }
    }
}

