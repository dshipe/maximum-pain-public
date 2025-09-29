using MaxPainInfrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class PythonController : Controller
    {
        private AwsContext _awsContext;
        private HomeContext _homeContext;

        public PythonController(AwsContext awsContext, HomeContext homeContext)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
        }

        // GET: api/Python/CupWithHandleHistory
        [HttpGet("CupWithHandleHistory")]
        public async Task<JsonResult> CupWithHandleHistory(DateTime? midnight)
        {
            if (midnight.HasValue && midnight.Value == Convert.ToDateTime("01/01/1900"))
            {
                midnight = null;
            }
            List<CupWithHandleHistory> items = await _homeContext.GetCupWithHandleHistory(midnight);
            return Json(items);
        }

        [HttpGet("GetDailyScanDates")]
        public async Task<JsonResult> GetDailyScanDates()
        {
            List<DailyScan> items = await _homeContext.DailyScanDates();
            return Json(items);
        }


        [HttpGet("GetDailyScan")]
        public async Task<JsonResult> GetDailyScan(DateTime midnight)
        {
            List<DailyScan> items = await _homeContext.DailyScan(midnight);
            return Json(items);
        }

        [HttpGet("DailyScanUpdateWatch")]
        public async Task<JsonResult> DailyScanUpdateWatch(int id, bool flag)
        {
            List<DailyScan> items = await _homeContext.DailyScanUpdateWatch(id, flag);
            return Json(items);
        }

        [HttpGet("GetMarketDirection")]
        public async Task<JsonResult> GetMarketDirection()
        {
            List<MarketDirection> items = await _homeContext.MarketDirection();
            return Json(items);
        }
    }
}