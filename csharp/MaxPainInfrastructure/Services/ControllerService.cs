using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Xml;
using Twilio.TwiML.Voice;

namespace MaxPainInfrastructure.Services
{
    public class ControllerService : IControllerService
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ILoggerService _logger;
        private readonly ICalculationService _calculation;
        private readonly IChartService _chart;
        private readonly IConfigurationService _configuration;
        private readonly IEmailService _email;
        private readonly IFinDataService _finData;
        private readonly IFinImportService _finImport;
        private readonly IHistoryService _history;
        private readonly ISecretService _secret;
        private readonly ISMSService _sms;


        public ControllerService(
            AwsContext awsContext,
            HomeContext homeContext,
            ILoggerService loggerService,
            ICalculationService calculationService,
            IChartService chartService,
            IConfigurationService configurationService,
            IEmailService emailService,
            IFinDataService finDataService,
            IFinImportService finImportService,
            IHistoryService historyService,
            ISecretService secretService,
            ISMSService smsService
            )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _logger = loggerService;
            _calculation = calculationService;
            _chart = chartService;
            _configuration = configurationService;
            _email = emailService;
            _finData = finDataService;
            _finImport = finImportService;
            _history = historyService;
            _secret = secretService;
            _sms = smsService;
        }

        #region stock
        public async Task<List<Stock>> GetStocks()
        {
            List<Stock> stocks = new List<Stock>();

            List<PythonTicker>? python = await _awsContext.GetPythonTicker();
            string json = DBHelper.Serialize(python);
            List<StockTicker> tickers = DBHelper.Deserialize<List<StockTicker>>(json);

            int step = 50;
            for (int i = 0; i < tickers.Count; i += step)
            {
                string csv = string.Empty;
                int end = i + 50 > tickers.Count ? tickers.Count : i + 50;
                for (int j = i; j < end; j++)
                {
                    if (csv.Length != 0) csv = string.Concat(csv, ",");
                    csv = string.Concat(csv, tickers[j].Ticker);
                }

                List<Stock> subset = await _finData.FetchStock(csv);
                foreach (Stock s in subset)
                {
                    stocks.Add(s);
                }
            }

            return stocks;
        }
        #endregion

        #region Scheduled Task
        public async Task<List<string>> ScheduledTask(bool debug)
        {
            List<string> result = new List<string>();

            string xml = await _awsContext.SettingsRead();
            XmlDocument xmlSettings = new XmlDocument();
            xmlSettings.LoadXml(xml);

            string xpath = "/Settings/UseWindowsTask";
            XmlElement? elm = (XmlElement?)xmlSettings.SelectSingleNode(xpath);
            bool useWindowsTask = Convert.ToBoolean(elm == null ? false : elm.InnerText);


            // twitter
            try
            {
                bool useTwitter = false;
                result.Add($"useTwitter={useTwitter}");

                xpath = "/Settings/UseTwitter";
                elm = (XmlElement?)xmlSettings.SelectSingleNode(xpath);
                if (elm != null) useTwitter = Convert.ToBoolean(elm.InnerText);

                /*
                if (useTwitter)
                {
                    TwitterHelper helper = new TwitterHelper();
                    await helper.InitializeXml();
                    TwitterMessage msg = await helper.Execute();
                    result.Add(DBHelper.Serialize(msg));
                }
                */
            }
            catch (Exception ex)
            {
                await _logger.InfoAsync("ScheduledTask Twitter ERROR", ex.ToString());
                result.Add("SheduledTask Twitter ERROR");
                result.Add(ex.ToString());
            }

            // health check
            try
            {
                bool status = await HealthCheck(xmlSettings, debug);
            }
            catch (Exception ex)
            {
                await _logger.InfoAsync("ScheduledTask HealthCheck ERROR", ex.ToString());
                result.Add("SheduledTask HealthCheck ERROR");
                result.Add(ex.ToString());
            }

            return result;
        }

        public async Task<bool> HealthCheckUnitTest(string xsltFile, XmlDocument xmlSettings, bool debug)
        {
            AwsContext awsContext = new AwsContext();
            HomeContext homeContext = new HomeContext();

            return await HealthCheck(xsltFile, false);
        }

        public async Task<bool> HealthCheck(XmlDocument xmlSettings, bool debug)
        {
            DateTime current = DateTime.UtcNow;
            DateTime lastRun = Convert.ToDateTime(xmlSettings.SelectSingleNode("/Settings/HealthCheckLastRun").InnerText);
            if (!(current > lastRun.AddHours(6)))
            {
                return true;
            }

            await _configuration.Set("HealthCheckLastRun", current.ToString("MM /dd/yy HH:mm:ss"));

            string xslContent = Utility.GetEmbeddedFile("HealthCheck.xsl");

            return await HealthCheck(xslContent, debug);
        }

        public async Task<bool> HealthCheck(string xslContent, bool debug)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<HealthChecks/>");
            bool hasError = false;
            bool errorTest = false;
            string message = string.Empty;

            string ticker = "AAPL";

            // option data
            try
            {
                OptChn chain = await _finData.FetchOptionChain(ticker, DateTime.MinValue);
                errorTest = (chain.Options.Count == 0) ? true : false;
                message = string.Format("{0} quotes.Count={1}", ticker, chain.Options.Count);
            }
            catch (Exception ex)
            {
                errorTest = true;
                message = ex.ToString();
            }
            if (errorTest) hasError = true;

            xmlDoc.DocumentElement.AppendChild(AddItem(xmlDoc, "Maximum-Pain.com Option Data", errorTest, message));

            // home DB
            try
            {
                List<MostActive> actives = await _homeContext.MostActive.ToListAsync();
                errorTest = (actives.Count == 0) ? true : false;
                message = string.Format("Home DB MostActive SP actives.Count={0}", actives.Count);
            }
            catch (Exception ex)
            {
                errorTest = true;
                message = ex.ToString();
            }
            if (errorTest) hasError = true;
            xmlDoc.DocumentElement.AppendChild(AddItem(xmlDoc, "Home DB", errorTest, message));

            // option history
            try
            {
                DateTime expected = ExpectedOptionDate(Utility.CurrentDateEST());

                string sql = "SELECT MAX(CreatedOn) AS CreatedOn, dateadd(dd,0, datediff(dd,0,MAX(CreatedOn)))  AS [Date] FROM HistoricalOptionQuote WITH(NOLOCK)";
                DateTime actual = Convert.ToDateTime(await _homeContext.FetchScalar(sql, null, "Date"));

                errorTest = (actual == expected.Date) ? false : true;
                string exp = expected.ToString("MM/dd/yy");
                string act = actual.ToString("MM/dd/yy");
                message = $"Home DB Option History date. expected={exp} actual={act}";
            }
            catch (Exception ex)
            {
                errorTest = true;
                message = ex.ToString();
            }
            if (errorTest) hasError = true;
            xmlDoc.DocumentElement.AppendChild(AddItem(xmlDoc, "FIN Option History", errorTest, message));

            // twitter
            string result = string.Empty;
            try
            {
                string value = await _configuration.Get("UseTwitter");
                bool useTwitter = value.Length == 0 ? false : Convert.ToBoolean(value);

                /*
                if (useTwitter)
                {
                    TwitterHelper helper = new TwitterHelper();
                    await helper.InitializeXml();
                    message = await helper.HealthCheck();
                    errorTest = result.Length == 0 ? false : true;
                }
                */
            }
            catch (Exception ex)
            {
                errorTest = true;
                message = ex.ToString();
            }
            if (errorTest) hasError = true;
            xmlDoc.DocumentElement.AppendChild(AddItem(xmlDoc, "Twitter", errorTest, message));

            string html = Utility.TransformXml(xmlDoc.OuterXml, xslContent);
            await _logger.InfoAsync("HealthCheck", html);
            if (hasError)
            {
                try
                {
                    var email = await _secret.GetValue("ConstEmail");
                    await _email.SendEmail(email, email, string.Empty, string.Empty, "Maximum-pain.com HEALTH CHECK ERROR", html, string.Empty, true);
                }
                catch (Exception ex)
                {
                    xmlDoc.DocumentElement.AppendChild(AddItem(xmlDoc, "Email failure", true, ex.ToString()));
                }
            }

            return errorTest;
        }

        private XmlElement AddItem(XmlDocument xmlDoc, string name, bool hasError, string description)
        {
            XmlElement xmlElm = xmlDoc.CreateElement("HealthCheck");
            xmlElm.SetAttribute("Name", name);
            xmlElm.SetAttribute("HasError", hasError.ToString());
            xmlElm.SetAttribute("Description", description);
            return xmlElm;
        }

        public DateTime ExpectedOptionDate(DateTime estDate)
        {
            // before 5pm, so go to previous day
            if (estDate.Hour < 17) estDate = estDate.AddDays(-1);  // yesterday
            // weekend, so move back to Friday
            if (estDate.DayOfWeek == DayOfWeek.Sunday) estDate = estDate.AddDays(-2);
            if (estDate.DayOfWeek == DayOfWeek.Saturday) estDate = estDate.AddDays(-1);

            estDate = Convert.ToDateTime(estDate.ToString("MM/dd/yyyy"));

            return estDate.Date;
        }
        #endregion

        public async Task<string> DailyMonitor()
        {
            List<DailyScan> alertList = new List<DailyScan>();

            string sql = @"
                SELECT Id, Ticker, CreatedOn, ADR, (ADR / Price * 100) AS ADRPercent 
                FROM Python..DailyResult WITH(NOLOCK)
                WHERE ISNULL(HasAlerted,0) = 0
                AND WatchFlag  = 1
                AND CreatedOn > (
                    SELECT CONVERT(VARCHAR, MAX(CreatedOn), 101) 
                    FROM Python..DailyResult WITH(NOLOCK)
                )
            ";

            // alert on daily scan
            List<DailyScan> scans = await _awsContext.FetchModel<DailyScan>(sql, null, 30);
            if (scans.Count > 0)
            {
                List<string> tickers = scans.Select(x => x.Ticker).ToList();
                List<Stock> stocks = await _finData.FetchStock(string.Join(",", tickers));

                if (stocks != null)
                {
                    foreach (DailyScan scan in scans)
                    {
                        var stock = stocks.Where(x => x.symbol == scan.Ticker.ToUpper()).FirstOrDefault();
                        if (stock != null)
                        {
                            scan.NetPercentChange = stock.quote.netPercentChange;
                            scan.MarkPercentChange = stock.quote.markPercentChange;

                            if (scan.NetPercentChange > scan.ADRPercent / 2 || scan.NetPercentChange > 4)
                            {
                                alertList.Add(scan);
                            }
                        }
                    }
                }
            }

            // alert on index
            List<string> tickers2 = new List<string>() { "^GSPC", "^DJI", "^IXIC" };
            List<Stock> stocks2 = await _finData.FetchStock(string.Join(",", tickers2));
            foreach(Stock stock in stocks2)
            {
                if (stock.quote.netPercentChange > 2)
                {
                    await _homeContext.DailyScanAdd(stock.symbol);

                    DailyScan scan = new DailyScan()
                    {
                        Ticker = stock.symbol,
                        NetPercentChange = stock.quote.netPercentChange,
                        ADRPercent = 0
                    };
                    alertList.Add(scan);
                }
            }

            if (alertList.Count > 0)
            {
                string content = string.Empty;
                string csv = String.Join(",", alertList.Select(x => x.Ticker).ToArray());
                foreach (DailyScan scan in alertList)
                {
                    //var watch = scan.WatchFlag.Value ? " WATCH" : string.Empty;
                    var watch = string.Empty;
                    content = $"{content} {scan.Ticker} {Math.Round(scan.NetPercentChange, 3)}{watch}\r\n";
                }

                await _logger.InfoAsync("ControllerService DailyMonitor send alert", content);

                sql = @"
                    UPDATE Python..DailyResult SET HasAlerted = 1
                    FROM Python..DailyResult WITH(NOLOCK)
                    WHERE Ticker IN (SELECT Item FROM Python.dbo.DelimitedSplit8K(@TickerCSV, ','))
                    AND CreatedOn > (
                        SELECT CONVERT(VARCHAR, MAX(CreatedOn), 101) 
                        FROM Python..DailyResult WITH(NOLOCK)
                    )
                ";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("TickerCSV", csv));
                await _awsContext.Execute(sql, parameters, 30);

                await _sms.SendTelegram(content);
            }

            return DBHelper.Serialize(alertList);
        }

        public async Task<List<Daily>> Daily(DateTime start, DateTime end, string? source, string? tickers)
        {
            var sql = @"
                SELECT 
	                Ticker
	                ,[Source]
	                ,[Date]
	                ,[Open]
	                ,[High]
	                ,[Low]
	                ,[Close]
	                ,[AdjClose]
	                ,[Volume]
	                ,ROUND(AVG([Close]) OVER (
                        ORDER BY [Date]
                        ROWS BETWEEN 9 PRECEDING AND CURRENT ROW
                    ),2) AS SMA10
	                ,ROUND(AVG([Close]) OVER (
                        ORDER BY [Date]
                        ROWS BETWEEN 19 PRECEDING AND CURRENT ROW
                    ),2) AS SMA20
	                ,ROUND(AVG([Volume]) OVER (
                        ORDER BY [Date]
                        ROWS BETWEEN 19 PRECEDING AND CURRENT ROW
                    ),2) AS Volume20
                FROM Python..vwDaily 
                WHERE [Date] BETWEEN @Start AND @End    
            ";


            var parameters = new List<SqlParameter>
            {
                new SqlParameter("Start", start),
                new SqlParameter("End", end)
            };

            if (!string.IsNullOrEmpty(tickers))
            {
                parameters.Add(new SqlParameter("Tickers", tickers));
                sql = string.Concat(sql, " AND Ticker IN (@Tickers) ORDER BY [Date]");
                return await _awsContext.FetchModel<Daily>(sql, parameters, 30);
            }

            if (!string.IsNullOrEmpty(source))
            {
                parameters.Add(new SqlParameter("Source", source));
                sql = string.Concat(sql, " AND Source = @Source ORDER BY [Date]");
                return await _awsContext.FetchModel<Daily>(sql, parameters, 30);
            }

            sql = string.Concat(sql, " ORDER BY [Date]");
            return await _awsContext.FetchModel<Daily>(sql, null, 30);
        }

        public static T[] ConcatArrays<T>(params T[][] p)
        {
            var position = 0;
            var outputArray = new T[p.Sum(a => a.Length)];
            foreach (var curr in p)
            {
                Array.Copy(curr, 0, outputArray, position, curr.Length);
                position += curr.Length;
            }
            return outputArray;
        }
    }
}
