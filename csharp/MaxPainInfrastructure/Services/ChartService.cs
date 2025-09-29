using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using System.Data;
using System.Web;

namespace MaxPainInfrastructure.Services
{
    public class ChartService : IChartService
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ILoggerService _logger;
        private readonly IConfigurationService _configuration;
        private readonly ICalculationService _calculation;
        private readonly IFinDataService _finData;
        private readonly ISecretService _secretService;

        public ChartService(
            AwsContext awsContext,
            HomeContext homeContext,
            ILoggerService loggerService,
            IConfigurationService configurationService,
            ICalculationService _calculationService,
            IFinDataService finDataService,
            ISecretService secretService
            )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _logger = loggerService;
            _configuration = configurationService;
            _calculation = _calculationService;
            _finData = finDataService;
            _secretService = secretService;
        }

        #region filter
        public SdlChn FilterStraddle(SdlChn sc, bool useStockPrice, int numberOfValues)
        {
            if (numberOfValues == 0) return sc;

            try
            {
                if (sc.Straddles.Count <= numberOfValues) return sc;
                int half = numberOfValues / 2;

                // find the reference point
                decimal referencePoint = sc.StockPrice;
                if (!useStockPrice)
                {
                    MPChain mpc = _calculation.Calculate(sc);
                    referencePoint = mpc.MaxPain;
                }

                // locate the reference strike within the straddles
                Sdl centerItem = sc.Straddles.First(x => x.Strike() >= referencePoint);
                int center = sc.Straddles.IndexOf(centerItem);

                // use only the X number of strikes around the reference
                int min = center - (half - 1);
                List<Sdl> straddles = sc.Straddles.Skip(min).Take(numberOfValues).ToList();

                sc.Straddles = straddles;
            }
            catch
            {
                throw;
            }
            return sc;
        }

        /*
        public SdlChn FilterStraddleMultiMat(SdlChn sc, bool useStockPrice, int numberOfValues)
        {
            if (numberOfValues == 0) return sc;

            try
            {
                SdlChn? result = DBHelper.Deserialize<SdlChn>(DBHelper.Serialize(sc));
                result.Straddles = new List<Sdl>();

                List<int> mints = sc.Straddles
                   .GroupBy(s => s.Mint())
                   .Select(x => x.Key)
                   .ToList();

                foreach (int mint in mints)
                {
                    List<Sdl> filtered = sc.Straddles.FindAll(s => s.Mint() == mint).ToList();
                    if (filtered.Count > numberOfValues)
                    {
                        int half = numberOfValues / 2;

                        // find the reference point
                        decimal referencePoint = sc.StockPrice;

                        // locate the reference strike within the straddles
                        Sdl centerItem = filtered.First(x => x.Strike() >= referencePoint);
                        int center = filtered.IndexOf(centerItem);

                        // use only the X number of strikes around the reference
                        int min = center - (half - 1);
                        result.Straddles.AddRange(filtered.Skip(min).Take(numberOfValues).ToList());
                    }
                    else
                    {
                        result.Straddles.AddRange(filtered);
                    }
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        public MPChain FilterMaxPain(MPChain mpc, bool useStockPrice, int numberOfValues)
        {
            if (numberOfValues == 0) return mpc;

            try
            {
                if (mpc.Items.Count <= numberOfValues) return mpc;
                int half = numberOfValues / 2;

                // find the reference point
                decimal referencePoint = mpc.StockPrice;
                if (!useStockPrice)
                {
                    referencePoint = mpc.MaxPain;
                }

                // locate the reference strike
                MPItem centerItem = mpc.Items.First(x => x.s >= referencePoint);
                int center = mpc.Items.IndexOf(centerItem);

                // use only the X number of strikes around the reference
                int min = center - (half - 1);
                List<MPItem> filtered = mpc.Items.Skip(min).Take(numberOfValues).ToList();

                mpc.Items = filtered;
            }
            catch (Exception)
            {
            }
            return mpc;
        }
        */
        #endregion

        #region MaxPain
        public ChartInfo MaxPain(SdlChn sc)
        {
            MPChain mpc = _calculation.Calculate(sc);
            return MaxPain(mpc);
        }

        public ChartInfo MaxPain(MPChain mpc)
        {
            ChartInfo info = new ChartInfo();
            if (mpc.Items.Count == 0) return info;

            string ticker = mpc.Stock;
            string maturity = mpc.Maturity.ToString("MM/dd/yy");
            string modifiedOn = Utility.GMTToEST(mpc.CreatedOn).ToString(Constants.CHART_DATE_FORMAT);
            info.Title = string.Format("{0} Max Pain {1} maturity={2} created={3} https://{4}", ticker, mpc.MaxPain, maturity, modifiedOn, Constants.DOMAIN);

            info.ChartType = "stackedcolumn";
            info.Enable3D = false;
            info.Interval = Convert.ToInt32(SetChartInterval(mpc.Items[0].s, mpc.Items[mpc.Items.Count - 1].s));

            info.VAxisTitle = "Cash";
            info.VAxisFormat = "#,##0";

            info.HAxisTitle = "Strike";
            info.HAxisFormat = "#,##0";

            ChartSeries series = new ChartSeries();
            series.Title = "Call";
            series.Color = "#009900";
            foreach (MPItem item in mpc.Items)
            {
                ChartPoint point = new ChartPoint(item.s.ToString(), item.cch.ToString());
                series.Points.Add(point);
            }
            info.Series.Add(series);

            series = new ChartSeries();

            series.Title = "Put";

            series.Color = "#ff0000";
            foreach (MPItem item in mpc.Items)
            {
                ChartPoint point = new ChartPoint(item.s.ToString(), item.pch.ToString());
                series.Points.Add(point);
            }
            info.Series.Add(series);

            return info;
        }
        #endregion

        #region Price
        public ChartInfo Price(SdlChn sc, string title, bool isPut)
        {
            ChartInfo info = new ChartInfo();
            if (sc.Straddles.Count == 0) return info;

            string ticker = sc.Straddles[0].Ticker();
            string maturityStr = sc.Straddles[0].Mstr();
            string modifiedOn = Utility.GMTToEST(sc.CreatedOn).ToString(Constants.CHART_DATE_FORMAT);
            info.Title = $"{ticker} {title} maturity={maturityStr} created={modifiedOn} https://{Constants.DOMAIN}";

            info.ChartType = "line";
            info.Enable3D = false;
            info.Interval = Convert.ToInt32(SetChartInterval(sc.Straddles[0].Strike(), sc.Straddles[sc.Straddles.Count - 1].Strike()));

            info.VAxisTitle = "Price";
            info.VAxisFormat = "#,##0.#####";

            info.HAxisTitle = "Strike";
            info.HAxisFormat = "#,##0";

            ChartSeries series1 = new ChartSeries();
            series1.Title = "Ask";
            series1.Color = "#6699cc";

            ChartSeries series2 = new ChartSeries();
            series2.Title = "Intrinsic";
            series2.Color = "#ffe000";

            ChartSeries series3 = new ChartSeries();
            series3.Title = "Time";
            series3.Color = "#ff0000";

            foreach (Sdl straddle in sc.Straddles)
            {
                string value = GetLinePoint(sc, straddle, "ask", false);
                ChartPoint point = new ChartPoint(straddle.Strike().ToString(), value);
                series1.Points.Add(point);

                value = GetLinePoint(sc, straddle, "intrinsic", false);
                point = new ChartPoint(straddle.Strike().ToString(), value);
                series2.Points.Add(point);

                value = GetLinePoint(sc, straddle, "time", false);
                point = new ChartPoint(straddle.Strike().ToString(), value);
                series3.Points.Add(point);
            }
            info.Series.Add(series1);
            info.Series.Add(series2);
            info.Series.Add(series3);

            return info;
        }
        #endregion

        #region IV
        public ChartInfo IVPredict(SdlChn sc, string title, string key, int degree)
        {
            ChartInfo info = new ChartInfo();
            if (sc.Straddles.Count == 0) return info;

            string ticker = sc.Straddles[0].Ticker();
            string maturityStr = sc.Straddles[0].Mstr();
            string modifiedOn = Utility.GMTToEST(sc.CreatedOn).ToString(Constants.CHART_DATE_FORMAT);
            info.Title = $"{ticker} {title} maturity={maturityStr} created={modifiedOn} https://{Constants.DOMAIN}";

            info.ChartType = "line";
            info.Enable3D = false;
            info.Interval = Convert.ToInt32(SetChartInterval(sc.Straddles[0].Strike(), sc.Straddles[sc.Straddles.Count - 1].Strike()));

            info.VAxisTitle = key;
            info.VAxisFormat = "#,##0.#####";

            info.HAxisTitle = "Strike";
            info.HAxisFormat = "#,##0";

            ChartSeries seriesC1 = new ChartSeries();
            seriesC1.Title = "Call+";
            seriesC1.Color = "#009900";

            ChartSeries seriesC2 = new ChartSeries();
            seriesC2.Title = "Call-";
            seriesC2.Color = "#99cc99";

            ChartSeries seriesP1 = new ChartSeries();
            seriesP1.Title = "Put+";
            seriesP1.Color = "#ff0000";

            ChartSeries seriesP2 = new ChartSeries();
            seriesP2.Title = "Put-";
            seriesP2.Color = "#ff9999";

            foreach (Sdl straddle in sc.Straddles)
            {
                string value = GetLinePoint(sc, straddle, "1sd", false);
                decimal range = Convert.ToDecimal(value) * Convert.ToDecimal(degree);

                decimal high = sc.StockPrice + range;
                ChartPoint point = new ChartPoint(straddle.Strike().ToString(), high.ToString());
                seriesC1.Points.Add(point);

                decimal low = sc.StockPrice - range;
                point = new ChartPoint(straddle.Strike().ToString(), low.ToString());
                seriesC2.Points.Add(point);

                value = GetLinePoint(sc, straddle, "1sd", true);
                range = Convert.ToDecimal(value) * Convert.ToDecimal(degree);

                high = sc.StockPrice + range;
                point = new ChartPoint(straddle.Strike().ToString(), high.ToString());
                seriesP1.Points.Add(point);

                low = sc.StockPrice - range;
                point = new ChartPoint(straddle.Strike().ToString(), low.ToString());
                seriesP2.Points.Add(point);
            }
            info.Series.Add(seriesC1);
            info.Series.Add(seriesP1);
            info.Series.Add(seriesC2);
            info.Series.Add(seriesP2);

            return info;
        }
        #endregion

        #region Generic Line
        public ChartInfo LineDouble(SdlChn sc, string title, string key, int? zoomIn)
        {
            ChartInfo info = new ChartInfo();
            if (sc.Straddles.Count == 0) return info;
            if (zoomIn.HasValue) sc = FilterStraddle(sc, true, zoomIn.Value);

            string ticker = sc.Straddles[0].Ticker();
            string maturityStr = sc.Straddles[0].Mstr();
            string modifiedOn = Utility.GMTToEST(sc.CreatedOn).ToString(Constants.CHART_DATE_FORMAT);
            info.Title = $"{ticker} {title} maturity={maturityStr} created={modifiedOn} https://{Constants.DOMAIN}";

            info.ChartType = "line";
            info.Enable3D = false;
            info.Interval = Convert.ToInt32(SetChartInterval(sc.Straddles[0].Strike(), sc.Straddles[sc.Straddles.Count - 1].Strike()));

            info.VAxisTitle = key;
            info.VAxisFormat = "#,##0.#####";

            info.HAxisTitle = "Strike";
            info.HAxisFormat = "#,##0";

            ChartSeries series = new ChartSeries();
            series.Title = "Call";
            series.Color = "#009900";
            foreach (Sdl straddle in sc.Straddles)
            {
                string value = GetLinePoint(sc, straddle, key, false);
                ChartPoint point = new ChartPoint(straddle.Strike().ToString(), value);
                series.Points.Add(point);
            }
            info.Series.Add(series);

            series = new ChartSeries();
            series.Title = "Put";
            series.Color = "#ff0000";
            foreach (Sdl straddle in sc.Straddles)
            {
                string value = GetLinePoint(sc, straddle, key, true);
                ChartPoint point = new ChartPoint(straddle.Strike().ToString(), value);
                series.Points.Add(point);
            }
            info.Series.Add(series);

            return info;
        }

        public ChartInfo LineSingle(SdlChn sc, string title, string key)
        {
            ChartInfo info = new ChartInfo();
            if (sc.Straddles.Count == 0) return info;

            string ticker = sc.Straddles[0].Ticker();
            string maturityStr = sc.Straddles[0].Mstr();
            string modifiedOn = Utility.GMTToEST(sc.CreatedOn).ToString(Constants.CHART_DATE_FORMAT);
            info.Title = $"{ticker} {title} maturity={maturityStr} created={modifiedOn} https://{Constants.DOMAIN}";

            info.ChartType = "line";
            info.Enable3D = false;
            info.Interval = Convert.ToInt32(SetChartInterval(sc.Straddles[0].Strike(), sc.Straddles[sc.Straddles.Count - 1].Strike()));

            info.VAxisTitle = key;
            info.VAxisFormat = "#,##0.#####";

            info.HAxisTitle = "Strike";
            info.HAxisFormat = "#,##0";

            ChartSeries series = new ChartSeries();
            series.Title = "Values";
            series.Color = "#336699";
            foreach (Sdl straddle in sc.Straddles)
            {
                string value = GetLinePoint(sc, straddle, key, false);
                ChartPoint point = new ChartPoint(straddle.Strike().ToString(), value);
                series.Points.Add(point);
            }
            info.Series.Add(series);

            return info;
        }

        private string GetLinePoint(SdlChn chain, Sdl straddle, string key, bool isPut)
        {
            string result = string.Empty;
            switch (key.ToLower())
            {
                case "open interest": result = isPut ? straddle.poi.ToString() : straddle.coi.ToString(); break;
                case "volume": result = isPut ? straddle.pv.ToString() : straddle.cv.ToString(); break;
                case "implied volatility": result = isPut ? straddle.piv.ToString() : straddle.civ.ToString(); break;
                case "delta": result = isPut ? straddle.pde.ToString() : straddle.cde.ToString(); break;
                case "gamma": result = isPut ? straddle.pga.ToString() : straddle.cga.ToString(); break;
                case "theta": result = isPut ? straddle.pth.ToString() : straddle.cth.ToString(); break;
                case "vega": result = isPut ? straddle.pve.ToString() : straddle.cve.ToString(); break;
                case "rho": result = isPut ? straddle.prh.ToString() : straddle.crh.ToString(); break;

                case "ask": result = isPut ? straddle.pa.ToString() : straddle.ca.ToString(); break;
                case "bid": result = isPut ? straddle.pb.ToString() : straddle.cb.ToString(); break;
                case "price": result = isPut ? straddle.plp.ToString() : straddle.clp.ToString(); break;
                case "intrinsic": result = straddle.IntrinsicValue(chain.StockPrice, isPut).ToString(); break;
                case "time": result = straddle.TimeValue(chain.StockPrice, isPut).ToString(); break;

                case "ga0": result = (straddle.cga - straddle.pga).ToString(); break;
                case "1sd": result = straddle.oneStdDev(chain.StockPrice, isPut).ToString(); break;
            }
            return result;
        }
        #endregion

        #region History Line
        public ChartInfo HistoryLineDouble(SdlChn sc, string title, string key)
        {
            ChartInfo info = new ChartInfo();
            string ticker = sc.Straddles[0].Ticker();
            string maturityStr = sc.Straddles[0].Mstr();
            info.Title = $"{ticker} {title} maturity={maturityStr} strike={sc.Straddles[0].Strike()} https://{Constants.DOMAIN}";

            info.ChartType = "line";
            info.DataType = "date";
            info.Enable3D = false;
            info.Interval = 1;

            info.VAxisTitle = key;
            info.VAxisFormat = "#,##0";

            info.HAxisTitle = "Date";
            info.HAxisFormat = ""; // "MM/dd/yy";

            ChartSeries series = new ChartSeries();
            series.Title = "Call";
            series.Color = "#009900";
            foreach (Sdl straddle in sc.Straddles)
            {
                string value = GetHistoryLinePoint(sc, straddle, key, false);
                ChartPoint point = new ChartPoint(straddle.d, value);
                series.Points.Add(point);
            }
            info.Series.Add(series);

            series = new ChartSeries();
            series.Title = "Put";
            series.Color = "#ff0000";
            foreach (Sdl straddle in sc.Straddles)
            {
                string value = GetHistoryLinePoint(sc, straddle, key, true);
                ChartPoint point = new ChartPoint(straddle.d, value);
                series.Points.Add(point);
            }
            info.Series.Add(series);

            return info;
        }

        private string GetHistoryLinePoint(SdlChn chain, Sdl straddle, string key, bool isPut)
        {
            string result = string.Empty;
            switch (key.ToLower())
            {
                case "open interest": result = isPut ? straddle.poi.ToString() : straddle.coi.ToString(); break;
                case "volume": result = isPut ? straddle.pv.ToString() : straddle.cv.ToString(); break;
                case "price": result = isPut ? straddle.plp.ToString() : straddle.clp.ToString(); break;
                case "implied volatility": result = isPut ? straddle.piv.ToString() : straddle.civ.ToString(); break;
                case "delta": result = isPut ? straddle.pde.ToString() : straddle.cde.ToString(); break;
                case "gamma": result = isPut ? straddle.pga.ToString() : straddle.cga.ToString(); break;
                case "theta": result = isPut ? straddle.pth.ToString() : straddle.cth.ToString(); break;
                case "vega": result = isPut ? straddle.pve.ToString() : straddle.cve.ToString(); break;
                case "rho": result = isPut ? straddle.prh.ToString() : straddle.crh.ToString(); break;

                case "ga0": result = (straddle.cga - straddle.pga).ToString(); break; //straddle.ga0().ToString(); break;
                case "1sd": result = straddle.oneStdDev(chain.StockPrice, isPut).ToString(); break;
            }
            return result;
        }

        public ChartInfo HistoryMaxPain(List<MaxPainHistory> quotes, bool showStockPrice)
        {
            ChartInfo info = new ChartInfo();
            string ticker = quotes[0].TK;
            string maturityStr = quotes[0].M;
            info.Title = string.Format("{0} Max Pain History maturity={1} https://{2}", ticker, maturityStr, Constants.DOMAIN);

            info.ChartType = "line";
            info.DataType = "date";
            info.Enable3D = false;
            info.Interval = 1;

            info.VAxisTitle = "Price";
            info.VAxisFormat = "#,##0";

            info.HAxisTitle = "Date";
            info.HAxisFormat = "";

            ChartSeries? series = null;
            if (showStockPrice)
            {
                series = new ChartSeries();
                series.Title = "Stock";
                series.Color = "#000000";
                foreach (MaxPainHistory quote in quotes)
                {
                    ChartPoint point = new ChartPoint(quote.D.ToString(), quote.SP.ToString());
                    series.Points.Add(point);
                }
                info.Series.Add(series);
            }

            series = new ChartSeries();
            series.Title = "Max Pain";
            series.Color = "#336699";
            foreach (MaxPainHistory quote in quotes)
            {
                ChartPoint point = new ChartPoint(quote.D.ToString(), quote.MP.ToString());
                series.Points.Add(point);
            }
            info.Series.Add(series);

            series = new ChartSeries();
            series.Title = "High Call";
            series.Color = "#009900";
            foreach (MaxPainHistory quote in quotes)
            {
                ChartPoint point = new ChartPoint(quote.D.ToString(), quote.COI.ToString());
                series.Points.Add(point);
            }
            info.Series.Add(series);

            series = new ChartSeries();
            series.Title = "High Put";
            series.Color = "#ff0000";
            foreach (MaxPainHistory quote in quotes)
            {
                ChartPoint point = new ChartPoint(quote.D.ToString(), quote.POI.ToString());
                series.Points.Add(point);
            }
            info.Series.Add(series);

            return info;
        }
        #endregion

        #region Other Charts
        public ChartInfo HistoricalVolatility(List<HistoricalVolatility> vols)
        {
            ChartInfo info = new ChartInfo();
            if (vols.Count != 0)
            {
                string ticker = vols[0].Ticker;
                info.Title = string.Format("{0} Historical Volatility https://{1}", ticker, Constants.DOMAIN);

                info.ChartType = "line";
                info.DataType = "date";
                info.Enable3D = false;
                info.Interval = 30;

                info.VAxisTitle = "Historical Volatility";
                info.VAxisFormat = "#,##0.#####";

                info.HAxisTitle = "Date";
                info.HAxisFormat = "";

                for (int i = 1; i <= 5; i++)
                {
                    ChartSeries series = new ChartSeries();
                    series.Title = "Day 20";
                    series.Color = "#ff0000";
                    if (i == 2) { series.Title = "Day 40"; series.Color = "#ff6600"; }
                    if (i == 3) { series.Title = "Day 60"; series.Color = "#ff9900"; }
                    if (i == 4) { series.Title = "Day 120"; series.Color = "#ffcc00"; }
                    if (i == 5) { series.Title = "Day 240"; series.Color = "#ffff00"; }

                    foreach (HistoricalVolatility vol in vols)
                    {
                        ChartPoint point = new ChartPoint(vol.Date.ToString("MM/dd/yy"), vol.Day20.ToString());
                        series.Points.Add(point);
                    }
                    info.Series.Add(series);
                }
            }
            return info;
        }

        public ChartInfo HighOpenInterest(List<HighOpenInterest> highs)
        {
            ChartInfo info = new ChartInfo();
            string maturity = highs[0].Maturity;
            info.Title = string.Format("High Open Interest maturity={0} https://{1}", maturity, Constants.DOMAIN);

            info.ChartType = "line";
            info.DataType = "date";
            info.Enable3D = false;
            info.Interval = 1;

            info.VAxisTitle = "Open Interest";
            info.VAxisFormat = "#,##0";

            info.HAxisTitle = "Date";
            info.HAxisFormat = "";

            int counter = 0;
            List<string> tickers = highs.Select(x => x.Ticker).Distinct().ToList();
            foreach (string ticker in tickers)
            {
                counter++;

                ChartSeries series = new ChartSeries();
                series.Title = ticker;
                series.Color = "#e0e0e0";
                if (counter == 1) { series.Color = "#ff0000"; }
                if (counter == 2) { series.Color = "#ffcc00"; }
                if (counter == 3) { series.Color = "#ffff00"; }
                if (counter == 4) { series.Color = "#009900"; }
                if (counter == 5) { series.Color = "#336699"; }
                if (counter == 6) { series.Color = "#ff00ff"; }

                List<HighOpenInterest> filtered = highs.FindAll(x => x.Ticker == ticker);
                foreach (HighOpenInterest high in filtered)
                {
                    ChartPoint point = new ChartPoint(high.CreatedOn, high.OpenInterest.ToString());
                    series.Points.Add(point);
                }
                info.Series.Add(series);
            }

            return info;
        }
        #endregion

        #region Stacked
        public ChartInfo Stacked(SdlChn sc, string title, string key, bool isPut)
        {
            ChartInfo info = new ChartInfo();
            if (sc.Straddles.Count == 0) return info;

            string ticker = sc.Straddles[0].Ticker();
            string modifiedOn = Utility.GMTToEST(sc.CreatedOn).ToString(Constants.CHART_DATE_FORMAT);
            info.Title = $"{ticker} {title} created={modifiedOn} https://{Constants.DOMAIN}";

            info.ChartType = "stackedcolumn";
            info.Enable3D = false;
            info.Interval = Convert.ToInt32(SetChartInterval(sc.Straddles[0].Strike(), sc.Straddles[sc.Straddles.Count - 1].Strike()));

            info.VAxisTitle = key;
            info.VAxisFormat = "#,##0.#####";

            info.HAxisTitle = "Strike";
            info.HAxisFormat = "#,##0";

            List<string> strikes = new List<string>();
            foreach (Sdl straddle in sc.Straddles)
            {
                string type = isPut ? "Put" : "Call";
                string seriesKey = straddle.Maturity().ToString("MM/dd/yyyy");

                // keep track of all strikes which is X axis
                string strike = straddle.Strike().ToString();
                if (!strikes.Contains(strike)) strikes.Add(strike);

                // find the series
                ChartSeries? series = info.Series.FirstOrDefault(s => s.Title != null && s.Title.Equals(seriesKey));
                if (series == null)
                {
                    series = new ChartSeries();
                    series.Title = seriesKey;
                    info.Series.Add(series);
                }

                string value = GetLinePoint(sc, straddle, key, isPut);
                ChartPoint point = new ChartPoint(strike, value);
                series.Points.Add(point);
            }

            // make sure each series has all the strikes
            foreach (ChartSeries series in info.Series)
            {
                foreach (string strike in strikes)
                {
                    ChartPoint? point = series.Points.FirstOrDefault(p => p.X.Equals(strike));
                    if (point == null)
                    {
                        point = new ChartPoint(strike, "0");
                        series.Points.Add(point);
                    }
                }
            }

            return info;
        }

        #endregion


        #region helper
        private double SetChartInterval(decimal min, decimal max)
        {
            decimal range = max - min;
            if (range <= 5) return 0.25;
            if (range <= 10) return 0.5;
            if (range <= 25) return 1;
            if (range <= 50) return 2;
            if (range <= 100) return 5;
            if (range <= 200) return 10;
            if (range <= 400) return 20;
            return 30;

            /*
            double fifty = (Convert.ToDouble(max - min)) / 50;
            if (fifty <= 2) return 1;
            if (fifty <= 7) return 5;
            if (fifty <= 13) return 10;
            if (fifty <= 23) return 20;
            if (fifty <= 53) return 50;
            if (fifty <= 103) return 100;
            return 200;
            */
        }
        #endregion

        #region Images
        public async Task<byte[]> FetchImage(string route, string ticker, DateTime maturity)
        {
            string chartDomain = await _secretService.GetValue("ConstChartDomain");
            string url = $"http://{chartDomain}/image/{route}/{ticker}";
            if (maturity != DateTime.MinValue)
            {
                url = $"http://{chartDomain}/image/{route}/{ticker}?m={maturity}";
            }

            HttpClient client = new HttpClient();
            byte[] buffer = await client.GetByteArrayAsync(url);
            return buffer;
        }

        public async Task<byte[]> FetchImage(ChartInfo info)
        {
            string chartDomain = await _secretService.GetValue("ConstChartDomain");
            string url = "http://{0}/Image/PostJson";
            url = string.Format(url, chartDomain);

            string json = DBHelper.Serialize(info);
            string payload = string.Format("json={0}", HttpUtility.UrlEncode(json));

            return await PostURI(url, payload);
        }

        public async Task<byte[]> PostURI(string url, string payload)
        {
            // Use HttpClientFactory for better performance and resource management
            using (var client = new HttpClient())
            {
                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        private byte[] ReadFully(Stream input)
        {
            // Use MemoryStream for better performance
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        #endregion
    }
}

