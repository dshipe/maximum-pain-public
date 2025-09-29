using MaxPainInfrastructure.Code;

namespace MaxPainInfrastructure.Models
{
    public class ChartInfo
    {
        public string? ChartType { get; set; }
        public string? DataType { get; set; }
        public string? Title { get; set; }
        public string? HAxisTitle { get; set; }
        public string? HAxisFormat { get; set; }
        public string? VAxisTitle { get; set; }
        public string? VAxisFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Interval { get; set; }
        public bool Enable3D { get; set; }

        //public List<string> SeriesNames {get; set;}
        //public List<string> ValueNames {get; set;}
        //public List<string> Color {get; set;}

        public List<ChartSeries> Series { get; set; }

        /*
        public List<object> DataArraySimple
        {
            get
            {
                List<object> objs = new List<object>();
                objs.Add(new[] { "strike", "call", "put" });
                for (int i = 0; i < Series[0].Points.Count; i++)
                {
                    double x = Convert.ToDouble(Series[0].Points[i].X);
                    double y1 = Convert.ToDouble(Series[0].Points[i].Y);
                    double y2 = Convert.ToDouble(Series[1].Points[i].Y);
                    objs.Add(new[] { x, y1, y2 });
                }
                return objs;
            }
            set
            {
                DataArraySimple = value;
            }
        }
        */

        public List<object> DataArray
        {
            get
            {
                List<object> objs = new List<object>();

                // build the header
                List<string> header = new List<string>();
                header.Add(HAxisTitle);
                foreach (ChartSeries s in this.Series)
                {
                    header.Add(s.Title);
                }
                objs.Add(header.ToArray());

                // build rows for numeric X axis
                for (int i = 0; i < Series[0].Points.Count; i++)
                {
                    // set X axis value
                    List<double> row = new List<double>();
                    double x = 0;
                    if (this.DataType == "number")
                    {
                        x = Convert.ToDouble(Series[0].Points[i].X);
                    }
                    if (this.DataType == "date")
                    {
                        x = Utility.DateTimeToUnixTimestamp(Series[0].Points[i].X);
                    }
                    row.Add(x);

                    // set Y axis values
                    foreach (ChartSeries s in this.Series)
                    {
                        if (i < s.Points.Count)
                        {
                            double y = Convert.ToDouble(s.Points[i].Y);
                            row.Add(y);
                        }
                    }
                    objs.Add(row.ToArray());
                }

                return objs;
            }
            set
            {
                DataArray = value;
            }
        }

        public ChartInfo()
        {
            Series = new List<ChartSeries>();
            Width = 1200;
            Height = 600;
            Interval = 5;
            Enable3D = false;
            DataType = "number";
        }
    }

    public class ChartSeries
    {
        public string? Title { get; set; }
        public string? Color { get; set; }
        public List<ChartPoint> Points { get; set; }

        public ChartSeries()
        {
            Points = new List<ChartPoint>();
        }
    }

    public class ChartPoint
    {
        public string? X { get; set; }
        public string? Y { get; set; }

        public ChartPoint()
        {
        }

        public ChartPoint(string x, string y)
        {
            X = x;
            Y = y;
        }
    }
}
