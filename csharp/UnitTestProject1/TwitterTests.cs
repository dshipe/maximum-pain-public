using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Linq;

using Microsoft.AspNetCore.Hosting;

using Microsoft.EntityFrameworkCore;

using MaxPainAPI.Code;
using MaxPainAPI.Models;
using MaxPainAPI.Controllers;

namespace UnitTestProject1
{
    [TestClass]
    public class TwitterTests
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        //private readonly IHostingEnvironment _env;

        public TwitterTests()
        {
            var awsOptions = new DbContextOptionsBuilder<AwsContext>()
                .UseInMemoryDatabase(databaseName: "AwsDatabase")
                .Options;
            _awsContext = new AwsContext(awsOptions);

            var homeOptions = new DbContextOptionsBuilder<HomeContext>()
                .UseInMemoryDatabase(databaseName: "HomeDatabase")
                .Options;
            _homeContext = new HomeContext(homeOptions);

            /*
            _env = new Mock<IHostingEnvironment>();
            //...Setup the mock as needed
            _env
                .Setup(m => m.EnvironmentName)
                .Returns("Hosting:UnitTestEnvironment");
            */
        }

        [TestMethod]
        public void GMT()
        {
            string createdAt = "Sun Jun 16 13:00:41 +0000 2019";
            DateTime createdOn = DateTime.ParseExact(createdAt, "ddd MMM dd HH:mm:ss %K yyyy", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
            //DateTime.SpecifyKind(createdOn, DateTimeKind.Local);
            //createdOn = Utility.GMTToEST(createdOn);
            Assert.AreEqual("06/16/2019", createdOn.ToString("MM/dd/yyyy"));
        }

        [TestMethod]
        public void PostToTwitter()
        {
            string xmlFile = string.Format(@"{0}\json\twitter.xml", Directory.GetCurrentDirectory());
            TwitterHelper helper = new TwitterHelper(null, null, MaxPainHelper.DebugStraddle(false), xmlFile);

            byte[] buffer = ChartHelper.FetchImage("openinterest", "aapl", DateTime.MinValue).Result;

            string result = helper.TwitterPost("chart for AAPL", buffer).Result;

            Assert.AreNotEqual(-1, result.IndexOf("id"));
        }

        [TestMethod]
        public void PostToStockTwits()
        {
            string xmlFile = string.Format(@"{0}\json\twitter.xml", Directory.GetCurrentDirectory());
            TwitterHelper helper = new TwitterHelper(null, null, MaxPainHelper.DebugStraddle(false), xmlFile);

            byte[] buffer = File.ReadAllBytes(@"C:\VSProjects\MaxPain\MaxPainAPI\maximum-pain.com\UnitTestProject1\TestImage.jpg");

            string result = helper.StockTwitsPost("chart for AAPL", buffer).Result;

            Assert.AreNotEqual(-1, result.IndexOf("200"));
        }
               
        [TestMethod]
        public void SendTweet()
        {
            string xmlFile = string.Format(@"{0}\json\twitter.xml", Directory.GetCurrentDirectory());
            TwitterHelper helper = new TwitterHelper(null, null, MaxPainHelper.DebugStraddle(true), xmlFile);
            bool result = helper.InitializeXml().Result;

            //helper.DebugMode = true;
            TwitterMessage message = helper.Execute().Result;
            Assert.AreNotEqual(0, message.Status.Length);
        }

        [TestMethod]
        public void ChooseTicker()
        {
            string xmlFile = string.Format(@"{0}\json\twitter.xml", Directory.GetCurrentDirectory());
            TwitterHelper helper = new TwitterHelper(null, null, MaxPainHelper.DebugStraddle(false), xmlFile);

            XmlDocument xmlDom = new XmlDocument();
            xmlDom.Load(xmlFile);

            //helper.DebugMode = true;
            string ticker = helper.ChooseTicker(xmlDom, DateTime.Now, false).Result;
            Assert.AreNotEqual(0, ticker.Length);
        }

        [TestMethod]
        public void GetTweetsFromUser()
        {
            string xmlFile = string.Format(@"{0}\json\twitter.xml", Directory.GetCurrentDirectory());
            TwitterHelper helper = new TwitterHelper(null, null, MaxPainHelper.DebugStraddle(false), xmlFile);

            List<UserTweet> uts = helper.GetTweetsFromUser("super_trades",20).Result;
            Assert.AreNotEqual(0, uts.Count);
        }

        [TestMethod]
        public void GetTweets()
        {
            TwitterHelper helper = new TwitterHelper();

            SocialBots.TweetList list = helper.GetTweets(DateTime.Now, true, 10).Result; 
            Assert.AreEqual(10, list.Tweets.Count);
        }

        [TestMethod]
        public void GetChart()
        {
            string jsonFile = "json/ChartInfo.json";
            jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFile);
            string json = File.ReadAllText(jsonFile);
            ChartInfo info = (ChartInfo)Utility.DeserializeJson<ChartInfo>(json);
            
            byte[] buffer = ChartHelper.FetchImage(info).Result;

            OpenImageBytes(buffer);
        }

        [TestMethod]
        public void TwitterHelperChart()
        {
            OptChn chain = MaxPainHelper.DebugOptions(true);
            SdlChn sc = MaxPainHelper.BuildStraddle(chain);
            MPChain mpc = MaxPainHelper.Calculate(sc);

			string key = "Open Interest";
            ChartInfo info = ChartHelper.LineDouble(sc, key, key, Constants.DEFAULT_ZOOM);

            byte[] buffer = ChartHelper.FetchImage(info).Result;
            OpenImageBytes(buffer);
        }
		
        [TestMethod]
        public void ConvertLink()
        {
            string xmlFile = string.Format(@"{0}\json\twitter.xml", Directory.GetCurrentDirectory());
            TwitterHelper helper = new TwitterHelper(null, null, MaxPainHelper.DebugStraddle(false), xmlFile);

			string content = "RT @todd_harrison: In time, yes. https://t.co/NKGQzUZ3Lx";
            string result =  helper.ConvertLink(content);
            string expected = @"RT @todd_harrison: In time, yes. <a href=""https://t.co/NKGQzUZ3Lx"">https://t.co/NKGQzUZ3Lx</a>";
            Assert.AreEqual(expected, result);
        }		


        private bool OpenImageBytes(byte[] buffer)
        {
            string imageFile = "twitter.png";
            imageFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageFile);
            File.WriteAllBytes(imageFile, buffer);
            return OpenImageFile(imageFile);
        }

        private bool OpenImageFile(string file)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "mspaint.exe";
            startInfo.Arguments = file;
            //startInfo.Verb = "edit";
            System.Diagnostics.Process.Start(startInfo);
            return true;
        }
    }
}