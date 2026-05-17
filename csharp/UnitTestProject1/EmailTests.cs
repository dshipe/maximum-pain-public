using MaxPainChart;
using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Utility = MaxPainInfrastructure.Code.Utility;

namespace UnitTestProject1
{
    [TestClass]
    public class EmailTests : BaseTests
    {
        private string _email = "dan.shipe@yahoo.com";


        [TestMethod]
        public async Task Send()
        {
            string jsonFile = string.Format(@"{0}\json\EmailMessage.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(jsonFile);
            EmailMessage msg = DBHelper.Deserialize<EmailMessage>(json);
            string result = await EmailSvc.SendEmail(msg.From, msg.To, msg.CC, msg.BCC, msg.Subject, msg.Body, msg.AttachmentCSV, msg.IsHtml);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public async Task Subscribe()
        {
            bool result = await EmailSvc.Subscribe(string.Empty, _email);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Confirm()
        {
            bool result = await EmailSvc.Confirm(string.Empty, _email);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Unsubscribe()
        {
            bool result = await EmailSvc.Unsubscribe(string.Empty, _email);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ScreenerImage()
        {
            string jsonFile = "json/OptionChainSPXW.json";
            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFile);
            string json = File.ReadAllText(jsonFile);
            OptChn chain = DBHelper.Deserialize<OptChn>(json);

            chain = CalculationSvc.FilterOptionChain(chain);

            SdlChn sc = CalculationSvc.BuildStraddle(chain);
            ChartInfo info = ChartSvc.LineDouble(sc, "Open Interest", "Open Interest", Constants.DEFAULT_ZOOM);
            byte[] buffer = ChartHelper.RenderChart(info);

            TestHelper.OpenImageBytes(buffer);
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public async Task ScreenerHtml()
        {
            string imageTicker = "SPY"; // ControllerHelper.GetScreenerImageTicker();

            //string xmlFile = @"C:\VSProjects\MaxPain\MaxPainAPI\MaxPainAPI\xml\twitter.xml";
            //TwitterHelper helper = new TwitterHelper(GetQuotesFromJsonFile(), xmlFile);

            string jsonFile = "json/MostActive.json";
            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFile);
            string json = File.ReadAllText(jsonFile);
            List<MostActive> actives = DBHelper.Deserialize<List<MostActive>>(json);

            jsonFile = "json/OutsideOIWalls.json";
            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFile);
            json = File.ReadAllText(jsonFile);
            List<OutsideOIWalls> walls = DBHelper.Deserialize<List<OutsideOIWalls>>(json);

            jsonFile = "json/ImportMaxPain.json";
            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFile);
            json = File.ReadAllText(jsonFile);
            List<Mx> pains = DBHelper.Deserialize<List<Mx>>(json);

            string xslFile = @"C:\Websites\workspaces\maximum-pain.com\MaxPainAPI\wwwroot\xslt\ScreenerTableEmail.xsl";

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string imageFile = $@"{path}\images\screener.png";
            byte[] buffer = File.ReadAllBytes(imageFile);

            XmlDocument xmlDom = await EmailSvc.GetScreenerXml(actives, walls, pains, imageTicker, buffer);
            string xmlFile = @"json\screener.xml";
            xmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);
            xmlDom.Save(xmlFile);

            string html = EmailSvc.GetScreenerHtml(xmlDom, xslFile, true).Result;
            string htmlFile = @"json\screener.html";
            htmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, htmlFile);
            File.WriteAllText(htmlFile, html);
            TestHelper.OpenBrowserFile(htmlFile);

            Assert.AreNotEqual(0, html.Length);
        }

        [TestMethod]
        public async Task ScreenerHtmlEmbedded()
        {
            string imageTicker = "SPY";

            string jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json/MostActive.json");
            List<MostActive> actives = DBHelper.Deserialize<List<MostActive>>(File.ReadAllText(jsonFile));
            actives.ForEach(a => a.QueryType = a.GetQueryType());

            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json/OutsideOIWalls.json");
            List<OutsideOIWalls> walls = DBHelper.Deserialize<List<OutsideOIWalls>>(File.ReadAllText(jsonFile));

            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json/ImportMaxPain.json");
            List<Mx> pains = DBHelper.Deserialize<List<Mx>>(File.ReadAllText(jsonFile));

            // Render the SPX chart locally from the fixture option chain — same logic as GetEmailImage.
            // FilterOptionChain (no date gate) is used instead of FilterOptionChainFutureOnly so that
            // stale fixture maturities don't throw "Sequence contains no elements".
            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json/OptionChainSPY.json");
            var json = File.ReadAllText(jsonFile);
            OptChn chain = DBHelper.Deserialize<OptChn>(json);
            chain = CalculationSvc.FilterOptionChain(chain);
            SdlChn sc = CalculationSvc.BuildStraddle(chain);
            ChartInfo info = ChartSvc.LineDouble(sc, "Open Interest", "Open Interest", Constants.DEFAULT_ZOOM);
            byte[] chartBuffer = ChartHelper.RenderChart(info);

            // Use the embedded Screener.xsl — no hardcoded path required
            string xslContent = Utility.GetEmbeddedFile("Screener.xsl");

            XmlDocument xmlDom = await EmailSvc.GetScreenerXml(actives, walls, pains, imageTicker, chartBuffer);

            string html = await EmailSvc.GetScreenerHtml(xmlDom, xslContent, false);

            string htmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "json/screener-embedded.html");
            File.WriteAllText(htmlFile, html);
            TestHelper.OpenBrowserFile(htmlFile);

            Assert.IsTrue(html.Contains("maximum-pain.com"), "Expected branded header in output HTML");
            Assert.IsTrue(html.Contains("unsubscribe"), "Expected unsubscribe link in output HTML");
            Assert.IsTrue(html.Contains("data:image/png;base64,"), "Expected inline Base64 chart image");
            Assert.IsFalse(html.Contains("OptionsPop"), "OptionsPop affiliate link should be removed");
        }
    }
}
