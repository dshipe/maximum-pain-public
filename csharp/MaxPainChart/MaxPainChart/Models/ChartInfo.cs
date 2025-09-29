using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaxPainChart.Models
{
    public class ChartInfo
    {
        public string ChartType { get; set; }
        public string Title { get; set; }
        public string HAxisTitle { get; set; }
        public string HAxisFormat { get; set; }
        public string VAxisTitle { get; set; }
        public string VAxisFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Interval { get; set; }
        public bool Enable3D { get; set; }
        public bool IsDarkMode { get; set; }
        public bool IsTransparent { get; set; }

        public List<ChartSeries> Series { get; set; }

        public List<object> DataArray { get; set; }

        public ChartInfo()
        {
            Series = new List<ChartSeries>();
            Width = 1200;
            Height = 600;
            Interval = 5;
            Enable3D = false;
        }
    }

    public class ChartSeries
    {
        public string Title { get; set; }
        public string Color { get; set; }
        public List<ChartPoint> Points { get; set; }

        public ChartSeries()
        {
            Points = new List<ChartPoint>();
        }
    }

    public class ChartPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ChartPoint()
        {
        }
        public ChartPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}