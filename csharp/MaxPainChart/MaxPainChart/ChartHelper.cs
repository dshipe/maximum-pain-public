using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.UI.DataVisualization.Charting;
using System.Xml;
using System.Data;
using System.IO;

using MaxPainChart.Models;
using System.Drawing;

namespace MaxPainChart
{
    public enum DataType
    {
        Open_Interest = 1
        , Volume = 2
        , Max_Pain = 3
        , Implied_Volatility = 4
        , Delta = 5
        , Gamma = 6
        , Theta = 7
        , Vega = 8
        , Rho = 9
    }

    public class ChartHelper
    {
        #region Chart
        public static byte[] RenderChart(ChartInfo info)
        {
            //info.IsDarkMode = true;

            Color backColor = Color.White;
            Color gridColor = Color.LightGray;
            Color foreColor = Color.Black;
            if (info.IsDarkMode)
            {
                //backColor = Color.Black;
                backColor = Color.FromArgb(222, 226, 230);
                gridColor = Color.FromArgb(255, 64, 64, 64);
                foreColor = Color.FromArgb(255, 128, 128, 128);
            }

            Chart myChart = new System.Web.UI.DataVisualization.Charting.Chart();

            SeriesChartType type = SeriesChartType.Line;
            if (info.ChartType.ToLower().Equals("line")) type = SeriesChartType.Line;
            if (info.ChartType.ToLower().Equals("stackedcolumn")) type = SeriesChartType.StackedColumn;

            myChart.Width = info.Width;
            myChart.Height = info.Height;

            // define chart area
            myChart.ChartAreas.Add("Main");
            ChartArea ca = myChart.ChartAreas["Main"];
            ca.Position.Height = 100;
            ca.Position.Width = 100;
            ca.Position.X = 0;
            ca.Position.Y = 10;

            // define X axis
            ca.AxisX.MajorTickMark.Enabled = false;
            ca.AxisX.IsLabelAutoFit = true;
            ca.AxisX.LabelStyle.Angle = -90;
            ca.AxisX.MajorGrid.Enabled = true;
            ca.AxisX.MajorGrid.LineColor = gridColor;
            ca.AxisX.TitleForeColor = foreColor;
            ca.AxisX.LabelStyle.ForeColor = foreColor;

            System.Drawing.Font f = GetChartFont();
            ca.AxisX.LabelStyle.Font = f;
            ca.AxisX.Title = info.HAxisTitle;
            ca.AxisX.LabelStyle.Format = info.HAxisFormat;
            ca.AxisX.IsMarginVisible = false;
            if(info.Interval!=null && info.Interval>0)
            {
                ca.AxisX.LabelStyle.Interval = info.Interval;
                ca.AxisX.MajorGrid.Interval = info.Interval;
            }

            // define Y axis
            ca.AxisY.IsLabelAutoFit = true;
            ca.AxisY.MajorGrid.Enabled = true;
            ca.AxisY.MajorGrid.LineColor = gridColor;
            ca.AxisY.TitleForeColor = foreColor;
            ca.AxisY.LabelStyle.ForeColor = foreColor;

            ca.AxisY.LabelStyle.Font = f;
            ca.AxisY.Title = info.VAxisTitle;
            ca.AxisY.LabelStyle.Format = info.VAxisFormat;
            ca.AxisY.IsMarginVisible = false;

            // legend
            myChart.Legends.Add("leg");
            myChart.Legends[0].Enabled = true;
            myChart.Legends[0].Alignment = System.Drawing.StringAlignment.Near;
            myChart.Legends[0].Font = GetChartFont();
            myChart.Legends[0].ForeColor = foreColor;

            // create series
            foreach (ChartSeries series in info.Series)
            {
                myChart.Series.Add(series.Title);
                myChart.Series[series.Title].ChartType = type;
                myChart.Series[series.Title].Color = System.Drawing.ColorTranslator.FromHtml(series.Color);
                myChart.Series[series.Title].ChartArea = "Main";
                myChart.Series[series.Title].BorderWidth = 2;

                //for (int i = 0; i < info.Series[0].Points.Count; i++)
                for (int i = 0; i < series.Points.Count; i++)
                {
                    double x = series.Points[i].X;
                    double y = series.Points[i].Y;
                    myChart.Series[series.Title].Points.AddXY(x,y);
                }
            }

            Title t = new Title();
            myChart.Titles.Add(t);
            t.Font = GetChartFont();
            t.Alignment = System.Drawing.ContentAlignment.TopLeft;
            t.Text = info.Title;
            t.ForeColor = foreColor;

            myChart.ChartAreas[0].Area3DStyle.Enable3D = info.Enable3D;

            if (info.IsTransparent)
            {
                //backColor = Color.FromArgb(255, 1, 1, 1);
                myChart.BackColor = backColor;
                ca.BackColor = backColor;
                myChart.Legends[0].BackColor = backColor;
                return MakeTransparent(info, myChart);
            }

            myChart.BackColor = backColor;
            ca.BackColor = backColor;
            myChart.Legends[0].BackColor = backColor;
            return ToByteArray(myChart);
        }

        private static System.Drawing.Font GetChartFont()
        {
            return new System.Drawing.Font("Verdana", 12.0F, System.Drawing.FontStyle.Regular);
        }
        #endregion

        #region transaprency
        private static byte[] MakeTransparent(ChartInfo info, System.Web.UI.DataVisualization.Charting.Chart chart1)
        {
            //Bitmap bmp = new Bitmap(chart1.ClientSize.Width, chart1.ClientSize.Height);
            Bitmap bmp = new Bitmap(info.Width, info.Height);

            Rectangle r = new Rectangle(0, 0, info.Width, info.Height);
            Graphics g = Graphics.FromImage(bmp);

            chart1.AntiAliasing = AntiAliasingStyles.Graphics;
            chart1.Paint(g, r);
            bmp.MakeTransparent(chart1.BackColor);

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.GetBuffer();
            }
        }
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
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ChartInfo>(json);
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
			
            string url = string.Format(format, root, HttpUtility.UrlEncode(ticker));

            if (maturity != null)
            {
                DateTime m = Convert.ToDateTime(maturity);
                url = string.Format("{0}?m={1}", url, HttpUtility.UrlEncode(m.ToString("MM/dd/yyyy")));
            }

            return url;
        }


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
        #endregion

        #region byte array
        private static byte[] ToByteArray(Chart myChart)
        {
            return ToByteArray(myChart, ChartImageFormat.Jpeg);
        }

        private static byte[] ToByteArray(Chart myChart, ChartImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                myChart.SaveImage(ms, format);
                return ms.GetBuffer();
            }
        }
        #endregion
    }
}