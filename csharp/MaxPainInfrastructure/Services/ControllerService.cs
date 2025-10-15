using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Xml;

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
                await _logger.InfoAsync("SheduledTask Twitter ERROR", ex.ToString());
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
                await _logger.InfoAsync("SheduledTask HealthCheck ERROR", ex.ToString());
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

                string sql = "SELECT [Date] FROM vwHistoryRecent WITH(NOLOCK)";
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
            //USE Python
            //GO
            //ALTER TABLE DailyResult ADD HasAlerted BIT

            string sql = @"
                SELECT Id, Ticker, CreatedOn
                FROM Python..DailyResult WITH(NOLOCK)
                WHERE ISNULL(HasAlerted,0) = 0
                AND WatchFlag = 1
                AND CreatedOn > (
                    SELECT CONVERT(VARCHAR, MAX(CreatedOn), 101) 
                    FROM Python..DailyResult WITH(NOLOCK)
                )
            ";

            string json = await _awsContext.FetchJson(sql, null, 30);
            JArray jArray = JArray.Parse(json);

            List<string> tickers = new List<string>();
            if (jArray.Count > 0)
            {
                foreach (JObject item in jArray)
                {
                    tickers.Add(item.GetValue("Ticker").ToString());
                }
            }

            List<DailyStock> dailies = new List<DailyStock>();
            if (tickers.Count > 0)
            {
                List<Stock> quotes = await _finData.FetchStock(string.Join(",", tickers));
                foreach (Stock stk in quotes)
                {
                    json = DBHelper.Serialize(stk.quote);
                    DailyStock ds = DBHelper.Deserialize<DailyStock>(json);
                    ds.Ticker = stk.symbol;

                    if (ds.NetPercentChange > 2)
                    {
                        dailies.Add(ds);
                    }
                }
            }

            if (dailies.Count > 0)
            {
                string content = string.Empty;
                string csv = String.Join(",", dailies.Select(x => x.Ticker).ToArray());
                foreach (DailyStock ds in dailies)
                {
                    content = $"{content} {ds.Ticker} {ds.NetPercentChange}\r\n";
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

                await _sms.SendWhatsapp(content);
            }

            return DBHelper.Serialize(dailies);
        }
    }


    public class DailyStock
    {
        public string Ticker { get; set; }
        //public Decimal AskPrice { get; set; }
        //public Decimal BidPrice { get; set; }
        public Decimal OpenPrice { get; set; }
        public Decimal LastPrice { get; set; }
        public Decimal MarkPercentChange { get; set; }
        public Decimal NetPercentChange { get; set; }
        public Int64 TotalVolume { get; set; }
    }
}
