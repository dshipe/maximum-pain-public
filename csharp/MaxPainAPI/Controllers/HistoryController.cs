using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {

        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ICalculationService _calculation;
        private readonly IChartService _chart;
        private readonly IControllerService _controller;
        private readonly IFinDataService _finData;
        private readonly IHistoryService _history;

        private readonly IWebHostEnvironment _env;

        public HistoryController(
            AwsContext awsContext,
            HomeContext homeContext,
            HomeContext context,
            IWebHostEnvironment env,
            ICalculationService calculationService,
            IChartService chartService,
            IControllerService controllerService,
            IFinDataService finDataService,
            IHistoryService historyService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _calculation = calculationService;
            _chart = chartService;
            _controller = controllerService;
            _finData = finDataService;
            _history = historyService;

            _env = env;
            _homeContext = context;
        }

        [HttpGet("Options/{ticker}")]
        public async Task<JsonResult> Options(string ticker, int pastDays, int futureDays)
        {
            OptChn chain = await _controller.FetchOptionHistory(ticker, DateTime.MinValue, 0, pastDays, futureDays);
            // convert YMD back into a date
            chain.Options.ForEach(x => x.d = Convert.ToDateTime($"{x.d.Substring(2, 2)}/{x.d.Substring(4, 2)}/{x.d.Substring(0, 2)}").ToString("MM/dd/yy"));
            return Json(chain);
        }

        [HttpGet("Straddle/{ticker}")]
        public async Task<JsonResult> Straddle(string ticker, int pastDays, int futureDays)
        {
            OptChn chain = await _controller.FetchOptionHistory(ticker, DateTime.MinValue, 0, pastDays, futureDays);
            SdlChn sc = _calculation.BuildStraddle(chain);
            // convert YMD back into a date
            sc.Straddles.ForEach(x => x.d = Convert.ToDateTime($"{x.d.Substring(2, 2)}/{x.d.Substring(4, 2)}/{x.d.Substring(0, 2)}").ToString("MM/dd/yy"));
            return Json(sc);
        }

        [HttpGet("MaxPain/{ticker}")]
        public async Task<JsonResult> MaxPain(string ticker, int pastDays, int futureDays)
        {
            List<MaxPainHistory> quotes = await _controller.FetchMaxPainHistory(ticker, DateTime.MinValue);
            return Json(quotes);
        }

        [HttpPost("MaxPainPost")]
        public JsonResult MaxPainPost([FromForm] string json)
        {
            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            List<MaxPainHistory> histories = _calculation.CalculateMaxPainHistory(sc);
            return Json(histories);
        }

        [HttpGet("HistoricalStock/{ticker}")]
        public async Task<JsonResult> HistoricalStock(string ticker, int pastDays, int futureDays)
        {
            List<HistoricalStock> stocks = await _history.HistoricalStock(ticker, pastDays, futureDays);
            return Json(stocks);
        }

    }
}
