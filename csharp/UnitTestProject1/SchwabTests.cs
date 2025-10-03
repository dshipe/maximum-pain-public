using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Models.Schwab;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class SchwabTests : BaseTests
    {
        private string _consumerKey = string.Empty;
        private string _responseUrl = string.Empty;
        private ScwToken _token = null;

        #region constructor
        public SchwabTests()
        {
            _responseUrl = ConfigurationSvc.Get("SchwabResponseUrl").Result;
            string json = ConfigurationSvc.Get("SchwabTokens").Result;

            string outputFile = string.Format(@"{0}\json\SchwabTokens.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, json);

            _token = DBHelper.Deserialize<ScwToken>(json);
        }

        private async Task<string> CreateSetting()
        {
            string file = string.Format(@"{0}\json\SchwabTokens.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(file);

            return await ConfigurationSvc.ReadXml();
        }

        [TestMethod]
        public async Task CreateSettingTest()
        {
            string xml = await CreateSetting();
            Assert.AreNotEqual(-1, xml.IndexOf("{"));
        }
        #endregion

        /*
        [TestMethod]
        public void FirstToken()
        {
            Initialize();

            FinanceDataConsumer.TDAmeritrade tda = new FinanceDataConsumer.TDAmeritrade();
            string code = tda.DecodeUrl(_responseUrl);
            //TestHelper.OpenInNotepad(code);

            Tokens tokens = tda.GetFirstTokens(_consumerKey, code).Result;

            Assert.AreNotEqual(0, tokens.access_token.Length);
        }

        [TestMethod]
        public void RefreshToken()
        {
            Initialize();

            FinanceDataConsumer.TDAmeritrade tda = new FinanceDataConsumer.TDAmeritrade();
            string code = tda.DecodeUrl(_responseUrl);
            //TestHelper.OpenInNotepad(code);

            Tokens tokens = tda.GetRefreshTokens(_consumerKey, code, _token.refresh_token).Result;

            string outputFile = string.Format(@"{0}\json\TDATokens.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, MaxPainAPI.Code.DBHelper.Serialize(tokens));
            
            Assert.AreNotEqual(0, tokens.access_token.Length);
        }
        */

        [TestMethod]
        public async Task RefreshToken()
        {
            _token = await SchwabSvc.UpdateToken(_token);
            var json = DBHelper.Serialize(_token);
            string outputFile = string.Format(@"{0}\json\schwab\tokens.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, json);

            // save to settings
            await ConfigurationSvc.Set("SchwabTokens", json);
        }

        [TestMethod]
        public async Task Expirations()
        {
            await RefreshToken();

            ScwExpirationList expList = await SchwabSvc.GetExpirations(_token.access_token, "$spx");
            var outputFile = string.Format(@"{0}\json\schwab\ExpirationList.json", Directory.GetCurrentDirectory());
            var json = DBHelper.Serialize(expList);
            File.WriteAllText(outputFile, json);

            Assert.AreNotEqual(0, json.Length);
        }

        [TestMethod]
        public async Task Options()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            await RefreshToken();

            OptChn chain = await SchwabSvc.GetOptions(_token.access_token, "spx");
            //OptChn chain = await SchwabSvc.GetOptions(_token.access_token, "bac");

            timer.Stop();
            OpenInNotepad($"Date={DateTime.Now}\r\ntimer={timer.ElapsedMilliseconds}");

            var json = DBHelper.Serialize(chain);
            var outputFile = string.Format(@"{0}\json\schwab\optionchain.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, json);
            //OpenInNotepad(json);

            Assert.AreNotEqual(0, chain.Options.Count);
        }

        [TestMethod]
        public async Task OptionsCSV()
        {
            await RefreshToken();

            string ticker = "BAC";
            List<ScwOptionCSV> list = await SchwabSvc.GetOptionsCSV(_token.access_token, ticker);
            var outputFile = string.Format(@"{0}\json\schwab\optionchain.json", Directory.GetCurrentDirectory());
            var json = DBHelper.Serialize(list);
            File.WriteAllText(outputFile, json);

            Assert.AreNotEqual(0, json.Length);
        }

        [TestMethod]
        public void ParseOptions()
        {
            string root = Directory.GetCurrentDirectory();

            string inputFile = string.Format(@"{0}\json\schwab\optionchain-raw.json", root);
            string json = File.ReadAllText(inputFile);

            var chain = SchwabSvc.ParseOptions(json);
            string outputFile = string.Format(@"{0}\json\schwab\optionchain-parsed.json", root);
            json = DBHelper.Serialize(chain);
            File.WriteAllText(outputFile, json);

            var optChn = SchwabSvc.MapOptions(chain);
            outputFile = string.Format(@"{0}\json\schwab\optionchain-maxpain.json", root);
            json = DBHelper.Serialize(optChn);
            File.WriteAllText(outputFile, json);

            Assert.AreNotEqual(0, optChn.Options.Count);
        }

        [TestMethod]
        public void ParseSymbol()
        {
            ScwOptionSymbol os = SchwabSvc.ParseSymbol("AAPL  240503C00100000");

            string expected = "AAPL240503C00100000";
            Assert.AreEqual(expected, os.ticker);
        }

        [TestMethod]
        public async Task Stocks()
        {
            await RefreshToken();

            List<Stock> collection = await SchwabSvc.GetStocks(_token.access_token, "A,ABC,AAL,AAPL,ABBV,ABNB,ABT,ACGL,ACN");
            var outputFile = string.Format(@"{0}\json\schwab\stocks.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, DBHelper.Serialize(collection));

            Assert.AreNotEqual(0, collection[0].symbol.Length);
        }

        [TestMethod]
        public void ParseStocks()
        {
            string root = Directory.GetCurrentDirectory();

            string inputFile = string.Format(@"{0}\json\schwab\stocks-raw.json", root);
            string json = File.ReadAllText(inputFile);

            var quotes = SchwabSvc.ParseStocks(json);
            string outputFile = string.Format(@"{0}\json\schwab\stocks-parsed.json", root);
            json = DBHelper.Serialize(quotes);
            File.WriteAllText(outputFile, json);

            List<Stock> stocks = SchwabSvc.MapStocks(quotes);
            outputFile = string.Format(@"{0}\json\schwab\stocks-maxpain.json", root);
            json = DBHelper.Serialize(stocks);
            File.WriteAllText(outputFile, json);

            Assert.AreNotEqual(0, stocks.Count);
        }


        [TestMethod]
        public async Task IsMarketOpen()
        {
            await RefreshToken();
            var result = await SchwabSvc.IsMarketOpen(_token.access_token, DateTime.Now);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task WatchList()
        {
            var json = await ConfigurationSvc.Get("SchwabTokens");
            _token = DBHelper.Deserialize<ScwToken>(json);

            var ws = await SchwabSvc.GetWatchList(_token.access_token, "1");

            var result = await SchwabSvc.CreateWatchlist(_token.access_token, "W250930", new string[] { "APP", "ORCL" });
            Assert.AreNotEqual(true, result.Length == 0);
        }

        [TestMethod]
        public void UnixTime()
        {
            long unix = 1585938483371;
            DateTime actual = Utility.UnixTimestampToDateTime(unix);
            DateTime expected = Convert.ToDateTime("4/3/2020 18:28:03.371");
            Assert.AreEqual(expected, actual);
        }
    }
}