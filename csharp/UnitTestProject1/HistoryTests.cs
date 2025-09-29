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
    public class HistoryTests : BaseTests
    {
        [TestMethod]
        public async Task GetByTicker()
        {
            var optChn = await HistorySvc.GetByTicker("INTC", 15, 7);
            string jsonFile = string.Format(@"{0}\json\GetByTicker.json", Directory.GetCurrentDirectory());
            File.WriteAllText(jsonFile, DBHelper.Serialize(optChn));

            Assert.AreNotEqual(0, optChn.Options.Count);
        }

        [TestMethod]
        public void ChartHistory()
        {
            string key = "Open Interest";

            string jsonFile = string.Format(@"{0}\json\OptionHistory.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(jsonFile);

            OptChn chain = DBHelper.Deserialize<OptChn>(json);
            FilterOptionHistory(chain, DateTime.MinValue);
            string outputFile = string.Format(@"{0}\json\chn.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, DBHelper.Serialize(chain).Replace("},", "},\r\n"));

            SdlChn sc = CalculationSvc.BuildStraddle(chain);
            outputFile = string.Format(@"{0}\json\sdl.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, DBHelper.Serialize(sc).Replace("},", "},\r\n"));

            ChartInfo info = ChartSvc.HistoryLineDouble(sc, key, key);
            outputFile = string.Format(@"{0}\json\ChartInfo-ChartHistory.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, DBHelper.Serialize(info));

            byte[] buffer = ChartSvc.FetchImage(info).Result;
            TestHelper.OpenImageBytes(buffer);

            Assert.AreNotEqual(0, sc.Straddles.Count);
            Assert.AreNotEqual(0, info.Title.Length);
        }

        [TestMethod]
        public void ChartMaxPain()
        {
            string jsonFile = string.Format(@"{0}\json\MaxPainHistory.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(jsonFile);
            List<MaxPainHistory> all = DBHelper.Deserialize<List<MaxPainHistory>>(json);
            string m = "08/30/2019";
            List<MaxPainHistory> quotes = all.Where(x => x.M == m).ToList();
            ChartInfo info = ChartSvc.HistoryMaxPain(quotes, true);

            string outputFile = string.Format(@"{0}\json\ChartInfo.json", Directory.GetCurrentDirectory());
            File.WriteAllText(outputFile, DBHelper.Serialize(info));

            Assert.AreNotEqual(0, info.Title.Length);
        }


        private bool FilterOptionHistory(OptChn chain, DateTime maturity)
        {
            int m = Utility.DateToYMD(maturity);
            if (maturity == DateTime.MinValue) m = chain.Options.Select(x => x.Mint()).Min();
            decimal s = chain.Options.Select(x => x.Strike()).Min();

            List<Opt> options = chain.Options
                .FindAll(x => x.Mint() == m && x.Strike() == s)
                .OrderBy(x => x.d)
                .ToList();
            chain.Options = options;
            return true;
        }
    }
}