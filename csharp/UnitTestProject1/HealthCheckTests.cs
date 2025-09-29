using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;

namespace UnitTestProject1
{
    [TestClass]
    public class HealthCheckTests : BaseTests
    {

        [TestMethod]
        public void ExpectedOptionDate()
        {
            DateTime test = Convert.ToDateTime("11/11/2019 14:00");
            test = ControllerSvc.ExpectedOptionDate(test);
            DateTime expected = Convert.ToDateTime("11/8/2019");
            Assert.AreEqual(expected, test);

            test = Convert.ToDateTime("11/12/2019 14:00");
            test = ControllerSvc.ExpectedOptionDate(test);
            expected = Convert.ToDateTime("11/11/2019");
            Assert.AreEqual(expected, test);

            test = Convert.ToDateTime("11/11/2019 4:00");
            test = ControllerSvc.ExpectedOptionDate(test);
            expected = Convert.ToDateTime("11/8/2019");
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void RunHealthCheck()
        {
            XmlDocument xmlSettings = new XmlDocument();
            xmlSettings.LoadXml("<Settings><ScreenerLastRun>12/16/19 14:01:32</ScreenerLastRun><HealthCheckLastRun>12/16/19 23:24:15</HealthCheckLastRun></Settings>");

            string xsltFile = @"C:\VSProjects\MaxPain\MaxPainAPI\maximum-pain.com\MaxPainAPI\xslt\HealthCheck.xslt";
            bool result = ControllerSvc.HealthCheckUnitTest(xsltFile, xmlSettings, false).Result;

            Assert.AreEqual(true, result);
        }

        private XmlDocument CreateDOM()
        {
            string xml = @"
                <HealthChecks>
                    <HealthCheck Name=""Maximum-Pain.com Option Data"" HasError=""False"" Description=""Test for AAPL option data"" />
                    <HealthCheck Name=""Home DB"" HasError =""True"" Description=""Check the MostActive SP from the Home DB"" />
                </HealthChecks>
            ";
            XmlDocument xmlDom = new XmlDocument();
            xmlDom.LoadXml(xml);
            return xmlDom;
        }

    }
}
