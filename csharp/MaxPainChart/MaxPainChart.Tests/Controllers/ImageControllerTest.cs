using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MaxPainChart;
using MaxPainChart.Controllers;
using MaxPainChart.Models;
using System.IO;
using System.Collections.Generic;

namespace MaxPainChart.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Screener()
        {
            // Arrange
            ImageController controller = new ImageController();

            // Act
            string ticker = "SPX";
            ViewResult result = controller.OpenInterest(ticker, null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void OpenInterest()
        {
            // Arrange
            ImageController controller = new ImageController();

            // Act
            string ticker = "AAPL";
            DateTime maturity = Convert.ToDateTime("05/17/2019");
            ViewResult result = controller.OpenInterest(ticker, maturity) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LocalOpenInterest()
        {
            string filename = string.Format(@"{0}\json\openinterest.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(filename);

            ChartInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChartInfo>(json);
            byte[] buffer = ChartHelper.RenderChart(info);
            OpenImageBytes(buffer);

            Assert.AreNotEqual(buffer.Length, 0);
        }

        [TestMethod]
        public void LocalVolume()
        {
            string filename = string.Format(@"{0}\json\volume.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(filename);

            ChartInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChartInfo>(json);
            byte[] buffer = ChartHelper.RenderChart(info);
            OpenImageBytes(buffer);

            Assert.AreNotEqual(buffer.Length, 0);
        }

        [TestMethod]
        public void LocalMaxPain()
        {
            string filename = string.Format(@"{0}\json\maxpain.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(filename);

            ChartInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<ChartInfo>(json);
            byte[] buffer = ChartHelper.RenderChart(info);
            OpenImageBytes(buffer);

            Assert.AreNotEqual(buffer.Length, 0);
        }

        [TestMethod]
        public void PostChartInfo()
        {
            string filename = string.Format(@"{0}\json\chartinfo.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(filename);

            ChartInfo info = (ChartInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(ChartInfo));
            byte[] buffer = ChartHelper.RenderChart(info);
            OpenImageBytes(buffer);

            Assert.AreNotEqual(buffer.Length, 0);
        }


        [TestMethod]
        public void PostChartInfoHistory()
        {
            string filename = string.Format(@"{0}\json\chartinfo-history.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(filename);

            ChartInfo info = (ChartInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(ChartInfo));
            byte[] buffer = ChartHelper.RenderChart(info);
            OpenImageBytes(buffer);

            Assert.AreNotEqual(buffer.Length, 0);
        }

        [TestMethod]
        public void PostChartInfoStacked()
        {
            string filename = string.Format(@"{0}\json\chartinfo-stacked.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(filename);

            ChartInfo info = (ChartInfo)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(ChartInfo));
            byte[] buffer = ChartHelper.RenderChart(info);
            OpenImageBytes(buffer);

            Assert.AreNotEqual(buffer.Length, 0);
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
