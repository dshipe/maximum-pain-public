using Amazon.Lambda.APIGatewayEvents;
using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Models.Schwab;
using MaxPainInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Web;
using System.Xml;

namespace MaxPainLambda
{
    public class LambdaService : ILambdaService
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ILoggerService _loggerSvc;
        private readonly ICalculationService _calculationSvc;
        private readonly IChartService _chartSvc;
        private readonly IConfigurationService _configurationSvc;
        private readonly IControllerService _controllerSvc;
        private readonly IEmailService _emailSvc;
        private readonly IFinDataService _finDataSvc;
        private readonly IFinImportService _finImportSvc;
        private readonly IHistoryService _historySvc;
        private readonly ISchwabService _schwabSvc;
        private readonly ISecretService _secretSvc;
        private readonly ISMSService _smsSvc;

        public LambdaService(
            AwsContext awsContext,
            HomeContext homeContext,
            ILoggerService loggerService,
            ICalculationService calculationService,
            IChartService chartService,
            IConfigurationService configurationService,
            IControllerService controllerService,
            IEmailService emailService,
            IFinDataService finDataService,
            IFinImportService finImportService,
            IHistoryService historyService,
            ISchwabService schwabService,
            ISecretService secretService,
            ISMSService smsService
        )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _loggerSvc = loggerService;
            _calculationSvc = calculationService;
            _chartSvc = chartService;
            _configurationSvc = configurationService;
            _controllerSvc = controllerService;
            _emailSvc = emailService;
            _finDataSvc = finDataService;
            _finImportSvc = finImportService;
            _historySvc = historyService;
            _schwabSvc = schwabService;
            _secretSvc = secretService;
            _smsSvc = smsService;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> HandleRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            try
            {
                string? path = request.RequestContext?.Http?.Path?.ToLower();
                if (!string.IsNullOrEmpty(path) && path.EndsWith("/"))
                {
                    path = path.Substring(0, path.Length - 1);
                }
                string[] pathArray = path.Split('/');

                if (pathArray.Length < 3 || !string.Equals(pathArray[1], "api"))
                {
                    string errMsg = $"Did not recongize path {path}\r\n{DBHelper.Serialize(request)}";
                    return ReturnError(404, errMsg);
                }
                string controller = pathArray[2];
                string apiMethod = string.Empty;
                if (pathArray.Length > 3) apiMethod = pathArray[3];

                NameValueCollection formData = ParseFormData(request);

                string responseContent = string.Empty;

                #region Parse Path
                #region Auth
                if (string.Equals(controller, "auth"))
                {
                    if (string.Equals(apiMethod, "loginschwab"))
                    {
                        ScwToken? token = await AuthInitialize();
                        string callbackUrl = await _configurationSvc.Get("SchwabCallbackUrl");
                        string url = await _schwabSvc.GetSchwabLoginUrl(callbackUrl);
                        return ReturnRedirect(url);
                    }
                    if (string.Equals(apiMethod, "callbackschwaburl"))
                    {
                        string responseUrl = FormValue<string>(formData, "ResponseUrl");
                        responseContent = await AuthSchwabCallback(responseUrl);
                    }
                }
                #endregion

                #region Blog
                if (string.Equals(controller, "blog"))
                {
                    if (string.Equals(apiMethod, "entries"))
                    {
                        responseContent = DBHelper.Serialize(await _awsContext.BlogEntry.ToListAsync());
                    }
                    if (string.Equals(apiMethod, "entry"))
                    {
                        long id = Convert.ToInt32(ParseUrlParameter(request));
                        responseContent = DBHelper.Serialize(await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Id == id));
                    }
                    if (string.Equals(apiMethod, "entrybytitle"))
                    {
                        string title = QueryValue<string>(request, "title");
                        string noDash = title.Replace("-", string.Empty).ToLower();
                        responseContent = DBHelper.Serialize(await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Title.ToLower() == noDash));
                    }
                    if (string.Equals(apiMethod, "upsert"))
                    {
                        string json = FormValue<string>(formData, "json");
                        BlogEntry? entry = DBHelper.Deserialize<BlogEntry>(json);
                        if (entry != null)
                        {

                            DateTime currentDate = DateTime.UtcNow;
                            entry.ModifiedOn = currentDate;

                            bool isNew = true;
                            if (entry.Id > 0) isNew = false;

                            if (isNew)
                            {
                                entry.CreatedOn = currentDate;
                                entry.ModifiedOn = currentDate;

                                _awsContext.BlogEntry.Add(entry);
                                _awsContext.Entry(entry).State = EntityState.Added;
                                _awsContext.SaveChanges();

                                BlogEntry? result = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Title == entry.Title);
                                responseContent = DBHelper.Serialize(result);
                            }
                            else
                            {
                                entry.ModifiedOn = currentDate;
                                BlogEntry? entity = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Id == entry.Id);
                                if (entity != null)
                                {
                                    BlogCopyObject(entry, entity);

                                    _awsContext.Entry(entity).State = EntityState.Modified;
                                    _awsContext.SaveChanges();
                                }

                                entity = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Id == entry.Id);
                                responseContent = DBHelper.Serialize(entity);
                            }
                        }
                    }
                }
                #endregion

                #region ChartInfo
                if (string.Equals(controller, "chartinfo"))
                {
                    if (string.Equals(apiMethod, "linesinglepost"))
                    {
                        string json = FormValue<string>(formData, "json", false, string.Empty);
                        if (string.IsNullOrEmpty(json))
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no json in formdata");
                        }

                        string title = FormValue<string>(formData, "title");
                        string key = FormValue<string>(formData, "key");
                        int? zoomIn = FormValue<int?>(formData, "zoomIn", false);

                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            ChartInfo info = _chartSvc.LineSingle(sc, title, key);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "linedoublepost"))
                    {
                        string json = FormValue<string>(formData, "json", false, string.Empty);
                        if (string.IsNullOrEmpty(json))
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no json in formdata");
                        }

                        string title = FormValue<string>(formData, "title");
                        string key = FormValue<string>(formData, "key");
                        int? zoomIn = FormValue<int?>(formData, "zoomIn", false);

                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            ChartInfo info = _chartSvc.LineDouble(sc, title, key, zoomIn);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "maxpainpost"))
                    {
                        string json = FormValue<string>(formData, "json");

                        MPChain? mpc = DBHelper.Deserialize<MPChain>(json);
                        if (mpc != null)
                        {
                            ChartInfo info = _chartSvc.MaxPain(mpc);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "maxpainpostb64"))
                    {
                        string b64 = FormValue<string>(formData, "b64");
                        byte[] buffer = System.Convert.FromBase64String(b64);
                        string json = ReadGZip(buffer);

                        MPChain? mpc = DBHelper.Deserialize<MPChain>(json);
                        if (mpc != null)
                        {
                            ChartInfo info = _chartSvc.MaxPain(mpc);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "stackedpost"))
                    {
                        string json = FormValue<string>(formData, "json", false, string.Empty);
                        if (string.IsNullOrEmpty(json))
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no json in formdata");
                        }

                        string title = FormValue<string>(formData, "title");
                        string key = FormValue<string>(formData, "key");
                        int zoomIn = FormValue<int>(formData, "zoomIn", false, "100");

                        bool isPut = false;
                        if (title.ToLower().IndexOf("put") != -1) isPut = true;

                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            ChartInfo info = _chartSvc.Stacked(sc, title, key, isPut);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "ivpredictpost"))
                    {
                        string json = FormValue<string>(formData, "json", false, string.Empty);
                        if (string.IsNullOrEmpty(json))
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no json in formdata");
                        }

                        string title = FormValue<string>(formData, "title");
                        string key = FormValue<string>(formData, "key");
                        int? zoomIn = FormValue<int?>(formData, "zoomIn", false);
                        int degree = FormValue<int>(formData, "degree");

                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            ChartInfo info = _chartSvc.IVPredict(sc, title, key, degree);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "pricepost"))
                    {
                        string json = FormValue<string>(formData, "json", false, string.Empty);
                        if (string.IsNullOrEmpty(json))
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no json in formdata");
                        }

                        string title = FormValue<string>(formData, "title");
                        int? zoomIn = FormValue<int?>(formData, "zoomIn", false);
                        bool isPut = FormValue<bool>(formData, "isPut", false, "false");

                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            ChartInfo info = _chartSvc.Price(sc, title, isPut);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "historydoublepost"))
                    {
                        string json = FormValue<string>(formData, "json", false, string.Empty);
                        if (string.IsNullOrEmpty(json))
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no json in formdata");
                        }

                        string title = FormValue<string>(formData, "title");
                        string key = FormValue<string>(formData, "key");

                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            ChartInfo info = _chartSvc.HistoryLineDouble(sc, title, key);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                    if (string.Equals(apiMethod, "historymaxpainpost"))
                    {
                        string json = FormValue<string>(formData, "json", false, string.Empty);
                        if (string.IsNullOrEmpty(json))
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no json in formdata");
                        }

                        string title = FormValue<string>(formData, "title");
                        string key = FormValue<string>(formData, "key");
                        int? zoomIn = FormValue<int?>(formData, "zoomIn", false);

                        bool showStock = true;
                        List<MaxPainHistory>? histories = DBHelper.Deserialize<List<MaxPainHistory>>(json);
                        if (histories != null)
                        {
                            ChartInfo info = _chartSvc.HistoryMaxPain(histories, showStock);
                            responseContent = DBHelper.Serialize(info);
                        }
                    }
                }
                #endregion

                #region Email
                if (string.Equals(controller, "email"))
                {
                    if (string.Equals(apiMethod, "send"))
                    {
                        string json = FormValue<string>(formData, "json");
                        EmailMessage? msg = DBHelper.Deserialize<EmailMessage>(json);
                        if (msg != null)
                        {
                            responseContent = await _emailSvc.SendEmail(msg.From, msg.To, msg.CC, msg.BCC, msg.Subject, msg.Body, msg.AttachmentCSV, msg.IsHtml);
                        }
                    }
                    if (string.Equals(apiMethod, "test"))
                    {
                        await _loggerSvc.InfoAsync("test method", BuildLogMessage(request));
                        string to = QueryValue<string>(request, "to", false, "dan.shipe@yahoo.com");
                        EmailMessage msg = new EmailMessage()
                        {
                            From = "info@maximum-pain.com",
                            To = to,
                            Subject = "Test Email",
                            Body = "Test Body"
                        };

                        responseContent = await _emailSvc.SendEmail(msg.From, msg.To, msg.CC, msg.BCC, msg.Subject, msg.Body, msg.AttachmentCSV, msg.IsHtml);
                    }
                    if (string.Equals(apiMethod, "image"))
                    {
                        string? ticker = QueryValue<string>(request, "ticker", false);
                        if (string.IsNullOrEmpty(ticker)) ticker = "SPX";

                        byte[] buffer = await _emailSvc.GetEmailImage(ticker);
                        return ReturnImage(buffer);
                    }
                }
                #endregion

                #region EmailList
                if (string.Equals(controller, "emaillist"))
                {
                    if (string.Equals(apiMethod, "stats"))
                    {
                        responseContent = DBHelper.Serialize(await _awsContext.EmailStat.ToListAsync());
                    }
                    if (string.Equals(apiMethod, "screener"))
                    {
                        DateTime timestamp = QueryValue<DateTime>(request, "timestamp");
                        bool debug = QueryValue<bool>(request, "debug", false, "false");
                        bool useShortUrls = QueryValue<bool>(request, "useShortUrls", false, "true");
                        bool runNow = QueryValue<bool>(request, "runNow", false, "true");
                        string pw = QueryValue<string>(request, "password");

                        await _loggerSvc.InfoAsync($"Lambda:EmailList:Screener BEGIN debug={debug} useShortUrls={useShortUrls} runNow={runNow}", string.Empty);

                        string passPhrase = await _secretSvc.GetValue("PassPhrase");
                        if (pw == null || !pw.Equals(passPhrase))
                        {
                            await _loggerSvc.InfoAsync($"Lambda:EmailList:Screener END password is incorrect", string.Empty);
                            return ReturnError(502, "Password is incorrect");
                        }

                        responseContent = await _emailSvc.ScreenerGenerate(true, useShortUrls, string.Empty, false);
                        await _loggerSvc.InfoAsync($"Lambda:EmailList:Screener END", string.Empty);
                    }
                    if (string.Equals(apiMethod, "subscribe"))
                    {
                        await _loggerSvc.InfoAsync(apiMethod, BuildLogMessage(request));

                        string name = FormValue<string>(formData, "name", false, string.Empty);
                        string email = FormValue<string>(formData, "email");
                        await _emailSvc.Subscribe(name, email);
                        responseContent = $"Confirmation sent to {email}";
                    }
                    if (string.Equals(apiMethod, "confirm"))
                    {
                        await _loggerSvc.InfoAsync(apiMethod, BuildLogMessage(request));

                        string name = string.Empty;
                        string email = QueryValue<string>(request, "email");
                        await _emailSvc.Confirm(name, email);
                        responseContent = $"maximum-pain.com:  {email} has been confirmed.";
                    }
                    if (string.Equals(apiMethod, "unsubscribe"))
                    {
                        await _loggerSvc.InfoAsync(apiMethod, BuildLogMessage(request));

                        string name = string.Empty;
                        string email = QueryValue<string>(request, "email");
                        await _emailSvc.Unsubscribe(name, email);
                        responseContent = $"maximum-pain.com:  {email} has been unsubscribed.";
                    }
                }
                #endregion

                #region FinImport
                if (string.Equals(controller, "finimport"))
                {
                    if (string.Equals(apiMethod, "runimport"))
                    {
                        DateTime utc = QueryValue<DateTime>(request, "utc", false, DateTime.MinValue.ToString());
                        bool debug = QueryValue<bool>(request, "debug", false, "false");
                        bool sendEmail = QueryValue<bool>(request, "sendEmail", false, "false");
                        bool useMessage = QueryValue<bool>(request, "useMessage", false, "true");
                        string pw = QueryValue<string>(request, "pw");

                        await _loggerSvc.InfoAsync($"Lambda:FinImport:RunImport BEGIN debug={debug} utc={utc} sendEmail={sendEmail}", string.Empty);

                        string passPhrase = await _secretSvc.GetValue("PassPhrase");
                        if (string.Compare(pw, passPhrase, false) != 0)
                        {
                            await _loggerSvc.InfoAsync($"Lambda:FinImport:RunImport  END password is incorrect.  passPhrase=\"{passPhrase}\" pw=\"{pw}\"", string.Empty);
                            responseContent = $"Lambda:FinImport:RunImport END debug={debug} utc={utc} sendEmail={sendEmail}\r\npassword is incorrect";
                        }
                        else
                        {
                            _finImportSvc.UseMessage = true; // useMessage;
                            _finImportSvc.IsDebug = debug;
                            _finImportSvc.ImportDateUTC = utc;

                            string log = await _finImportSvc.RunImport();

                            /*
                            DateTime utc = DateTime.UtcNow;
                            DateTime est = Utility.GMTToEST(utc);
                            int currentTime = Convert.ToInt32(est.ToString("HHmm"));
                            */

                            if (sendEmail && _finImportSvc.IsMarketOpen)
                            {
                                string xml = await _awsContext.SettingsRead();
                                XmlDocument xmlSettings = new XmlDocument();
                                xmlSettings.LoadXml(xml);

                                await _loggerSvc.InfoAsync($"FinImportController.RunImport SEND EMAIL sendEmail={sendEmail} isMarketOpen={_finImportSvc.IsMarketOpen} debug={debug}", string.Empty);
                                string imageTicker = await _configurationSvc.Get("ScreenerImageTicker");
                                string html = await _emailSvc.ScreenerGenerate(true, true, string.Empty, false);
                                await _loggerSvc.InfoAsync($"FinImportController.RunImport END", string.Empty);
                            }

                            responseContent = log;
                        }
                    }
                    if (string.Equals(apiMethod, "getlastdatemarketopen"))
                    {
                        DateTime est = MaxPainInfrastructure.Code.Utility.CurrentDateEST();
                        est = QueryValue<DateTime>(request, "est", false, est.ToString());
                        responseContent = (await _finImportSvc.GetLastDayMarketOpen(est)).ToString();
                    }
                    if (string.Equals(apiMethod, "sendemail"))
                    {
                        string xml = await _awsContext.SettingsRead();
                        XmlDocument xmlSettings = new XmlDocument();
                        xmlSettings.LoadXml(xml);

                        await _loggerSvc.InfoAsync($"FinImportController.SendEmail SEND EMAIL", string.Empty);
                        string imageTicker = await _configurationSvc.Get("ScreenerImageTicker");
                        string html = await _emailSvc.ScreenerGenerate(true, true, string.Empty, false);
                        await _loggerSvc.InfoAsync($"FinImportController.SendEmail END", string.Empty);

                        responseContent = html;
                    }
                    if (string.Equals(apiMethod, "importoptions"))
                    {
                        DateTime utc = QueryValue<DateTime>(request, "utc");
                        bool debug = QueryValue<bool>(request, "debug", false, "false");
                        bool saveMessage = QueryValue<bool>(request, "saveMessage", false, "true");
                        string tickersCSV = QueryValue<string>(request, "tickersCSV");
                        string pw = QueryValue<string>(request, "password");

                        string passPhrase = await _secretSvc.GetValue("PassPhrase");
                        if (pw == null || !pw.Equals(passPhrase))
                        {
                            responseContent = $"Lambda:FinImport:importoptions password is incorrect";
                        }
                        else
                        {
                            _finImportSvc.IsDebug = debug;
                            _finImportSvc.UseMessage = saveMessage;
                            _finImportSvc.ImportDateUTC = utc;

                            if (!string.IsNullOrEmpty(tickersCSV)) _finImportSvc.TickersCSV = tickersCSV;

                            DateTime? dt = await _finImportSvc.FetchMarketDate();
                            if (!dt.HasValue)
                            {
                                await _finImportSvc.ImportOptions(dt.Value);
                            }
                            responseContent = _finImportSvc.GetLog();
                        }
                    }
                    if (string.Equals(apiMethod, "importstocks"))
                    {
                        DateTime dt = QueryValue<DateTime>(request, "dt");
                        bool debug = QueryValue<bool>(request, "debug", false, "false");
                        bool saveMessage = QueryValue<bool>(request, "saveMessage", false, "true");
                        string pw = QueryValue<string>(request, "password");

                        string passPhrase = await _secretSvc.GetValue("PassPhrase");
                        if (pw == null || !pw.Equals(passPhrase))
                        {
                            responseContent = $"Lambda:FinImport:importstocks password is incorrect";
                        }
                        else
                        {
                            responseContent = await _finImportSvc.ImportStocks(dt);
                        }
                    }
                    if (string.Equals(apiMethod, "getmarketcalendar"))
                    {
                        DateTime min = QueryValue<DateTime>(request, "min");
                        DateTime max = QueryValue<DateTime>(request, "max");

                        List<DateTime> cal = new List<DateTime>();
                        DateTime loopDate = min;
                        while (loopDate <= max)
                        {
                            bool result = await _finDataSvc.IsMarketOpen(loopDate);
                            if (result)
                            {
                                cal.Add(loopDate);
                            }
                            loopDate = loopDate.AddDays(1);
                        }

                        responseContent = DBHelper.Serialize(await _homeContext.MarketCalendar.Where(mc => mc.Date >= min && mc.Date <= max).ToListAsync());
                    }
                    if (string.Equals(apiMethod, "showmarketcalendar"))
                    {
                        bool top30 = QueryValue<bool>(request, "top30", false, "true");
                        if (top30)
                        {
                            responseContent = DBHelper.Serialize(await _homeContext.MarketCalendar
                                .OrderByDescending(c => c.Date)
                                .Take(30)
                                .ToListAsync());
                        }
                        else
                        {
                            responseContent = DBHelper.Serialize(await _homeContext.MarketCalendar
                                .ToListAsync());
                        }
                    }
                    if (string.Equals(apiMethod, "showhistorydate"))
                    {
                        responseContent = DBHelper.Serialize(await _historySvc.GetHistoryDate());
                    }
                    if (string.Equals(apiMethod, "importmaxpain"))
                    {
                        responseContent = DBHelper.Serialize(await _homeContext.ImportMaxPainRecentRead());
                    }
                    if (string.Equals(apiMethod, "transform"))
                    {
                        DateTime start = QueryValue<DateTime>(request, "start");
                        //string pw = QueryValue<string>(request, "password");
                        responseContent = await _finImportSvc.DoTransform(start);
                    }
                    if (string.Equals(apiMethod, "stocktickers"))
                    {
                        List<PythonTicker>? python = await _awsContext.GetPythonTicker();
                        string json = DBHelper.Serialize(python);
                        List<StockTicker> list = DBHelper.Deserialize<List<StockTicker>>(json);
                        responseContent = DBHelper.Serialize(list);
                    }
                    if (string.Equals(apiMethod, "stocks"))
                    {
                        responseContent = DBHelper.Serialize(await _controllerSvc.GetStocks());
                    }
                    if (string.Equals(apiMethod, "importdatecount"))
                    {
                        int count = QueryValue<int>(request, "count", false, "30");
                        string sql = @"
			            SELECT CreatedOn, COUNT(*) AS Records 
			            FROM HistoricalOptionQuoteXML 
			            WHERE CreatedOn >DATEADD(dd, -30, GETUTCDATE())
			            GROUP BY CreatedOn
			            ORDER BY CreatedOn DESC
		            ";
                        responseContent = await _homeContext.FetchJson(sql, null, 30);
                    }
                    if (string.Equals(apiMethod, "cachedatecount"))
                    {
                        int count = QueryValue<int>(request, "count", false, "30");
                        string sql = @"
				        SELECT CreatedOn, COUNT(*) AS Records 
				        FROM ImportCache
				        WHERE CreatedOn >DATEADD(dd, -15, GETUTCDATE())
				        GROUP BY CreatedOn
				        ORDER BY CreatedOn DESC
		            ";
                        responseContent = await _homeContext.FetchJson(sql, null, 30);
                    }
                    if (string.Equals(apiMethod, "importlog"))
                    {
                        int count = QueryValue<int>(request, "count", false, "30");
                        List<ImportLog> logs = await _homeContext.ImportLog.ToListAsync();
                        logs = logs.OrderByDescending(x => x.ID).Take(count).ToList();
                        responseContent = DBHelper.Serialize(logs);
                    }
                    if (string.Equals(apiMethod, "rebuildpains"))
                    {
                        DateTime begin = QueryValue<DateTime>(request, "begin");
                        DateTime end = QueryValue<DateTime>(request, "end");
                        responseContent = DBHelper.Serialize(await _finImportSvc.RebuildPains(begin, end));
                    }
                    if (string.Equals(apiMethod, "patchvolume"))
                    {
                        DateTime importDate = QueryValue<DateTime>(request, "importDate");
                        string ticker = QueryValue<string>(request, "ticker");

                        DateTime start = DateTime.UtcNow;
                        int updated = await _finImportSvc.PatchVolume(importDate, ticker);
                        DateTime complete = DateTime.UtcNow;

                        var jsonObj = new
                        {
                            importDate = importDate,
                            start = start,
                            complete = complete,
                            count = updated
                        };

                        responseContent = DBHelper.Serialize(jsonObj);
                    }
                    if (string.Equals(apiMethod, "mostactive"))
                    {
                        DateTime utc = QueryValue<DateTime>(request, "utc");
                        bool debug = QueryValue<bool>(request, "debug", false, "false");

                        HistoryDate history = await _historySvc.GetHistoryDate();
                        DateTime currentDate = history.CurrentDate;
                        DateTime previousDate = history.PreviousDate;

                        if (utc != DateTime.MinValue)
                        {
                            currentDate = utc;
                            previousDate = await _historySvc.PreviousMarketCalendar(currentDate);
                        }

                        List<OptChn> currentList = await _historySvc.ChainGetByDate(currentDate);

                        _finImportSvc.IsDebug = debug;
                        _finImportSvc.UseMessage = true;
                        List<MostActive> actives = await _finImportSvc.MostActive(currentList, previousDate);

                        responseContent = DBHelper.Serialize(actives);
                    }
                    if (string.Equals(apiMethod, "outsideoiwalls"))
                    {
                        DateTime utc = QueryValue<DateTime>(request, "utc");
                        bool debug = QueryValue<bool>(request, "debug", false, "false");

                        HistoryDate history = await _historySvc.GetHistoryDate();
                        DateTime currentDate = history.CurrentDate;
                        DateTime previousDate = history.PreviousDate;

                        if (utc != DateTime.MinValue)
                        {
                            currentDate = utc;
                            previousDate = await _historySvc.PreviousMarketCalendar(currentDate);
                        }

                        List<SdlChn> straddles = new List<SdlChn>();
                        List<OptChn> options = await _historySvc.ChainGetByDate(currentDate);
                        foreach (OptChn oc in options)
                        {
                            straddles.Add(_calculationSvc.BuildStraddle(oc));
                        }

                        _finImportSvc.IsDebug = debug;
                        _finImportSvc.UseMessage = true;
                        List<OutsideOIWalls> walls = await _finImportSvc.OutsideOIWalls(straddles);

                        responseContent = DBHelper.Serialize(walls);
                    }
                }
                #endregion

                #region HealthCheck
                if (string.Equals(controller, "healthcheck"))
                {
                    if (string.Equals(controller, "get"))
                    {
                        DateTime timestamp = QueryValue<DateTime>(request, "timestamp");
                        bool debug = QueryValue<bool>(request, "debug", false, "false");

                        string xml = await _awsContext.SettingsRead();
                        XmlDocument xmlSettings = new XmlDocument();
                        xmlSettings.LoadXml(xml);

                        bool status = await _controllerSvc.HealthCheck(xmlSettings, debug);

                        await _awsContext.SettingsPost(xmlSettings.OuterXml);

                        responseContent = status.ToString();
                    }
                }
                #endregion

                #region History
                if (string.Equals(controller, "history"))
                {
                    if (string.Equals(apiMethod, "straddle"))
                    {
                        // https://localhost:44301/api/history/straddle/BAC?pastDays=35&futureDays=14
                        var ticker = ParseUrlParameter(request);
                        int pastDays = QueryValue<int>(request, "pastDays", false, "15");
                        int futureDays = QueryValue<int>(request, "futureDays", false, "7");

                        OptChn? chain = await _historySvc.GetByTicker(ticker, pastDays, futureDays);

                        if (chain == null || chain.Options == null || chain.Options.Count == 0)
                        {
                            return ReturnError(502, $"{controller} {apiMethod} no Option History for {ticker}");
                        }

                        SdlChn sc = _calculationSvc.BuildStraddle(chain);
                        // convert YMD back into a date
                        sc.Straddles.ForEach(x => x.d = Convert.ToDateTime($"{x.d.Substring(2, 2)}/{x.d.Substring(4, 2)}/{x.d.Substring(0, 2)}").ToString("MM/dd/yy"));
                        responseContent = DBHelper.Serialize(sc);
                    }
                    if (string.Equals(apiMethod, "maxpainpost"))
                    {
                        string json = FormValue<string>(formData, "json");
                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            List<MaxPainHistory> histories = _calculationSvc.CalculateMaxPainHistory(sc);
                            responseContent = DBHelper.Serialize(histories);
                        }
                    }
                    if (string.Equals(apiMethod, "getbydate"))
                    {
                        DateTime utc = QueryValue<DateTime>(request, "utc", false, DateTime.UtcNow.ToString());
                        responseContent = DBHelper.Serialize(await _historySvc.GetByDate(utc));
                    }
                }
                #endregion

                #region Hop
                if (string.Equals(controller, "hop"))
                {
                    if (string.Equals(apiMethod, "cb"))
                    {
                        string v = QueryValue<string>(request, "v");
                        string dstUrl = $"http://scrptcal.{v}.hop.clickbank.net";
                        Hop hop = await InsertHop(request, dstUrl);
                        ReturnRedirect(dstUrl);
                    }
                    if (string.Equals(apiMethod, "detail"))
                    {
                        responseContent = DBHelper.Serialize(await _awsContext.Hop.OrderByDescending(x => x.Id).ToListAsync());
                    }
                    if (string.Equals(apiMethod, "summary"))
                    {
                        string sql = @"
                        SELECT
	                        dateadd(dd,0, datediff(dd,0, CreatedOn)) AS CreatedOn
	                        ,Destination
	                        ,COUNT(*) AS Hops
                        FROM Hop
                        GROUP BY dateadd(dd,0, datediff(dd,0, CreatedOn)), Destination
                    ";

                        responseContent = await _awsContext.FetchJson(sql, null, 30);
                    }
                    if (string.Equals(apiMethod, "agent"))
                    {
                        string sql = @"
                        SELECT
	                        UserAgent
	                        ,COUNT(*) AS Hops
                        FROM Hop
                        GROUP BY UserAgent
                    ";

                        responseContent = await _awsContext.FetchJson(sql, null, 30);
                    }
                }
                #endregion

                #region Message
                if (string.Equals(controller, "message"))
                {
                    // default 
                    if (string.IsNullOrEmpty(apiMethod))
                    {
                        responseContent = await MessageGet(request.QueryStringParameters?["id"]);
                    }
                    if (string.Equals(apiMethod, "truncate"))
                    {
                        string sql = "TRUNCATE TABLE [Message]";
                        await _awsContext.Database.ExecuteSqlRawAsync(sql);
                        responseContent = await MessageGet(null);
                    }
                    if (string.Equals(apiMethod, "create"))
                    {
                        string subject = QueryValue<string>(request, "subject");
                        string body = QueryValue<string>(request, "body");
                        await _loggerSvc.InfoAsync(subject, body);
                        responseContent = await MessageGet(null);
                    }
                }
                #endregion

                #region Options
                if (string.Equals(controller, "options"))
                {
                    if (string.Equals(apiMethod, "chain"))
                    {
                        string ticker = ParseUrlParameter(request);
                        DateTime maturity = QueryValue<DateTime>(request, "m", false, DateTime.MinValue.ToString());
                        OptChn chain = await _finDataSvc.FetchOptionChain(ticker, maturity, false);
                        responseContent = DBHelper.Serialize(chain);
                    }
                    if (string.Equals(apiMethod, "straddle"))
                    {
                        //_loggerSvc.InfoAsync("options/straddle", "options/straddle");
                        string ticker = ParseUrlParameter(request);
                        DateTime maturity = QueryValue<DateTime>(request, "m", false, DateTime.MinValue.ToString());
                        OptChn chain = await _finDataSvc.FetchOptionChain(ticker, maturity, false);
                        responseContent = DBHelper.Serialize(_calculationSvc.BuildStraddle(chain));
                    }
                    if (string.Equals(apiMethod, "maxpainpost"))
                    {
                        string json = FormValue<string>(formData, "json");
                        SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);
                        if (sc != null)
                        {
                            responseContent = DBHelper.Serialize(_calculationSvc.Calculate(sc));
                        }
                    }
                    if (string.Equals(apiMethod, "csv"))
                    {
                        string ticker = ParseUrlParameter(request);
                        List<ScwOptionCSV> list = await _finDataSvc.FetchOptionCSV(ticker);
                        responseContent = DBHelper.Serialize(list);
                    }
                }
                #endregion

                #region Python
                if (string.Equals(controller, "python"))
                {
                    if (string.Equals(apiMethod, "cupwithhandlehistory"))
                    {
                        DateTime? midnight = QueryValue<DateTime?>(request, "midnight", false, DateTime.MinValue.ToString());
                        if (midnight == DateTime.MinValue || midnight == Convert.ToDateTime("01/01/1900"))
                        {
                            midnight = null;
                        }
                        responseContent = DBHelper.Serialize(await _homeContext.GetCupWithHandleHistory(midnight));
                    }
                    if (string.Equals(apiMethod, "getdailyscandates"))
                    {
                        responseContent = DBHelper.Serialize(await _homeContext.DailyScanDates());
                    }
                    if (string.Equals(apiMethod, "getdailyscan"))
                    {
                        DateTime midnight = QueryValue<DateTime>(request, "midnight");
                        responseContent = DBHelper.Serialize(await _homeContext.DailyScan(midnight));
                    }
                    if (string.Equals(apiMethod, "adddailyscan"))
                    {
                        string ticker = QueryValue<string>(request, "ticker");
                        DBHelper.Serialize(await _homeContext.DailyScanAdd(ticker));
                        var midnight = await _homeContext.DailyScanMaxDate();
                        responseContent = DBHelper.Serialize(await _homeContext.DailyScan(midnight));
                    }
                    if (string.Equals(apiMethod, "dailymonitor"))
                    {
                        await _loggerSvc.InfoAsync("dailymonitor", "dailymonitor");
                        responseContent = await _controllerSvc.DailyMonitor();
                    }
                    if (string.Equals(apiMethod, "dailyscanupdatewatch"))
                    {
                        int id = QueryValue<int>(request, "id");
                        bool flag = QueryValue<bool>(request, "flag");
                        responseContent = DBHelper.Serialize(await _homeContext.DailyScanUpdateWatch(id, flag));
                    }
                    if (string.Equals(apiMethod, "getmarketdirection"))
                    {
                        responseContent = DBHelper.Serialize(await _homeContext.MarketDirection());
                    }
                    if (string.Equals(apiMethod, "ec2stop"))
                    {
                        await _loggerSvc.InfoAsync("ec2stop", "ec2stop");
                        var url = "https://clwpt5pzdpdkpnqa32dany2o2m0ytnzi.lambda-url.us-east-1.on.aws?action=stop";
                        await GetUrl(url);
                        responseContent = await MessageGet(null);
                    }
                    if (string.Equals(apiMethod, "ec2start"))
                    {
                        await _loggerSvc.InfoAsync("ec2start", "ec2start");
                        var url = "https://clwpt5pzdpdkpnqa32dany2o2m0ytnzi.lambda-url.us-east-1.on.aws?action=stop";
                        await GetUrl(url);
                        responseContent = await MessageGet(null);
                    }
                }
                #endregion

                #region RemoteData
                if (string.Equals(controller, "remotedata"))
                {
                    if (string.Equals(apiMethod, "mostactive"))
                    {
                        List<MostActive> actives = await _homeContext.MostActive.ToListAsync();
                        actives.ForEach(a => a.QueryType = a.GetQueryType());
                        responseContent = DBHelper.Serialize(actives);
                    }
                    if (string.Equals(apiMethod, "highopeninterest"))
                    {
                        DateTime maturity = MaxPainInfrastructure.Code.Utility.NextFriday();
                        List<HighOpenInterest>? highs = await _homeContext.HighOpenInterestRead(maturity);
                        responseContent = DBHelper.Serialize(highs);
                    }
                    if (string.Equals(apiMethod, "outsideoiwalls"))
                    {
                        List<OutsideOIWalls>? walls = await _homeContext.OutsideOIWalls.ToListAsync();
                        responseContent = DBHelper.Serialize(walls);
                    }
                    if (string.Equals(apiMethod, "historicalmaxpain"))
                    {
                        string ticker = QueryValue<String>(request, "ticker");
                        DateTime m = QueryValue<DateTime>(request, "m");
                        List<MaxPainHistory>? items = await _homeContext.MaxPainHistoryRead(ticker, m);
                        responseContent = DBHelper.Serialize(items);
                    }
                }
                #endregion

                #region Settings
                if (string.Equals(controller, "settings"))
                {
                    if (string.Equals(apiMethod, "interval"))
                    {
                        await _controllerSvc.DailyMonitor();
                        responseContent = DateTime.UtcNow.ToString();
                    }

                    if (string.Equals(apiMethod, "scheduledtask"))
                    {
                        string timestamp = ParseUrlParameter(request);
                        bool debug = QueryValue<bool>(request, "debug", false, "false");

                        List<string> tasks = await _controllerSvc.ScheduledTask(Convert.ToBoolean(debug));

                        var jsonObj = new
                        {
                            server = System.Environment.MachineName,
                            buildDate = string.Empty,
                            result = tasks
                        };

                        responseContent = DBHelper.Serialize(jsonObj);
                    }
                    if (string.Equals(apiMethod, "readjson"))
                    {
                        string xml = await _configurationSvc.ReadXml();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);
                        responseContent = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
                    }
                    if (string.Equals(apiMethod, "readxml"))
                    {
                        responseContent = await _configurationSvc.ReadXml();
                    }
                    if (string.Equals(apiMethod, "get"))
                    {
                        string key = QueryValue<string>(request, "key");
                        responseContent = await _configurationSvc.Get(key);
                    }
                    if (string.Equals(apiMethod, "set"))
                    {
                        string key = QueryValue<string>(request, "key");
                        string value = QueryValue<string>(request, "value");
                        responseContent = await _configurationSvc.Get(key);
                    }
                    if (string.Equals(apiMethod, "setpost"))
                    {
                        string key = FormValue<string>(formData, "key");
                        string value = FormValue<string>(formData, "value");
                        responseContent = await _configurationSvc.Get(key);
                    }
                    if (string.Equals(apiMethod, "savexml"))
                    {
                        string xml = FormValue<string>(formData, "xml");
                        await _configurationSvc.SaveXml(xml);
                        responseContent = xml;
                    }
                    if (string.Equals(apiMethod, "savejson"))
                    {
                        string json = FormValue<string>(formData, "json");
                        XmlDocument? doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(json);
                        if (doc != null)
                        {
                            await _configurationSvc.SaveXml(doc.OuterXml);
                            responseContent = doc.OuterXml;
                        }
                    }
                    if (string.Equals(apiMethod, "serverdetail"))
                    {
                        var createdOn = await _finDataSvc.GetCreatedOn();
                        var jsonObj = new
                        {
                            MachineName = createdOn.AddDays(7), //System.Environment.MachineName,
                            TokenCreatedOn = createdOn,
                            TokenExpireOn = createdOn.AddDays(7)
                        };

                        responseContent = DBHelper.Serialize(jsonObj);
                    }
                }
                #endregion

                #region Test
                if (string.Equals(controller, "test"))
                {
                    if (string.Equals(apiMethod, "account"))
                    {
                        var result = await _finDataSvc.Schwab_Account();
                        responseContent = DBHelper.Serialize(result);
                    }

                    if (string.Equals(apiMethod, "textmsg"))
                    {
                        string msg = QueryValue<string>(request, "msg");
                        //responseContent = await _controllerSvc.SendMessageToMobileAsync("1", "4043086715", msg);
                        responseContent = await _smsSvc.SendWhatsapp(msg);
                    }
                }
                #endregion
                #endregion

                timer.Stop();
                if (timer.ElapsedMilliseconds > 10 * 1000)
                {
                    string msg = $"timer=\"{timer.ElapsedMilliseconds} ms\"\r\n\r\n{BuildLogMessage(request, true)}";
                    await _loggerSvc.InfoAsync($"Function.cs ran for {timer.ElapsedMilliseconds} ms", msg);
                }

                if (string.IsNullOrEmpty(responseContent))
                {
                    string errMsg = $"Did not recongize path {path}\r\n{DBHelper.Serialize(request)}";
                    return ReturnError(404, errMsg);
                }

                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    StatusCode = 200,
                    Body = responseContent
                };
            }
            catch (Exception ex)
            {
                await _loggerSvc.InfoAsync("Lambda Error", $"{ex.ToString()}\r\n\r\n{BuildLogMessage(request)}");
                throw;
            }
        }

        #region helper
        private string SwapTicker(string ticker)
        {
            if (string.Compare(ticker, "SPXW", true) == 0)
            {
                //string msg = $"SwapTicker {ticker} to SPX";
                //_loggerSvc.InfoAsync(msg, "");
                return "SPX";
            }
            return ticker;
        }

        public string BuildLogMessage(APIGatewayHttpApiV2ProxyRequest request, bool isSummary = false)
        {
            NameValueCollection formData = new NameValueCollection();
            string? path = request.RequestContext.Http.Path.ToLower();

            var dict = new Dictionary<string?, string?>();
            if (formData != null && formData.AllKeys != null) dict = formData.AllKeys.ToDictionary(k => k, k => formData[k]);

            string summary = $"path={path}\r\nquerystring={DBHelper.Serialize(request.QueryStringParameters)}\r\nform={DBHelper.Serialize(dict)}";
            if (isSummary) return summary;
            return $"{summary}\r\n\r\nrequest={DBHelper.Serialize(request)}";
        }

        private async Task<string> GetUrl(string url)
        {
            string result = string.Empty;
            using (var httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            return result;
        }
        #endregion

        #region Function Url
        private string ReadGZip(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress, true))
            using (StreamReader unzip = new StreamReader(zip))
                return unzip.ReadToEnd();
        }

        private static string Decompress(byte[] data)
        {
            // Read the last 4 bytes to get the length
            byte[] lengthBuffer = new byte[4];
            Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
            int uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[uncompressedSize];
            using (var ms = new MemoryStream(data))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    gzip.Read(buffer, 0, uncompressedSize);
                }
            }
            string json = Encoding.UTF8.GetString(buffer);
            return json;
        }

        private NameValueCollection ParseFormData(APIGatewayHttpApiV2ProxyRequest request)
        {
            if (request.Body == null)
            {
                return new NameValueCollection();
            }

            // translate from Base64
            byte[] buffer = Convert.FromBase64String(request.Body);
            string escaped = System.Text.Encoding.UTF8.GetString(buffer);

            // translate from UrlEncoded
            string decoded = HttpUtility.UrlDecode(escaped);

            // parse from & delimited QueryString
            return HttpUtility.ParseQueryString(decoded);
        }

        private T FormValue<T>(NameValueCollection formData, string key, bool raiseError = true, string? defaultValue = null)
        {
            string? content = null;
            if (formData != null && formData.AllKeys.Contains(key)) content = formData[key];
            if (string.Compare(content, "null", true) == 0) content = null;
            if (content == null && defaultValue != null) content = defaultValue;
            if (raiseError && content == null) throw new ArgumentException($"missing form data \"{key}\"");

            if (!string.IsNullOrEmpty(content) && string.Compare(key, "ticker", true) == 0)
            {
                content = SwapTicker(content);
            }

            if (string.Compare(key, "json", true) == 0 && !content.StartsWith('{'))
            {
                // for some reason base64 contains empty spaces
                StringBuilder sb = new StringBuilder(content);
                sb.Replace(" ", "+");

                // decode the application/x-www-form-urlencoded value
                byte[] buffer = Convert.FromBase64String(sb.ToString());

                // unzip the value
                string unzipped = ReadGZip(buffer);

                // remove first and last double quote
                // unescape new line
                // unescape quotes
                sb = new StringBuilder(unzipped.Substring(1, unzipped.Length - 2));
                sb.Replace("\\n", string.Empty);
                sb.Replace("\\", string.Empty);
                content = sb.ToString();
                //await _loggerSvc.InfoAsync($"decomp json {request.RequestContext?.Http?.Path?}", content);
            }

            return ChangeType<T>(content);
        }

        private T QueryValue<T>(APIGatewayHttpApiV2ProxyRequest request, string key, bool raiseError = true, string? defaultValue = null)
        {
            string? content = null;
            if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(key)) content = request.QueryStringParameters?[key];
            if (string.Compare(content, "null", true) == 0) content = null;
            if (string.IsNullOrEmpty(content) && defaultValue != null) content = defaultValue;
            if (raiseError && content == null) throw new ArgumentException($"missing querystring parameter \"{key}\"");

            if (!string.IsNullOrEmpty(content) && string.Compare(key, "ticker", true) == 0)
            {
                content = SwapTicker(content);
            }

            return ChangeType<T>(content);
        }

        private T ChangeType<T>(string? content)
        {
            if (content == null) return default(T);
            Type t = typeof(T);
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                t = Nullable.GetUnderlyingType(t);
            }
            return (T)Convert.ChangeType(content, t);
        }

        private string ParseUrlParameter(APIGatewayHttpApiV2ProxyRequest request)
        {
            string? path = request.RequestContext.Http.Path.ToLower();
            string[] pathArray = path.Split('/');

            string content = pathArray[pathArray.Length - 1];
            content = SwapTicker(content);

            return content;
        }

        private APIGatewayHttpApiV2ProxyResponse ReturnRedirect(string url)
        {
            Dictionary<string, string> headersDict = new Dictionary<string, string>();
            headersDict.Add("Location", url);

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 301,
                Headers = headersDict
            };
        }

        private APIGatewayHttpApiV2ProxyResponse ReturnImage(byte[] buffer)
        {
            Dictionary<string, string> headersDict = new Dictionary<string, string>();
            headersDict.Add("Content-type", "image/png");

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 200,
                Headers = headersDict,
                Body = Convert.ToBase64String(buffer)
            };
        }

        public APIGatewayHttpApiV2ProxyResponse ReturnError(int statusCode, string errMsg)
        {
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = statusCode,
                Body = errMsg
            };
        }

        private APIGatewayHttpApiV2ProxyResponse ReturnDownloadFile(byte[] buffer, string contentType, string filename)
        {
            Dictionary<string, string> headersDict = new Dictionary<string, string>();
            headersDict.Add("Content-Type", contentType);
            headersDict.Add("Content-Disposition", $"attachment; filename=\"filename\"");

            return new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 200,
                Body = Convert.ToBase64String(buffer),
                IsBase64Encoded = true
            };
        }
        #endregion

        #region Auth
        private async Task<ScwToken> AuthInitialize()
        {
            string json = await _configurationSvc.Get("SchwabTokens");
            ScwToken? token = DBHelper.Deserialize<ScwToken>(json);
            return token;
        }

        private async Task<string> AuthSchwabCallback(string responseUrl)
        {
            try
            {
                var uri = new Uri(responseUrl);
                string queryString = uri.Query;
                NameValueCollection queryDictionary = HttpUtility.ParseQueryString(queryString);
                string? code = queryDictionary["code"];

                if (code == null || code.Length == 0)
                {
                    throw new ArgumentException("expected code querystring parameter", nameof(responseUrl));
                }

                ScwToken? token = await AuthInitialize();

                // generate the token
                string callbackUrl = await _configurationSvc.Get("SchwabCallbackUrl");
                token = await _schwabSvc.GetRefreshToken(code, callbackUrl);

                // save then token
                string json = DBHelper.Serialize(token);
                string content = await _configurationSvc.Set("SchwabTokens", json);

                // turn on Schwab
                // content = await SettingsHelper.Set(_awsContext, "UseSchwab", "true");

                // save the responseUrl
                content = await _configurationSvc.Set("SchwabResponseUrl", uri.AbsoluteUri);

                // truncate Options
                _awsContext.Database.ExecuteSqlRaw("TRUNCATE TABLE OptionChainJson");

                return content;
            }
            catch (Exception ex)
            {
                return DBHelper.Serialize(ex);
            }
        }
        #endregion

        #region Message
        public async Task<string> MessageGet(string? param)
        {
            string json = string.Empty;
            if (param == null)
            {
                //List<Message> msgs = await _awsContext.Message.OrderByDescending(x => x.Id).ToListAsync();
                var sql = "select top 200 id, createdon, subject, body from message order by id desc";
                json = await _awsContext.FetchJson(sql, null, 30);
                List<Message> msgs = DBHelper.Deserialize<List<Message>>(json);

                foreach (Message m in msgs)
                {
                    m.Body = Utility.DecodeEncodedNonAsciiCharacters(m.Body);
                    m.Body = m.Body.Replace("\'", "\u0022")
                        .Replace("<", "\u003C")
                        .Replace(">", "\u003E")
                        .Replace("\\n", "\n")
                        .Replace("\\r", "\r");
                }
                json = DBHelper.Serialize(msgs);
            }
            else
            {
                int id = Convert.ToInt32(param);
                Message? msg = await _awsContext.Message.FindAsync(id);
                if (msg != null)
                {
                    msg.Body = HttpUtility.HtmlEncode(msg.Body);
                    json = DBHelper.Serialize(msg);
                }
            }
            return json;
        }
        #endregion

        #region Blog
        private void BlogCopyObject(BlogEntry? src, BlogEntry? dst)
        {
            if (src == null || dst == null) return;

            dst.Title = src.Title;
            dst.ImageUrl = src.ImageUrl;
            dst.Ordinal = src.Ordinal;
            dst.IsActive = src.IsActive;
            dst.ShowOnHome = src.ShowOnHome;
            dst.CreatedOn = src.CreatedOn;
            dst.ModifiedOn = src.ModifiedOn;
            dst.Content = src.Content;
        }
        #endregion

        #region Hop
        private async Task<Hop> InsertHop(APIGatewayHttpApiV2ProxyRequest request, string dstUrl)
        {
            string refr = request.RequestContext.Http.SourceIp;
            string userAgent = request.RequestContext.Http.UserAgent;

            Hop hop = new Hop()
            {
                Destination = dstUrl,
                Referrer = refr,
                UserAgent = userAgent,
                CreatedOn = System.DateTime.UtcNow
            };

            _awsContext.Hop.Add(hop);
            _awsContext.Entry(hop).State = EntityState.Added;
            await _awsContext.SaveChangesAsync();

            return hop;
        }
        #endregion
    }
}
