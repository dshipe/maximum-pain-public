using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;


namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class HomeChartController : Controller
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ICalculationService _calculation;
        private readonly IChartService _chart;
        private readonly IControllerService _controller;
        private readonly IFinDataService _finData;

        private readonly IWebHostEnvironment _env;

        public HomeChartController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            ICalculationService calculationService,
            IChartService chartService,
            IControllerService controllerService,
            IFinDataService finDataService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _calculation = calculationService;
            _chart = chartService;
            _controller = controllerService;
            _finData = finDataService;

            _env = env;
        }

        #region Data
        [HttpGet("HistoricalVolatilityJson/{ticker}")]
        public async Task<JsonResult> HistoricalVolatilityJson(string ticker)
        {
            List<HistoricalVolatility> vols = await _homeContext.HistoricalVolatilityRead(ticker);
            ChartInfo info = _chart.HistoricalVolatility(vols);
            return Json(info);
        }

        [HttpGet("HighOpenInterestJson/{ticker}")]
        public async Task<JsonResult> HighOpenInterestJson(string ticker, DateTime m)
        {
            List<HighOpenInterest> highs = await _homeContext.HighOpenInterestRead(m);
            ChartInfo info = _chart.HighOpenInterest(highs);
            return Json(info);
        }
        #endregion

        #region Charts
        [HttpGet("HistoricalVolatility/{ticker}")]
        public IActionResult HistoricalVolatility(string ticker, DateTime m)
        {
            State model = new State();
            model.Ticker = ticker;
            model.Maturity = m;
            model.DataRoute = string.Format("/api/homechart/historicalvolatilityjson/{0}?m={1}", ticker, EncodeDate(m));
            return View("~/views/chart/generic.cshtml", model);
        }

        [HttpGet("HighOpenInterest/{ticker}")]
        public IActionResult HighOpenInterest(string ticker, DateTime m)
        {
            State model = new State();
            model.Ticker = ticker;
            model.Maturity = m;
            model.DataRoute = string.Format("/api/homechart/highopeninterestjson/{0}?m={1}", ticker, EncodeDate(m));
            return View("~/views/chart/generic.cshtml", model);
        }
        #endregion


        private string EncodeDate(DateTime dte)
        {
            return HttpUtility.UrlEncode(dte.ToString("MM/dd/yyyy"));
        }
    }
}