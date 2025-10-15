using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class HopController : Controller
    {
        private readonly AwsContext _awsContext;

        public HopController(AwsContext awsContext)
        {
            _awsContext = awsContext;
        }

        [HttpGet("test")]
        public async Task<JsonResult> test(string v)
        {
            string dstUrl = $"http://scrptcal.{v}.hop.clickbank.net";
            Hop hop = await InsertHop(HttpContext.Request.Headers, dstUrl);
            return new JsonResult(hop);
        }


        [HttpGet("cb")]
        public async Task<RedirectResult> cb(string v)
        {
            string dstUrl = $"http://scrptcal.{v}.hop.clickbank.net";
            Hop hop = await InsertHop(HttpContext.Request.Headers, dstUrl);
            return new RedirectResult(url: dstUrl, permanent: true, preserveMethod: true);
        }

        [HttpGet("Detail")]
        public async Task<JsonResult> Detail()
        {
            return Json(await _awsContext.Hop.OrderByDescending(x => x.Id).ToListAsync());
        }

        [HttpGet("Summary")]
        public async Task<JsonResult> Summary()
        {
            string sql = @"
                SELECT
	                dateadd(dd,0, datediff(dd,0, CreatedOn)) AS CreatedOn
	                ,Destination
	                ,COUNT(*) AS Hops
                FROM Hop
                GROUP BY dateadd(dd,0, datediff(dd,0, CreatedOn)), Destination
            ";

            string json = await _awsContext.FetchJson(sql, null, 30);
            List<HopSummary> hops = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HopSummary>>(json);

            return Json(hops);
        }

        [HttpGet("Agent")]
        public async Task<JsonResult> Agent()
        {
            string sql = @"
                SELECT
	                UserAgent
	                ,COUNT(*) AS Hops
                FROM Hop
                GROUP BY UserAgent
            ";

            string json = await _awsContext.FetchJson(sql, null, 30);
            List<HopAgent> hops = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HopAgent>>(json);

            return Json(hops);
        }



        #region helpers
        private async Task<Hop> InsertHop(Microsoft.AspNetCore.Http.IHeaderDictionary headers, string dstUrl)
        {
            string refr = GetReferrer(HttpContext.Request.Headers);
            string userAgent = GetReferrer(HttpContext.Request.Headers);

            Hop hop = new Hop()
            {
                Destination = dstUrl,
                Referrer = refr,
                UserAgent = userAgent,
                CreatedOn = System.DateTime.UtcNow
            };

            _awsContext.Hop.Add(hop);
            _awsContext.Entry(hop).State = EntityState.Added;
            await _awsContext.SaveChangesAsync();

            return hop;
        }

        private string GetReferrer(Microsoft.AspNetCore.Http.IHeaderDictionary headers)
        {
            string key = "Referer";
            if (!headers.ContainsKey(key)) return null;

            string result = headers[key].ToString();

            // limit to 255
            if (result.Length > 255) result = result.Substring(0, 255);
            return result;
        }

        private string GetUserAgent(Microsoft.AspNetCore.Http.IHeaderDictionary headers)
        {
            string key = "User-Agent";
            if (!headers.ContainsKey(key)) return null;

            string result = HttpContext.Request.Headers["User-Agent"];
            var uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(result);
            result = c.ToString();

            // limit to 255
            if (result.Length > 255) result = result.Substring(0, 255);
            return result;
        }

        #endregion
    }
}