using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class ChartInfoController : Controller
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly ICalculationService _calculation;
        private readonly IChartService _chart;
        private readonly IControllerService _controller;
        private readonly IFinDataService _finData;

        public ChartInfoController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env,
            IMemoryCache memoryCache,
            ICalculationService calculationService,
            IChartService chartService,
            IControllerService controllerService,
            IFinDataService finDataService)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;
            _cache = memoryCache;
            _calculation = calculationService;
            _chart = chartService;
            _controller = controllerService;
            _finData = finDataService;
        }

        #region Legacy MaxPainChart methods
        [HttpGet("OpenInterest/{ticker}")]
        public async Task<JsonResult> OpenInterest(string ticker, DateTime m, int zoomIn)
        {
            string key = "Open Interest";

            OptChn chain = await _controller.FetchOptionChain(ticker, m);
            chain = _calculation.FilterOptionChain(chain, m);
            SdlChn sc = _calculation.BuildStraddle(chain);
            ChartInfo info = _chart.LineDouble(sc, key, key, zoomIn);
            return Json(info);
        }

        [HttpGet("Volume/{ticker}")]
        public async Task<JsonResult> Volume(string ticker, DateTime m, int zoomIn)
        {
            string key = "Volume";

            OptChn chain = await _controller.FetchOptionChain(ticker, m);
            chain = _calculation.FilterOptionChain(chain, m);
            SdlChn sc = _calculation.BuildStraddle(chain);
            ChartInfo info = _chart.LineDouble(sc, key, key, zoomIn);
            return Json(info);
        }
        #endregion

        #region MaxPain
        [HttpGet("MaxPain/{ticker}")]
        public async Task<JsonResult> MaxPain(string ticker, DateTime m, int zoomIn)
        {
            OptChn chain = await _controller.FetchOptionChain(ticker, m);
            chain = _calculation.FilterOptionChain(chain);
            SdlChn sc = _calculation.BuildStraddle(chain);
            ChartInfo info = _chart.MaxPain(sc, zoomIn);
            return Json(info);
        }

        [HttpPost("MaxPainPost")]
        public JsonResult MaxPainPost([FromForm] string json, [FromForm] string title, [FromForm] string key, [FromForm] int zoomIn)
        {
            MPChain mpc = JsonConvert.DeserializeObject<MPChain>(json);
            ChartInfo info = _chart.MaxPain(mpc, zoomIn);
            return Json(info);
        }

        [HttpPost("HistoryMaxpainPost")]
        public JsonResult HistoryMaxpainPost([FromForm] string json, [FromForm] string title, [FromForm] string key, [FromForm] int zoomIn, [FromForm] bool showStock)
        {
            showStock = true;
            List<MaxPainHistory> histories = JsonConvert.DeserializeObject<List<MaxPainHistory>>(json);
            ChartInfo info = _chart.HistoryMaxPain(histories, showStock);
            return Json(info);
        }
        #endregion

        #region what
        [HttpPost("PricePost")]
        public JsonResult PricePost([FromForm] string json, [FromForm] string title, [FromForm] string key, [FromForm] int zoomIn, [FromForm] bool isPut)
        {
            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            ChartInfo info = _chart.Price(sc, title, zoomIn, isPut);
            return Json(info);
        }
        #endregion

        #region IVPredict
        [HttpGet("IVPredict/{ticker}")]
        public async Task<JsonResult> IVPredict(string ticker, DateTime m, int degree, int zoomIn)
        {
            if (degree == 0) degree = 1;
            OptChn chain = await _controller.FetchOptionChain(ticker, m);
            chain = _calculation.FilterOptionChain(chain);
            SdlChn sc = _calculation.BuildStraddle(chain);
            ChartInfo info = _chart.IVPredict(sc, "IV One Std Dev Predict", "1sd", degree, zoomIn);
            return Json(info);
        }

        [HttpPost("IVPredictPost")]
        public JsonResult IVPredictPost([FromForm] string json, [FromForm] string title, [FromForm] string key, [FromForm] int? degree, [FromForm] int zoomIn)
        {
            if (degree == null) degree = 1;
            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            ChartInfo info = _chart.IVPredict(sc, title, key, degree.Value, zoomIn);
            return Json(info);
        }
        #endregion

        #region generic chart post				
        [HttpPost("LineDoublePost")]
        public JsonResult LineDoublePost([FromForm] string json, [FromForm] string title, [FromForm] string key, [FromForm] int zoomIn)
        {
            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            ChartInfo info = _chart.LineDouble(sc, title, key, zoomIn);
            return Json(info);
        }

        [HttpPost("LineSinglePost")]
        public JsonResult LineSinglePost([FromForm] string json, [FromForm] string title, [FromForm] string key, [FromForm] int zoomIn)
        {
            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            ChartInfo info = _chart.LineSingle(sc, title, key, zoomIn);
            return Json(info);
        }
        #endregion

        #region HistoryPost
        [HttpPost("HistoryDoublePost")]
        public JsonResult HistoryDoublePost([FromForm] string json, [FromForm] string title, [FromForm] string key)
        {
            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            ChartInfo info = _chart.HistoryLineDouble(sc, title, key);
            return Json(info);
        }
        #endregion


        [HttpGet("Stacked/{ticker}")]
        public async Task<JsonResult> Stacked(string ticker, string title, string key, int zoomIn)
        {
            // https://localhost:5001/api/ChartInfo/Stacked/GE?title=title&key=open%20interest
            // https://maximum-pain.com/api/ChartInfo/Stacked/GE?title=title&key=open%20interest

            bool isPut = false;
            if (title.ToLower().IndexOf("put") != -1) isPut = true;

            OptChn chain = await _controller.FetchOptionChain(ticker, DateTime.MinValue, false);
            SdlChn sc = _calculation.BuildStraddle(chain);
            ChartInfo info = _chart.Stacked(sc, title, key, isPut, zoomIn);
            return Json(info);
        }

        [HttpPost("StackedPost")]
        public JsonResult StackedPost([FromForm] string json, [FromForm] string title, [FromForm] string key, [FromForm] int zoomIn)
        {
            bool isPut = false;
            if (title.ToLower().IndexOf("put") != -1) isPut = true;

            SdlChn sc = JsonConvert.DeserializeObject<SdlChn>(json);
            ChartInfo info = _chart.Stacked(sc, title, key, isPut, zoomIn);
            return Json(info);
        }
    }
}