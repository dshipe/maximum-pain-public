using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class OptionTests : BaseTests
    {
        [TestMethod]
        public async Task OptionChain()
        {
            OptChn chain = await FinDataSvc.FetchOptionData("QQQ");
            string json = DBHelper.Serialize(chain);
            string jsonFile = string.Format(@"{0}\json\OptionChain.json", Directory.GetCurrentDirectory());
            System.IO.File.WriteAllText(jsonFile, json.Replace("}", "}\r\n"));

            string xml = Utility.SerializeXml<OptChn>(chain);
            string xmlFile = string.Format(@"{0}\json\OptionChain.xml", Directory.GetCurrentDirectory());
            System.IO.File.WriteAllText(xmlFile, xml.Replace("/>", "/>\r\n"));

            Assert.AreNotEqual(chain.Options.Count, 0);
        }

        [TestMethod]
        public async Task StraddleChain()
        {
            //OptChn chain = MaxPainHelper.DebugOptions(false);

            OptChn chain = await FinDataSvc.FetchOptionData("qqq");
            string json = DBHelper.Serialize(chain);
            string jsonFile = string.Format(@"{0}\json\OptionChain.json", Directory.GetCurrentDirectory());
            System.IO.File.WriteAllText(jsonFile, json.Replace("}", "}\r\n"));

            SdlChn sc = CalculationSvc.BuildStraddle(chain);
            json = DBHelper.Serialize(sc);
            jsonFile = Path.Combine(TestHelper.ProjectPath(), "json", "StraddleChain.json");
            System.IO.File.WriteAllText(jsonFile, json.Replace("}", "}\r\n"));

            Assert.AreNotEqual(sc.Straddles.Count, 0);
        }

        public async Task StraddleChainController()
        {
            //OptChn chain = MaxPainHelper.DebugOptions(false);

            OptChn chain = await FinDataSvc.FetchOptionChain("SPX", DateTime.MinValue);
            string json = DBHelper.Serialize(chain);
            string jsonFile = string.Format(@"{0}\json\OptionChain.json", Directory.GetCurrentDirectory());
            System.IO.File.WriteAllText(jsonFile, json.Replace("}", "}\r\n"));

            SdlChn sc = CalculationSvc.BuildStraddle(chain);
            json = DBHelper.Serialize(sc);
            jsonFile = Path.Combine(TestHelper.ProjectPath(), "json", "StraddleChain.json");
            System.IO.File.WriteAllText(jsonFile, json.Replace("}", "}\r\n"));

            Assert.AreNotEqual(sc.Straddles.Count, 0);
        }


        [TestMethod]
        public async Task MaxPain()
        {
            OptChn chain = await FinDataSvc.FetchOptionData("GE");
            chain = CalculationSvc.FilterOptionChain(chain);

            SdlChn sc = CalculationSvc.BuildStraddle(chain);
            MPChain mpc = CalculationSvc.Calculate(sc);

            string json = DBHelper.Serialize(mpc);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "MaxPain.json");
            System.IO.File.WriteAllText(file, json.Replace("}", "}\r\n"));

            Assert.AreNotEqual(mpc.Items.Count, 0);
        }

        /*
        [TestMethod]
        public void Spread()
        {
            SdlChn sc = MaxPainHelper.DebugStraddle(true);

            List<Spread> spreads = MaxPainHelper.BuildSpread(sc);

            string json = DBHelper.Serialize(spreads);
            string file = Path.Combine(ProjectPath(), "json", "Spreads.json");
            System.IO.File.WriteAllText(file, json.Replace("}", "}\r\n"));

            string csv = DumpSpread(spreads);
            file = Path.Combine(ProjectPath(), "json", "Spreads.csv");
            System.IO.File.WriteAllText(file, csv);

            Assert.AreNotEqual(0, spreads.Count);
        }
        */

        [TestMethod]
        public async Task ChartOpenInterest()
        {
            string key = "Open Interest";

            SdlChn sc = await GetSdlChn("AAPL");
            ChartInfo info = ChartSvc.LineDouble(sc, key, key, Constants.DEFAULT_ZOOM);
            string json = DBHelper.Serialize(info);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "ChartInfo.json");

            byte[] buffer = ChartSvc.FetchImage(info).Result;
            TestHelper.OpenImageBytes(buffer);
            Assert.AreNotEqual(null, info);
        }

        [TestMethod]
        public async Task ChartVolume()
        {
            string key = "Volume";

            SdlChn sc = await GetSdlChn("AAPL");
            ChartInfo info = ChartSvc.LineDouble(sc, key, key, Constants.DEFAULT_ZOOM);
            string json = DBHelper.Serialize(info);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "ChartInfo.json");

            byte[] buffer = ChartSvc.FetchImage(info).Result;
            TestHelper.OpenImageBytes(buffer);
            Assert.AreNotEqual(null, info);
        }

        [TestMethod]
        public async Task ChartMaxPain()
        {
            SdlChn sc = await GetSdlChn("AAPL");
            MPChain mpc = CalculationSvc.Calculate(sc);
            string json = DBHelper.Serialize(mpc);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "MPChain.json");

            ChartInfo info = ChartSvc.MaxPain(mpc);
            json = DBHelper.Serialize(info);
            file = Path.Combine(TestHelper.ProjectPath(), "json", "ChartInfo.json");

            byte[] buffer = ChartSvc.FetchImage(info).Result;
            TestHelper.OpenImageBytes(buffer);
            Assert.AreNotEqual(null, info);
        }

        [TestMethod]
        public async Task ChartIVStdDev()
        {
            SdlChn sc = await GetSdlChn("AAPL");

            ChartInfo info = ChartSvc.LineDouble(sc, "IV Std Dev Range", "1sd", Constants.DEFAULT_ZOOM);
            string json = DBHelper.Serialize(info);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "ChartInfo.json");

            byte[] buffer = ChartSvc.FetchImage(info).Result;
            TestHelper.OpenImageBytes(buffer);
            Assert.AreNotEqual(null, info);
        }

        [TestMethod]
        public async Task ChartIVPredict()
        {
            SdlChn sc = await GetSdlChn("AAPL");

            ChartInfo info = ChartSvc.IVPredict(sc, "IV Std Dev Range", "1sd", 1);
            string json = DBHelper.Serialize(info);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "ChartInfo.json");

            byte[] buffer = ChartSvc.FetchImage(info).Result;
            TestHelper.OpenImageBytes(buffer);
            Assert.AreNotEqual(null, info);
        }

        [TestMethod]
        public async Task CallPrice()
        {
            SdlChn sc = await GetSdlChn("AAPL");

            ChartInfo info = ChartSvc.Price(sc, "Call Prices", false);
            string json = DBHelper.Serialize(info);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "ChartInfo.json");

            byte[] buffer = ChartSvc.FetchImage(info).Result;
            TestHelper.OpenImageBytes(buffer);
            Assert.AreNotEqual(null, info);
        }



        private async Task<SdlChn> GetSdlChn(string ticker)
        {
            OptChn chain = await FinDataSvc.FetchOptionData(ticker);
            chain = CalculationSvc.FilterOptionChain(chain);
            SdlChn sc = CalculationSvc.BuildStraddle(chain);

            string json = DBHelper.Serialize(sc);
            string file = Path.Combine(TestHelper.ProjectPath(), "json", "SdlChn.json");

            return sc;
        }

        public static string DumpSpread(List<Spread> spreads)
        {
            string rowFormat = "\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"";
            string csv = string.Format(rowFormat, "Ticker", "Maturity", "Type", "Long Strike", "Long Bid", "Short Strike", "Short Ask", "Difference", "Profit", "Cost");

            IEnumerable<DateTime> maturities = spreads.Select(x => x.Maturity).Distinct();
            foreach (DateTime maturity in maturities)
            {
                csv = string.Concat(csv, "\r\n");
                List<Spread> calls = spreads.FindAll(x => x.Maturity == maturity && x.OptionType == OptionTypes.Call).ToList();
                foreach (Spread spread in calls)
                {
                    rowFormat = "{0}\r\n\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\"";
                    csv = string.Format(rowFormat, csv, spread.Ticker, spread.Maturity.ToString("MM/dd/yy"), spread.OptionType, spread.LongStrike, spread.LongPrice, spread.ShortStrike, spread.ShortPrice, spread.Value, spread.Cost, spread.ROI);
                }

                csv = string.Concat(csv, "\r\n");
                List<Spread> puts = spreads.FindAll(x => x.Maturity == maturity && x.OptionType == OptionTypes.Put).ToList();
                foreach (Spread spread in puts)
                {
                    rowFormat = "{0}\r\n\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\"";
                    csv = string.Format(rowFormat, csv, spread.Ticker, spread.Maturity.ToString("MM/dd/yy"), spread.OptionType, spread.LongStrike, spread.LongPrice, spread.ShortStrike, spread.ShortPrice, spread.Value, spread.Cost, spread.ROI);
                }
            }

            return csv;
        }
    }
}
