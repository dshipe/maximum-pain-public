using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Configuration;

using MaxPainChart.Models;

namespace MaxPainChart.Controllers
{
    public class ImageController : Controller
    {
        public ActionResult Index()
        {
            return Content("<html><body><h1>ImageController</h1></body></html>");
        }


        public ActionResult FetchUrl(string ticker, DateTime m)
        {
            string remoteDomain = ConfigurationManager.AppSettings["RemoteDomain"];
            string url = ChartHelper.FetchUrl(DataType.Open_Interest, remoteDomain, ticker, m);
            return Content(url);
        }

        public ActionResult FetchJson(string ticker, DateTime m)
        {
            string remoteDomain = ConfigurationManager.AppSettings["RemoteDomain"];
            string json = ChartHelper.FetchJson(DataType.Open_Interest, remoteDomain, ticker, m);
            return Content(json);
        }

        public ActionResult OpenInterest(string ticker, DateTime? m)
        {
            string remoteDomain = ConfigurationManager.AppSettings["RemoteDomain"];
            ChartInfo info = ChartHelper.FetchChartInfo(DataType.Open_Interest, remoteDomain, ticker, m);
            byte[] buffer = ChartHelper.RenderChart(info);
            //System.IO.File.WriteAllBytes(@"C:\Websites\chart.png", buffer);
            return File(buffer, "image/png");
        }

        public ActionResult Volume(string ticker, DateTime? m)
        {
            string remoteDomain = ConfigurationManager.AppSettings["RemoteDomain"];
            ChartInfo info = ChartHelper.FetchChartInfo(DataType.Volume, remoteDomain, ticker, m);
            byte[] buffer = ChartHelper.RenderChart(info);
            return File(buffer, "image/png");
        }
        public ActionResult MaxPain(string ticker, DateTime? m)
        {
            string remoteDomain = ConfigurationManager.AppSettings["RemoteDomain"];
            ChartInfo info = ChartHelper.FetchChartInfo(DataType.Max_Pain, remoteDomain, ticker, m);
            byte[] buffer = ChartHelper.RenderChart(info);
            return File(buffer, "image/png");
        }

        [HttpPost]
        public ActionResult PostJson(string json)
        {
            ChartInfo info = (ChartInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(ChartInfo));
            byte[] buffer = ChartHelper.RenderChart(info);
            return File(buffer, "image/png");
        }

        public ActionResult PostLine(string json, string ty)
        {
            DataType type = ChartHelper.GetDataType(ty);

            ChartInfo info = (ChartInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(ChartInfo));
            byte[] buffer = ChartHelper.RenderChart(info);
            return File(buffer, "image/png");
        }

    }
}
