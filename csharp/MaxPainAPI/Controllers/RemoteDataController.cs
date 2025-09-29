using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class RemoteDataController : Controller
    {
        private readonly HomeContext _context;

        public RemoteDataController(HomeContext context)
        {
            _context = context;
        }

        // GET: api/RemoteData/MostActive
        [HttpGet("MostActive")]
        public async Task<JsonResult> MostActive(string ticker)
        {
            List<MostActive> actives = await _context.MostActive.ToListAsync();
            actives.ForEach(a => a.QueryType = a.GetQueryType());
            return Json(actives);
        }

        // GET: api/RemoteData/HighOpenInterest
        [HttpGet("HighOpenInterest")]
        public async Task<JsonResult> HighOpenInterest(string ticker)
        {
            DateTime maturity = Utility.NextFriday();
            List<HighOpenInterest> highs = await _context.HighOpenInterestRead(maturity);
            return Json(highs);
        }

        // GET: api/RemoteData/OutsideOIWalls
        [HttpGet("OutsideOIWalls")]
        public async Task<JsonResult> OutsideOIWalls()
        {
            List<OutsideOIWalls> walls = await _context.OutsideOIWalls.ToListAsync();
            return Json(walls);
        }

        [HttpGet("HistoricalMaxPain/{ticker}")]
        public async Task<JsonResult> HistoricalMaxPain(string ticker, DateTime m)
        {
            List<MaxPainHistory> items = await _context.MaxPainHistoryRead(ticker, m);
            return Json(items);
        }
    }
}