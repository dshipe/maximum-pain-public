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

        #region OptionHistory
        public async Task<OptChn> FetchOptionHistory(string ticker, DateTime maturity, decimal strike, int pastDays, int futureDays)
        {
            // fetch OptionQuote using EF and OptionChainJson
            //await _logger.InfoAsync("ControllerService FetchOptionHistory", "fetch from database");
            return await _history.GetByTicker(ticker, pastDays, futureDays);
        }
        #endregion

        #region MaxPainHistory
        public async Task<List<MaxPainHistory>?> FetchMaxPainHistory(string ticker, DateTime maturity)
        {
            // fetch OptionQuote using EF and OptionChainJson
            //await _logger.InfoAsync("ControllerService FetchMaxPainHistory", "fetch from database");
            List<MaxPainHistory>? all = await _homeContext.MaxPainHistoryRead(ticker, maturity);

            // filter by maturity
            //await _logger.InfoAsync("ControllerService FetchMaxPainHistory", "filter by maturity and strike");
            if (maturity == DateTime.MinValue)
            {
                return all;
            }

            //return all.Where(x => x.Maturity.ToString("yyMMdd") == maturity.ToString("yyMMdd")).ToList();
            string maturityStr = maturity.ToString("MM/dd/yyyy");
            return all.Where(x => x.M == maturityStr).ToList();
        }
        #endregion

        #region stock
        public async Task<List<StockTicker>> GetStockTickers()
        {
            List<PythonTicker>? python = await _awsContext.GetPythonTicker();
            string json = DBHelper.Serialize(python);
            return DBHelper.Deserialize<List<StockTicker>>(json);
        }

        public async Task<List<Stock>> GetStocks()
        {
            List<Stock> stocks = new List<Stock>();
            List<StockTicker> tickers = await GetStockTickers();

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
        public async Task<string> GetScreenerImageTicker()
        {
            return await _configuration.Get("ScreenerImageTicker");
        }

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

        public async Task<string> ExecuteImport(XmlDocument xmlSettings, bool debug, string key, int militaryHour)
        {
            DateTime utc = DateTime.UtcNow;
            DateTime est = Utility.GMTToEST(utc);

            if (est.DayOfWeek == DayOfWeek.Sunday || est.DayOfWeek == DayOfWeek.Saturday)
            {
                string response = $"{key} NOT RUN.  The day of the week is Saturday or Sunday";
                return response;
            }

            int currentTime = Convert.ToInt32(est.ToString("HHmm"));
            int oneHour = militaryHour + 100;
            if (oneHour > 2400) oneHour = oneHour - 2400;
            if (militaryHour != 0)
            {
                if (!(currentTime >= militaryHour && currentTime <= oneHour))
                {
                    string response = $"{key} NOT RUN.  The time must be between {militaryHour} and {oneHour} EST";
                    return response;
                }
            }

            DateTime lastRun = Convert.ToDateTime(xmlSettings.SelectSingleNode($"/Settings/{key}").InnerText);

            int currentDay = Convert.ToInt32(utc.ToString("yyyyMMdd"));
            int lastRunDay = Convert.ToInt32(lastRun.ToString("yyyyMMdd"));
            if (lastRunDay >= currentDay)
            {
                string response = $"{key} NOT RUN.  The last ran date was today";
                return response;
            }

            await _configuration.Set(key, utc.ToString("MM/dd/yy HH:mm:ss"));

            await _logger.InfoAsync("ExecuteImport", $"key={key} militaryHour={militaryHour} oneHour={oneHour} est={est}");
            _finImport.IsDebug = false;
            _finImport.UseMessage = true;
            _finImport.UseMostActiveCode = false;
            await _finImport.RunImport();

            return string.Empty;
        }

        public async Task<string> ScreenerDistribute(bool debug, bool useShortUrls, bool runNow)
        {
            string xml = await _awsContext.SettingsRead();
            XmlDocument xmlSettings = new XmlDocument();
            xmlSettings.LoadXml(xml);

            DateTime current = DateTime.UtcNow;
            int militaryHour = Convert.ToInt32(Utility.GMTToEST(current).ToString("HHmm"));
            if (Convert.ToBoolean(runNow)) militaryHour = -1;

            string imageTicker = await GetScreenerImageTicker();
            string html = await ExecuteScreener(xmlSettings, imageTicker, Convert.ToBoolean(debug), Convert.ToBoolean(useShortUrls), string.Empty, militaryHour);
            await _awsContext.SettingsPost(xmlSettings.OuterXml);

            return html;
        }

        public async Task<string> ExecuteScreener(XmlDocument xmlSettings, string imageTicker, bool debug, bool useShortUrls, string message, int militaryHour)
        {
            DateTime current = DateTime.UtcNow;
            if (militaryHour != -1)
            {
                if (current.DayOfWeek == DayOfWeek.Sunday || current.DayOfWeek == DayOfWeek.Saturday)
                {
                    string response = "NOT RUN.  The day of the week is Saturday or Sunday";
                    response = string.Concat(response, "<br/>debug: {0}<br/>useShortUrls: {1}<br/>message: {2}<br/>");
                    response = string.Concat(response, "current date: {3}<br/>day of week: {4}");
                    response = string.Format(response, debug, useShortUrls, message, current, current.DayOfWeek);
                    return response;
                }

                if (militaryHour != 0)
                {
                    int currentTime = Convert.ToInt32(Utility.GMTToEST(current).ToString("HHmm"));
                    if (!(currentTime >= militaryHour && currentTime <= militaryHour + 100))
                    {
                        string response = $"NOT RUN.  The time must be between {militaryHour} and {militaryHour + 100} EST";
                        response = string.Concat(response, "<br/>debug: {0}<br/>useShortUrls: {1}<br/>message: {2}<br/>");
                        response = string.Concat(response, "current date: {3}<br/>current time: {4}");
                        response = string.Format(response, debug, useShortUrls, message, current, currentTime);
                        return response;
                    }
                }

                DateTime lastRun = Convert.ToDateTime(xmlSettings.SelectSingleNode("/Settings/ScreenerLastRun").InnerText);

                int currentDay = Convert.ToInt32(current.ToString("yyyyMMdd"));
                int lastRunDay = Convert.ToInt32(lastRun.ToString("yyyyMMdd"));
                if (lastRunDay >= currentDay)
                {
                    string response = "NOT RUN.  The last ran date was today";
                    response = string.Concat(response, "<br/>debug: {0}<br/>useShortUrls: {1}<br/>message: {2}<br/>");
                    response = string.Concat(response, "current date: {3}<br/>current string: {4}");
                    response = string.Concat(response, "lastRun date: {5}<br/>lastRun string: {6}");
                    response = string.Format(response, debug, useShortUrls, message, current, currentDay, lastRun, lastRunDay);
                    return response;
                }
                await _configuration.Set("ScreenerLastRun", current.ToString("MM /dd/yy HH:mm:ss"));
            }

            List<MostActive> actives = await _homeContext.MostActive.ToListAsync();
            actives.ForEach(a => a.QueryType = a.GetQueryType());
            List<OutsideOIWalls> walls = await _homeContext.OutsideOIWalls.ToListAsync();
            //List<ImportMaxPain> pains = await homeContext.ImportMaxPainRecentRead();
            List<Mx> pains = new List<Mx>();

            byte[] buffer = await GetEmailImage(imageTicker);

            string xslContent = Utility.GetEmbeddedFile("screener.xsl");
            string html = await _email.Screener(actives, walls, pains, xslContent, imageTicker, buffer, debug, useShortUrls, message);

            await _logger.InfoAsync("ScheduledTask Screener", html);
            return html;
        }

        public async Task<byte[]> GetEmailImage(string imageTicker)
        {
            OptChn chain = await _finData.FetchOptions(imageTicker);
            List<Opt> test = chain.Options.Where(x => x.oi > 0).ToList();
            if (test.Count < 10)
            {
                // get chain from history
                HistoryDate hd = await _history.GetHistoryDate();
                List<OptChn> chains = await _history.ChainGetByDateAndTicker(hd.CurrentDate, imageTicker);
                chain = chains.First();
                test = chain.Options.Where(x => x.oi > 0).ToList();
                if (test.Count < 10)
                {
                    chains = await _history.ChainGetByDateAndTicker(hd.PreviousDate, imageTicker);
                    chain = chains.First();
                }
            }
            test = chain.Options.Where(x => x.oi > 0).ToList();
            chain = _calculation.FilterOptionChainFutureOnly(chain, true);
            test = chain.Options.Where(x => x.oi > 0).ToList();

            SdlChn sc = _calculation.BuildStraddle(chain);
            ChartInfo info = _chart.LineDouble(sc, "Open Interest", "Open Interest", 200);
            return await _chart.FetchImage(info);
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

        #region EmailList
        public string EmaiListUnsubscribeHTML(string message)
        {
            string html = Utility.GetEmbeddedFile("Unsubscribe.html");
            html = html.Replace("<h3></h3>", $"<h3>{message}</h3>");
            return html;
        }

        private async Task<bool> EmailListUpdate_Orig(string name, string email, EmailStatus status)
        {
            //EmailAccount account = _awsContext.EmailAccount.SingleOrDefault(x => string.Equals(x.Email.ToUpper(), email.ToUpper())

            List<EmailAccount> accounts = await _awsContext.EmailAccount
                    .Where(x => string.Equals(x.Email.ToUpper(), email.ToUpper()))
                    .ToListAsync();

            EmailAccount? account = null;
            if (accounts.Count != 0)
            {
                long id = accounts[0].Id;

                account = await _awsContext.EmailAccount.FindAsync(id);
                if (account != null)
                {
                    account.EmailStatusID = (System.Int32)status;
                    account.ModifiedOn = DateTime.UtcNow;

                    _awsContext.Entry(account).State = EntityState.Modified;
                    await _awsContext.SaveChangesAsync();
                }
                return true;
            }

            account = new EmailAccount();
            _awsContext.EmailAccount.Add(account);

            account.Email = email;
            if (!string.IsNullOrEmpty(name)) account.Name = name;
            account.EmailStatusID = (System.Int32)status;
            account.CreatedOn = DateTime.UtcNow;
            account.ModifiedOn = DateTime.UtcNow;

            _awsContext.Entry(account).State = EntityState.Added;
            await _awsContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EmailListUpdate(string name, string email, EmailStatus status)
        {
            string sql = @"
                IF NOT EXISTS (
                    SELECT Id
                    FROM EmailAccount
                    WHERE Email = @Email
                )
                BEGIN
                    UPDATE EmailAccount SET EmailStatusID = @StatusId, ModifiedOn = GetUTCDate()
                    WHERE Email = @Email
                END
                ELSE
                BEGIN
                    INSERT INTO EmailAccount (Name, Email, EmailStatusId, CreatedOn)
                    VALUES (@Name, @Email, @StatusId, GetUTCDate())
                END
            ";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("StatusId", (System.Int32)status));
            parameters.Add(new SqlParameter("Email", email));
            parameters.Add(new SqlParameter("Name", name));

            await _awsContext.Execute(sql, parameters, 30);
            return true;
        }
        #endregion

        #region Zip
        /*
        private void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        private byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        private string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        */
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

                await _sms.SendTextMessage(content);
            }

            return DBHelper.Serialize(dailies);
        }

        public async Task<bool> OauthExpirationCheck()
        {
            var createdOn = await _finData.GetCreatedOn();
            var expiresOn = createdOn.AddDays(7);
            var warnOn = createdOn.AddHours(7 * 24 - 6);

            if (DateTime.UtcNow >= warnOn)
            {
                //await SendTextMessageAWS($"token expires {expiresOn}");
            }

            return false;
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
