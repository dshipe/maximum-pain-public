using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class OptionsController : Controller
    {
        private readonly ILoggerService _logger;
        private readonly IFinDataService _finData;
        private readonly ICalculationService _calculation;
        private readonly IDateService _date;

        public OptionsController(
            ILoggerService loggerService,
            IFinDataService finDataService,
            ICalculationService calculationService,
            IDateService dateService)
        {
            _logger = loggerService;
            _finData = finDataService;
            _calculation = calculationService;
            _date = dateService;
        }

        #region get

        // GET: api/Options/Chain/aapl
        [HttpGet("Chain/{ticker}")]
        public async Task<JsonResult> Chain(string ticker, DateTime m)
        {
            OptChn chain = await _finData.FetchOptionChain(ticker, m, false);
            return Json(chain);
        }

        // GET: api/Option/Straddle/aapl
        [HttpGet("Straddle/{ticker}")]
        public async Task<JsonResult> Straddle(string ticker, DateTime m)
        {

            OptChn chain = await _finData.FetchOptionChain(ticker, m, false);
            return Json(_calculation.BuildStraddle(chain));
        }

        [HttpGet("MaxPain/{ticker}")]
        public async Task<JsonResult> MaxPain(string ticker, DateTime m)
        {
            OptChn chain = await _finData.FetchOptionChain(ticker, m);
            SdlChn sc = _calculation.BuildStraddle(chain);
            return Json(_calculation.Calculate(sc));
        }

        [HttpGet("Spreads/{ticker}")]
        public async Task<JsonResult> Spreads(string ticker, DateTime m)
        {
            OptChn chain = await _finData.FetchOptionChain(ticker, m);
            SdlChn sc = _calculation.BuildStraddle(chain);
            return Json(_calculation.BuildSpread(sc));
        }
        #endregion

        #region post
        // GET: api/Option/StraddleJson/aapl
        [HttpPost("StraddlePost")]
        public JsonResult StraddlePost([FromForm] string json, [FromForm] DateTime m)
        {
            OptChn chain = JsonConvert.DeserializeObject<OptChn>(json);
            chain = _calculation.FilterOptionChain(chain, _date.DateToYMD(m));
            return Json(_calculation.BuildStraddle(chain));
        }

        [HttpPost("MaxPainPost")]
        public JsonResult MaxPainPost([FromForm] string json)
        {
            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            return Json(_calculation.Calculate(sc));
        }
        #endregion

        #region CSV
        [HttpGet("csv/{ticker}")]
        public async Task<List<FinanceDataConsumer.Models.Schwab.OptionCSV>> csv(string ticker)
        {
            return await _finData.FetchOptionCSV(ticker);
        }
        #endregion

        #region TDA
        /*
        [HttpGet("tda/{ticker}")]
        public async Task<JsonResult> tda(string ticker)
        {
            string result = await ScrapeHelper.TDA_FetchOptionsRaw(_awsContext, ticker);
            return Json(result);
        }

        [HttpGet("tdacsv/{ticker}")]
        public async Task<string> tdacsv(string ticker)
        {
            return await ScrapeHelper.TDA_FetchOptionsCSV(_awsContext, ticker);
        }

        [HttpGet("tdacsvobj/{ticker}")]
        public async Task<List<OptionCSV>> tdacsvobj(string ticker)
        {
            return await ScrapeHelper.TDA_FetchOptionsCSVObj(_awsContext, ticker);
        }
        */
        #endregion
    }
}
