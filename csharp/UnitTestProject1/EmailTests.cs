using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

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
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task Confirm()
        {
            bool result = await EmailSvc.Confirm(string.Empty, _email);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task Unsubscribe()
        {
            bool result = await EmailSvc.Unsubscribe(string.Empty, _email);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void ScreenerImage()
        {
            string jsonFile = "json/OptionChainSPXW.json";
            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFile);
            string json = File.ReadAllText(jsonFile);
            OptChn chain = DBHelper.Deserialize<OptChn>(json);

            chain = CalculationSvc.FilterOptionChainFutureOnly(chain);

            SdlChn sc = CalculationSvc.BuildStraddle(chain);
            ChartInfo info = ChartSvc.LineDouble(sc, "Open Interest", "Open Interest", Constants.DEFAULT_ZOOM);
            byte[] buffer = ChartSvc.FetchImage(info).Result;

            TestHelper.OpenImageBytes(buffer);
            Assert.AreNotEqual(null, info);
        }

        [TestMethod]
        public async Task ScreenerHtml()
        {
            string imageTicker = "SPX"; // ControllerHelper.GetScreenerImageTicker();

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
    }
}
