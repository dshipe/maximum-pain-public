using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace UnitTestProject1
{
    [TestClass]
    public class FinImportTests : BaseTests
    {
        [TestMethod]
        public void Run()
        {
            string jsonFile = string.Format(@"{0}\json\OptionChain.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(jsonFile);
            OptChn chain = DBHelper.Deserialize<OptChn>(json);

            FinImportSvc.UnitTestChain = chain;
            FinImportSvc.IsDebug = true;
            FinImportSvc.UseMessage = false;
            FinImportSvc.UseMostActiveCode = true;
            FinImportSvc.RunImport();
        }

        [TestMethod]
        public async Task Stocks()
        {
            List<Stock> stocks = await ControllerSvc.GetStocks();
            Assert.AreNotEqual(0, stocks.Count);
        }

        [TestMethod]
        public void Serialize()
        {
            OptChn chain = CalculationSvc.DebugOptions(false);
            string xml = Utility.SerializeXml<OptChn>(chain);
            xml = xml.Replace("/>", "/>\r\n");

            string jsonFile = string.Format(@"{0}\json\history.xml", Directory.GetCurrentDirectory());
            Utility.SaveFile(jsonFile, xml);

            Assert.AreNotEqual(0, xml.Length);
        }

        [TestMethod]
        public void DoTransform()
        {
            string xml = ""; //engine.DoTransform().Result;

            string xmlFile = string.Format(@"{0}\json\transform.xml", Directory.GetCurrentDirectory());
            File.WriteAllText(xmlFile, xml);

            XmlDocument xmlDom = new XmlDocument();
            xmlDom.LoadXml(xml);
            Assert.AreEqual("OptChn", xmlDom.DocumentElement.Name);
        }
    }
}
