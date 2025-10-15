using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace MaxPainInfrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ICalculationService _calculation;
        private readonly IChartService _chart;
        private readonly IConfigurationService _configuration;
        private readonly IFinDataService _finData;
        private readonly IHistoryService _history;
        private readonly ILoggerService _logger;
        private readonly IMailerLiteService _mailerLite;
        private readonly ISecretService _secret;
        private readonly IUrlShortService _urlShort;

        public EmailService(
            AwsContext awsContext,
            HomeContext homeContext,
            ILoggerService loggerService,
            ICalculationService calculationService,
            IChartService chartService,
            IConfigurationService configurationService,
            IFinDataService finDataService,
            IHistoryService historyService,
            IMailerLiteService mailerLiteService,
            ISecretService secretService,
            IUrlShortService urlShortService
        )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _logger = loggerService;
            _calculation = calculationService;
            _chart = chartService;
            _configuration = configurationService;
            _finData = finDataService;
            _history = historyService;
            _mailerLite = mailerLiteService;
            _secret = secretService;
            _urlShort = urlShortService;
        }

        #region Standard Email
        public async Task<string> SendEmail(string from, string to, string? cc, string? bcc, string subject, string body, string? attachment)
        {
            return await SendEmail(from, to, cc, bcc, subject, body, attachment, false);
        }

        public async Task<string> SendEmail(string from, string to, string? cc, string? bcc, string subject, string body, string? attachment, bool isHtml)
        {
            return await SendEmailAWS(from, to, cc, bcc, subject, body, attachment, isHtml);
        }

        private async Task<string> SendEmailAWS(string from, string to, string? cc, string? bcc, string subject, string body, string? attachment, bool isHtml)
        {
            var message = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(to);
            if (!string.IsNullOrEmpty(cc)) message.CC.Add(cc);
            if (!string.IsNullOrEmpty(bcc)) message.Bcc.Add(bcc);


            // Supply your SMTP credentials below. Note that your SMTP credentials are different from your AWS credentials.
            string USERNAME = await _secret.GetValue("AWSSMTPUsername");
            string PASSWORD = await _secret.GetValue("AWSSMTPPassword");
            // Amazon SES SMTP host name. This example uses the US West (Oregon) region.
            string HOST = await _secret.GetValue("AWSSMTPHost");
            // The port you will connect to on the Amazon SES SMTP endpoint.
            int PORT = Convert.ToInt32(await _secret.GetValue("AWSSMTPPort"));

            // Create an SMTP client with the specified host name and port.
            try
            {
                using (SmtpClient client = new SmtpClient(HOST, PORT))
                {
                    // Create a network credential with your SMTP user name and password.
                    client.Credentials = new System.Net.NetworkCredential(USERNAME, PASSWORD);

                    // Use SSL when accessing Amazon SES. The SMTP session will begin on an unencrypted connection, and then 
                    // the client will issue a STARTTLS command to upgrade to an encrypted connection using SSL.
                    client.EnableSsl = true;

                    // Send the email. 
                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                string header = $"EmailHelper.SendEmailAWS ERROR to=\"{to}\" subject=\"{subject}\"";
                await _logger.InfoAsync(header, ex.ToString());
            }

            return string.Empty;
        }
        #endregion

        #region Email List
        public async Task<bool> Subscribe(string name, string email)
        {
            var lambda = await _secret.GetValue("ConstLambdaUrl");
            var fromEmail = await _secret.GetValue("ConstEmail");

            string subject = "Please confirm your subscription to maximum-pain.com";
            string link = $"https://{lambda}/api/emaillist/confirm?email={HttpUtility.UrlEncode(email)}";
            string body = $"<p>Please confirm your subscription to maximum-pain.com by clicking the following link</p><p style=\"align:center\"><a href=\"{link}\">Confirm my Email Address</a></p>";
            await SendEmail(fromEmail, email, string.Empty, string.Empty, subject, body, string.Empty, true);

            await EmailListUpdate(name, email, EmailStatus.Active);

            return true;
        }

        public async Task<bool> Confirm(string name, string email)
        {
            await _mailerLite.Subscribe(email, string.Empty);
            await EmailListUpdate(name, email, EmailStatus.Confirmed);
            return true;
        }

        public async Task<bool> Unsubscribe(string name, string email)
        {
            await _mailerLite.Unsubscribe(email);
            await EmailListUpdate(name, email, EmailStatus.Unsubscribed);
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

        public string EmaiListUnsubscribeHTML(string message)
        {
            string html = Utility.GetEmbeddedFile("Unsubscribe.html");
            html = html.Replace("<h3></h3>", $"<h3>{message}</h3>");
            return html;
        }
        #endregion

        #region Daily Screener
        public async Task<bool> SendBulkEmail(string subject, string content, bool debugMode)
        {
            if (debugMode)
            {
                string addr = "info@maximum-pain.com";
                await SendEmail(addr, addr, null, null, subject, content, null, true);
            }

            _mailerLite.DebugMode = debugMode;
            await _mailerLite.SendEmail(subject, content);

            return true;
        }

        private async Task<(bool, string)> ScreenerCheckTime()
        {
            DateTime current = DateTime.UtcNow;

            if (current.DayOfWeek == DayOfWeek.Sunday || current.DayOfWeek == DayOfWeek.Saturday)
            {
                string response = $"NOT RUN.  The day of the week is Saturday or Sunday.  Current date: {current}.  Day of week: {current.DayOfWeek}";
                return (false, response);
            }

            DateTime lastRun = Convert.ToDateTime(_configuration.Get("ScreenerLastRun"));
            int currentDay = Convert.ToInt32(current.ToString("yyyyMMdd"));
            int lastRunDay = Convert.ToInt32(lastRun.ToString("yyyyMMdd"));
            if (lastRunDay >= currentDay)
            {
                string response = $"NOT RUN.  The last ran date was today.  LastRun: {lastRun}";
                return (false, response);
            }

            await _configuration.Set("ScreenerLastRun", current.ToString("MM /dd/yy HH:mm:ss"));
            return (true, string.Empty);
        }

        public async Task<string> ScreenerGenerate(bool runNow, bool useShortUrls, string message, bool debug)
        {
            (bool runFlag, string reason) = await ScreenerCheckTime();
            if (runNow)
            {
                runFlag = true;
                reason = string.Empty;
            }
            if (!runFlag)
            {
                return reason;
            }

            DateTime current = DateTime.UtcNow;
            await _configuration.Set("ScreenerLastRun", current.ToString("MM /dd/yy HH:mm:ss"));

            List<MostActive> actives = await _homeContext.MostActive.ToListAsync();
            actives.ForEach(a => a.QueryType = a.GetQueryType());
            List<OutsideOIWalls> walls = await _homeContext.OutsideOIWalls.ToListAsync();
            //List<ImportMaxPain> pains = await homeContext.ImportMaxPainRecentRead();
            List<Mx> pains = new List<Mx>();

            string imageTicker = await _configuration.Get("ScreenerImageTicker");
            byte[] buffer = await GetEmailImage(imageTicker);

            string xslContent = Utility.GetEmbeddedFile("screener.xsl");
            XmlDocument xmlDom = await GetScreenerXml(actives, walls, pains, imageTicker, buffer);
            string html = await GetScreenerHtml(xmlDom, xslContent, useShortUrls);
            string subject = $"maximum-pain.com daily stock option screener {DateTime.Now:MM/dd/yyyy}";

            if (!string.IsNullOrEmpty(message)) html = message;
            await SendBulkEmail(subject, html, debug);

            await _logger.InfoAsync("ScheduledTask Screener", html);
            return html;
        }

        public async Task<XmlDocument> GetScreenerXml(List<MostActive> actives, List<OutsideOIWalls> walls, List<Mx> pains, string imageTicker, byte[] buffer)
        {
            string xmlActives = Utility.SerializeXmlClean(actives);
            string xmlWalls = Utility.SerializeXmlClean(walls);
            string xmlPains = Utility.SerializeXmlClean(pains);

            var lambda = await _secret.GetValue("ConstLambdaUrl");

            //string unsubsribeUrl = "https://maximum-pain.com/api/emaillist/unsubscribe?email={$email}";
            string unsubsribeUrl = string.Concat("https://", lambda, "/api/emailList/unsubscribe?email={$email}");
            string xml = $"<Root>{xmlActives}{xmlWalls}{xmlPains}</Root>";

            XmlDocument xmlDom = new XmlDocument();
            xmlDom.LoadXml(xml);
            xmlDom.DocumentElement.SetAttribute("UnsubscribeUrl", unsubsribeUrl);

            var lambdaUrl = await _secret.GetValue("ConstLambdaUrl");

            string imageUrl = $"https://{lambdaUrl}/api/email/image";
            string destination = $"https://{Constants.DOMAIN}/options/{imageTicker}";

            xmlDom.DocumentElement.SetAttribute("ChartTicker", imageTicker);
            xmlDom.DocumentElement.SetAttribute("ChartLink", destination);
            xmlDom.DocumentElement.SetAttribute("ChartImage", imageUrl);

            return xmlDom;
        }

        public async Task<string> GetScreenerHtml(XmlDocument xmlDom, string xslContent, bool useShortUrls)
        {
            string html = Utility.TransformXml(xmlDom.OuterXml, xslContent);

            if (useShortUrls)
            {
                html = await ShortenUrlsInHtml(html);
            }

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

        private async Task<string> ShortenUrlsInHtml(string html)
        {
            string pattern = @"https://([\w+?\.]+)+([a-zA-Z0-9~!@#\$%\^&\*\(\)_\-=\+\\\/\?\.\:\;\'\,]*)";
            Regex regx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regx.Matches(html);

            foreach (Match match in matches)
            {
                string originalUrl = match.Value;

                var chartDomain = await _secret.GetValue("ConstChartDomain");

                if (!originalUrl.Contains("subscribe.aspx", StringComparison.CurrentCultureIgnoreCase) &&
                    !originalUrl.Contains(chartDomain, StringComparison.CurrentCultureIgnoreCase))
                {
                    string newUrl = await _urlShort.Bitly(originalUrl);
                    html = html.Replace(originalUrl, newUrl);
                }
            }
            return html;
        }
        #endregion
    }
}
