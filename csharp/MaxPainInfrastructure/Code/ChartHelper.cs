using MaxPainInfrastructure.Models;
using ScottPlot;
using System.Drawing;
using System.Net;

namespace MaxPainChart
{
    public enum DataType
    {
        Open_Interest = 1,
        Volume = 2,
        Max_Pain = 3,
        Implied_Volatility = 4,
        Delta = 5,
        Gamma = 6,
        Theta = 7,
        Vega = 8,
        Rho = 9
    }

    public class ChartHelper
    {
        #region Chart
        public static byte[] RenderChart(ChartInfo info)
        {
            var plot = new Plot();

            // Add series
            foreach (ChartSeries series in info.Series)
            {
                double[] xValues = series.Points.Select(p => double.TryParse(p.X, out var x) ? x : 0).ToArray();
                double[] yValues = series.Points.Select(p => double.TryParse(p.Y, out var y) ? y : 0).ToArray();

                System.Drawing.Color seriesColor = ColorTranslator.FromHtml(series.Color);

                if (info.ChartType.ToLower().Equals("stackedcolumn"))
                {
                    var bar = plot.Add.Bars(xValues, yValues);
                    bar.Color = ScottPlot.Color.FromColor(seriesColor);
                    bar.LegendText = series.Title;
                }
                else
                {
                    var scatter = plot.Add.Scatter(xValues, yValues);
                    scatter.Color = ScottPlot.Color.FromColor(seriesColor);
                    scatter.LineWidth = 2;
                    scatter.LegendText = series.Title;
                }
            }

            // Set axis labels and title
            plot.XLabel(info.HAxisTitle);
            plot.YLabel(info.VAxisTitle);
            plot.Title(info.Title);

            // Show legend
            plot.ShowLegend();

            // Apply dark mode if needed
            if (info.IsDarkMode)
            {
                plot.FigureBackground.Color = ScottPlot.Color.FromHex("#DEE2E6");
                plot.DataBackground.Color = ScottPlot.Color.FromHex("#DEE2E6");
                plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#404040");
            }

            // Render to byte array
            byte[] imageBytes = plot.GetImage(info.Width, info.Height).GetImageBytes();

            if (info.IsTransparent)
            {
                //return MakeTransparent(info, imageBytes, info.IsDarkMode);
            }

            return imageBytes;
        }
        #endregion

        #region transparency
        /*
        private static byte[] MakeTransparent(ChartInfo info, byte[] imageBytes, bool isDarkMode)
        {
            System.Drawing.Color backColor = isDarkMode ? System.Drawing.Color.FromArgb(222, 226, 230) : System.Drawing.Color.White;

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                Bitmap bmp = new Bitmap(ms);
                bmp.MakeTransparent(backColor);
                using (MemoryStream outMs = new MemoryStream())
                {
                    bmp.Save(outMs, System.Drawing.Imaging.ImageFormat.Png);
                    return outMs.ToArray();
                }
            }
        }
        */
        #endregion

        #region Data
        public static DataType GetDataType(string key)
        {
            key = key.Replace(" ", "_");
            DataType result;
            Enum.TryParse(key, out result);
            return result;
        }

        public static ChartInfo FetchChartInfo(DataType dataType, string domain, string ticker, DateTime? maturity)
        {
            string json = FetchJson(dataType, domain, ticker, maturity);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ChartInfo>(json)!;
        }

        public static string FetchJson(DataType dataType, string domain, string ticker, DateTime? maturity)
        {
            string url = FetchUrl(dataType, domain, ticker, maturity);
            return Scrape(url);
        }

        public static string FetchUrl(DataType type, string domain, string ticker, DateTime? maturity)
        {
            string root = string.Format("http://{0}/api/chartinfo", domain);

            string format = string.Empty;
            switch (type)
            {
                case DataType.Open_Interest: format = "{0}/openinterest/{1}"; break;
                case DataType.Volume: format = "{0}/volume/{1}"; break;
                case DataType.Max_Pain: format = "{0}/maxpain/{1}"; break;
                default: format = "{0}/line/{1}?key"; break;
            }

            string url = string.Format(format, root, System.Web.HttpUtility.UrlEncode(ticker));

            if (maturity != null)
            {
                DateTime m = Convert.ToDateTime(maturity);
                url = string.Format("{0}?m={1}", url, System.Web.HttpUtility.UrlEncode(m.ToString("MM/dd/yyyy")));
            }

            return url;
        }

        #pragma warning disable SYSLIB0014
        private static string Scrape(string url)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            string data = string.Empty;
            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), System.Text.Encoding.UTF8))
            {
                data = sr.ReadToEnd();
                sr.Close();
            }
            return data;
        }
        #pragma warning restore SYSLIB0014
        #endregion
    }
}
