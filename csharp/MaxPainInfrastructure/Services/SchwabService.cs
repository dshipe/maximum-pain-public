using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Models.Schwab;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MaxPainInfrastructure.Services
{
    public enum GrantType
    {
        authorization_code,
        refresh_token
    }

    public class SchwabService : ISchwabService
    {
        // private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly HttpClient _httpClient = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            MaxConnectionsPerServer = 10
        })
        {
            Timeout = TimeSpan.FromMinutes(2)
        };


        private readonly ILoggerService _loggerSvc;
        private readonly ISecretService _secretSvc;

        private const string _rootUrl = "https://api.schwabapi.com";
        private const int _refreshTokenTimeout = 7 * 24 * 60 * 60; // 7 days translated to seconds
        private const int _accessTokenTimeout = 30 * 60; // 30 minutes translated to seconds

        public SchwabService(
            ILoggerService loggerSvc,
            ISecretService secretSvc
        )
        {
            //_httpClient.Timeout = TimeSpan.FromMinutes(5); // or your desire

            _loggerSvc = loggerSvc;
            _secretSvc = secretSvc;
        }

        #region Auth
        public async Task<string> GetSchwabLoginUrl(string callbackUrl)
        {
            string url = $"{_rootUrl}/v1/oauth/authorize";

            string appKey = await _secretSvc.GetValue("SchwabAppKey");
            string secret = await _secretSvc.GetValue("SchwabSecret");

            Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "client_id", appKey },
                    { "redirect_uri", callbackUrl }
                };
            var content = new FormUrlEncodedContent(data);
            var querystring = await content.ReadAsStringAsync();
            url = $"{url}?{querystring}";

            return url;
        }

        public async Task<ScwToken> GetRefreshToken(string code, string callbackUrl)
        {
            /*
             *     import base64
                headers = {'Authorization': f'Basic {base64.b64encode(bytes(f"{universe.credentials.appKey}:{universe.credentials.appSecret}", "utf-8")).decode("utf-8")}', 'Content-Type': 'application/x-www-form-urlencoded'}
                if grant_type == 'authorization_code': data = {'grant_type': 'authorization_code', 'code': code, 'redirect_uri': universe.credentials.callbackUrl} #gets access and refresh tokens using authorization code
                elif grant_type == 'refresh_token': data = {'grant_type': 'refresh_token', 'refresh_token': code} #refreshes the access token
                else:
                    universe.terminal.error("Invalid grant type")
                    return None
                universe.terminal.warning(headers)
                universe.terminal.warning(data)
                #return _ResponseHandler(requests.post('https://api.schwabapi.com/v1/oauth/token', headers=headers, data=data))
                retrun  = ""
            */

            string method = "GetRefreshToken";

            string grantType = "authorization_code";
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "grant_type", grantType },
                { "code", code },
                { "redirect_uri", callbackUrl }
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(data);

            string appKey = await _secretSvc.GetValue("SchwabAppKey");
            string secret = await _secretSvc.GetValue("SchwabSecret");
            string authHeader = $"{appKey}:{secret}";
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authHeader));
            AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Basic", base64);

            string url = $"{_rootUrl}/v1/oauth/token";
            method = $"{method} url={url} authHeader={base64} content={DBHelper.Serialize(data)}";

            string json = await PostWithAuthHeader(auth, url, content, method);

            var createdOn = DateTime.UtcNow;
            ScwToken token = DBHelper.Deserialize<ScwToken>(json);
            token.access_created_on_utc = createdOn;
            token.refresh_created_on_utc = createdOn;

            return token;
        }

        private async Task<ScwToken> GetAccessToken(ScwToken currentToken)
        {
            string method = "GetAccessToken";

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", currentToken.refresh_token }
            };

            string appKey = await _secretSvc.GetValue("SchwabAppKey");
            string secret = await _secretSvc.GetValue("SchwabSecret");
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appKey}:{secret}"));
            var auth = new AuthenticationHeaderValue("Basic", authHeader);
            var content = new FormUrlEncodedContent(data);

            var url = $"{_rootUrl}/v1/oauth/token";
            var json = await PostWithAuthHeader(auth, url, content, "GetAccessToken");

            var token = DBHelper.Deserialize<ScwToken>(json);
            token.access_created_on_utc = currentToken.access_created_on_utc;
            token.refresh_created_on_utc = DateTime.UtcNow;

            return token;
        }

        public async Task<ScwToken> UpdateToken(ScwToken token, bool force = false)
        {
            var diffInSeconds = (DateTime.UtcNow - token.refresh_created_on_utc).TotalSeconds;
            var remainingSeconds = _accessTokenTimeout - diffInSeconds;

            if (force || remainingSeconds <= 0)
            {
                token = await GetAccessToken(token);
                //token = await GetRefreshTokenOrig(token.refresh_token);
            }
            return token;
        }
        #endregion

        #region Options
        public async Task<ScwExpirationList> GetExpirations(string accessToken, string ticker)
        {
            var url = $"{_rootUrl}/marketdata/v1/expirationchain?symbol={Uri.EscapeDataString(ticker.ToUpper())}";
            var result = await GetWithAuth(accessToken, url, "Schwab Option");
            return DBHelper.Deserialize<ScwExpirationList>(result);
        }

        public async Task<OptChn> GetOptions(string accessToken, string ticker)
        {
            ScwOptChn chain = await GetOptionsRawStringParallel(accessToken, ticker);
            return MapOptions(chain);
        }

        public async Task<List<ScwOptionCSV>> GetOptionsCSV(string accessToken, string ticker)
        {
            ScwOptChn chain = await GetOptionsRawStringParallel(accessToken, ticker);
            return ParseJsonToObject(chain, ticker);
        }

        private async Task<ScwOptChn> GetOptionsRawStringParallel(string accessToken, string ticker)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            List<String> logList = new List<String>();
            logList.Add($"{timer.ElapsedMilliseconds} ms GetOptionsRawStringParallel method begin.  ticker=\"{ticker}\"");

            int years = 3;
            DateTime start = DateTime.Now;
            DateTime end = start.AddDays(365 * years);
            DateTime loop = start;

            int incrementDays = 365 * years;
            string upper = ticker.ToUpper();
            if (upper.Equals("SPX")) upper = "$SPX";

            List<string> daily = new List<string>() { "QQQ", "$SPX", "SPX", "SPXW", "SPY", "GLD" };
            if (daily.Contains(upper))
            {
                incrementDays = 5;
            }

            // construct list of dates    
            int counter = 0;
            List<DateModel> dates = new List<DateModel>();
            while (loop <= end)
            {
                dates.Add(new DateModel(counter, loop, loop.AddDays(incrementDays - 1)));

                counter++;
                loop = loop.AddDays(incrementDays);
                if (counter > 5 && incrementDays < 365) incrementDays = 20;
                if (counter > 8 && incrementDays < 365) incrementDays = 90;
                if (counter > 12 && incrementDays < 365) incrementDays = 365 * years;
            }

            // load the first Date
            DateModel dm = dates[0];
            // remove the first Date from the list
            dates.RemoveAt(0);

            // fetch the first Date
            ScwOptChn dstChain = await GetAndParseOptionsByDate(accessToken, upper, dm.Start, dm.End);

            // fetch JSON for each date
            await Parallel.ForEachAsync(dates, async (dm, cancellationToken) =>
            {
                ScwOptChn srcChain = await GetAndParseOptionsByDate(accessToken, upper, dm.Start, dm.End);
                logList.Add($"counter={dm.Index} ms={timer.ElapsedMilliseconds} ticker=\"{ticker}\" dm.Start=\"{dm.Start}\" array length=\"{dstChain.options.Length}\"");

                if (srcChain != null && srcChain.options != null && srcChain.options.Length != 0)
                {
                    dstChain.options = ConcatArrays(dstChain.options, srcChain.options);
                }
            });

            logList.Add($"{timer.ElapsedMilliseconds} ms method complete");
            timer.Stop();

            /*
            if (timer.ElapsedMilliseconds > 1)
            {
                string msg = String.Join("\r\n", logList.ToArray());
                //await _loggerSvc.InfoAsync($"Stopwatch ticker=\"{ticker}\" {timer.ElapsedMilliseconds} ms", msg);
                Utility.OpenInNotepad(msg);
            }
            */

            return dstChain;
        }

        private async Task<ScwOptChn> GetAndParseOptionsByDate(string accessToken, string ticker, DateTime start, DateTime end)
        {
            try
            {
                string json = await GetOptionsRawStringByDate(accessToken, ticker, start, end);
                return ParseOptions(json);
            }
            catch (Exception ex) when (ex.Message.Contains("TooBigBody"))
            {
                await _loggerSvc.InfoAsync($"TooBigBody: ticker=\"{ticker}\" start=\"{start:yyyy-MM-dd}\" end=\"{end:yyyy-MM-dd}\" — retrying with split range", ex.Message);
                var mid = start.AddDays((end - start).TotalDays / 2);
                ScwOptChn chain1 = await GetAndParseOptionsByDate(accessToken, ticker, start, mid);
                ScwOptChn chain2 = await GetAndParseOptionsByDate(accessToken, ticker, mid.AddDays(1), end);

                if (chain2?.options != null && chain2.options.Length > 0)
                    chain1.options = ConcatArrays(chain1.options, chain2.options);

                return chain1;
            }
        }

        private async Task<string> GetOptionsRawStringByDate(string accessToken, string ticker, DateTime start, DateTime end)
        {
            string url = $"{_rootUrl}/marketdata/v1/chains";

            Dictionary<string, string> allData = new Dictionary<string, string>
                {
                    { "symbol", ticker.ToUpper() },
                    { "contractType", string.Empty },
                    { "strikeCount", string.Empty },
                    { "includeUnderlyingQuotes", string.Empty },
                    { "strategy", string.Empty },
                    { "interval", string.Empty },
                    { "range", string.Empty },
                    { "fromDate", string.Empty },
                    { "toDate", string.Empty },
                    { "volatility", string.Empty },
                    { "underlyingPrice", string.Empty },
                    { "interestRate", string.Empty },
                    { "daysToExpiration", string.Empty },
                    { "expMonth", string.Empty },
                    { "optionType", string.Empty },
                    { "entitlement", string.Empty }
                };

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "symbol", ticker.ToUpper()  },
                { "fromDate", start.ToString("yyyy-MM-dd") },
                { "toDate", end.ToString("yyyy-MM-dd") }
            };

            var content = new FormUrlEncodedContent(data);
            var querystring = await content.ReadAsStringAsync();
            url = $"{url}?{querystring}";

            string result = await GetWithAuth(accessToken, url, "Schwab Option");

            if (result.IndexOf("NaN") != -1) result = result.Replace("NaN", "0");
            if (result.IndexOf("Infinity") != -1) result = result.Replace("Infinity", "0");
            if (result.IndexOf("-Infinity") != -1) result = result.Replace("-Infinity", "0");

            return result;
        }

        /*
        public string TransformOptions(string json)
        {
            //var transform = Regex.Replace(json, @"""(\d{4})-(\d{2})-(\d{2}):(\d+)"":", @"""Exp"":");
            //transform = Regex.Replace(transform, @"""([.\d]+)"":", @"""Stk"":");

            var transform = Regex.Replace(json, @"""(\d{4})-(\d{2})-(\d{2}):(\d+)"":", string.Empty);
            transform = Regex.Replace(transform, @"""([.\d]+)"":", string.Empty);

            transform = Regex.Replace(transform, @"{(\s+){", string.Empty);
            transform = Regex.Replace(transform, @"\],(\s+)\[", ",");
            transform = Regex.Replace(transform, @"}(\s+)\](\s+)},(\s+){(\s+)\[(\s+){", "},{");

            return transform;
        }
        */

        public ScwOptChn ParseOptionsOrig(string json)
        {
            var root = (JContainer)JToken.Parse(json);
            var query = root
                // Recursively descend the JSON hierarchy
                .DescendantsAndSelf()
                // Select all properties named descendant
                .OfType<JProperty>()
                .Where(p => p.Name.Contains("."))
                // Select their value
                .Select(p => p.Value)
                // And filter for those that are arrays.
                .OfType<JArray>();

            List<ScwOpt> options = new List<ScwOpt>();
            foreach (var item in query)
            {
                foreach (var child in item)
                {
                    options.Add(DBHelper.Deserialize<ScwOpt>(child.ToString()));
                }
            }

            return new ScwOptChn()
            {
                stock = root["symbol"].ToString(),
                price = Convert.ToSingle(root["underlyingPrice"].ToString()),
                interestRate = Convert.ToSingle(root["interestRate"].ToString()),
                volatility = Convert.ToSingle(root["volatility"].ToString()),
                options = options.ToArray()
            };
        }

        public ScwOptChn ParseOptionsJObject(string json)
        {
            var root = (JContainer)JToken.Parse(json);
            var query = root
                // Recursively descend the JSON hierarchy
                .DescendantsAndSelf()
                // Select all properties named descendant
                .OfType<JProperty>()
                .Where(p => p.Name.Contains("."))
                // Select their value
                .Select(p => p.Value)
                // And filter for those that are arrays.
                .OfType<JArray>();

            string internalJson = string.Empty;
            foreach (var item in query)
            {
                internalJson = Newtonsoft.Json.JsonConvert.SerializeObject(item);
            }

            if (string.IsNullOrEmpty(internalJson)) return null;

            return new ScwOptChn()
            {
                stock = root["symbol"].ToString(),
                price = Convert.ToSingle(root["underlyingPrice"].ToString()),
                interestRate = Convert.ToSingle(root["interestRate"].ToString()),
                volatility = Convert.ToSingle(root["volatility"].ToString()),
                options = DBHelper.Deserialize<ScwOpt[]>(internalJson)
            };
        }

        public ScwOptChn ParseOptions(string json)
        {
            var rawChain = DBHelper.Deserialize<Models.Schwab.Raw.OptionChain>(json);

            var callOptions = rawChain.CallExpDateMap?.SelectMany(m => m.Value).SelectMany(s => s.Value) ?? Enumerable.Empty<Models.Schwab.Raw.Option>();
            var putOptions = rawChain.PutExpDateMap?.SelectMany(m => m.Value).SelectMany(s => s.Value) ?? Enumerable.Empty<Models.Schwab.Raw.Option>();
            var allOptions = callOptions.Concat(putOptions).ToArray();

            return new ScwOptChn()
            {
                stock = rawChain.Symbol,
                price = Convert.ToSingle(rawChain.UnderlyingPrice),
                interestRate = Convert.ToSingle(rawChain.InterestRate),
                volatility = Convert.ToSingle(rawChain.Volatility),
                options = DBHelper.Deserialize<ScwOpt[]>(DBHelper.Serialize(allOptions))
            };
        }

        public ScwOptionSymbol ParseSymbol(string content)
        {
            // 123456789 12345678 12
            // AAPL  240503C00100000

            ScwOptionSymbol os = new ScwOptionSymbol();

            string[] array = content.Split('_');

            // get the ticker
            os.underlying = content.Substring(0, 5).Trim();

            string y = content.Substring(6, 2);
            string m = content.Substring(8, 2);
            string d = content.Substring(10, 2);
            os.maturity = Convert.ToDateTime(string.Format("{0}/{1}/{2}", m, d, y));

            os.optionType = content.Substring(12, 1);

            os.price = Convert.ToDecimal(content.Substring(13));
            os.price = os.price / 1000;
            return os;
        }

        public OptChn MapOptions(ScwOptChn chain)
        {
            OptChn result = new OptChn();
            result.Stock = chain.stock;
            result.StockPrice = (decimal)chain.price;
            result.InterestRate = chain.interestRate;
            result.Volatility = chain.volatility;

            foreach (ScwOpt option in chain.options.Distinct())
            {
                Opt x = new Opt();

                ScwOptionSymbol os = ParseSymbol(option.symbol);
                x.ot = os.ticker;

                x.b = (decimal)option.bid;
                x.a = (decimal)option.ask;
                x.p = (decimal)option.last;
                x.c = option.netChange;
                x.oi = option.openInterest;
                x.v = option.totalVolume;
                x.iv = option.volatility;

                x.de = option.delta;
                x.ga = option.gamma;
                x.th = option.theta;
                x.ve = option.vega;
                x.rh = option.rho;

                result.Options.Add(x);
            }

            return result;
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
        #endregion

        #region stocks
        public async Task<List<Stock>> GetStocks(string accessToken, string tickers)
        {
            var url = $"{_rootUrl}/marketdata/v1/quotes?symbols={Uri.EscapeDataString(tickers.ToUpper())}";
            var result = await GetWithAuth(accessToken, url, "Schwab Stock");
            var stockQuotes = ParseStocks(result);
            return MapStocks(stockQuotes);
        }

        public List<ScwStockQuote> ParseStocks(string json)
        {
            var stockQuotes = new List<ScwStockQuote>();

            var root = (JContainer)JToken.Parse(json);
            foreach (var child in root.Children())
            {
                if (child.ToString().IndexOf("errors") == -1)
                {
                    var stock = DBHelper.Deserialize<ScwStockQuote>(child.Children().First().ToString());
                    stockQuotes.Add(stock);
                }
            }

            return stockQuotes;
        }

        public List<Stock> MapStocks(List<ScwStockQuote> quotes)
        {
            List<Stock> result = new List<Stock>();
            foreach (var quote in quotes)
            {
                var stock = DBHelper.Deserialize<Stock>(DBHelper.Serialize(quote));
                result.Add(stock);
            }

            return result;
        }

        #endregion

        #region Market
        public async Task<bool> IsMarketOpen(string accessToken, DateTime dte)
        {
            var url = $"{_rootUrl}/marketdata/v1/markets?markets=OPTION&date={dte:yyyy-MM-dd}";
            var result = await GetWithAuth(accessToken, url, "Schwab Market");
            var market = DBHelper.Deserialize<ScwMarket>(result);

            return market?.option?.EQO?.isOpen == true || market?.option?.IND?.isOpen == true;
        }
        #endregion

        #region CSV
        private List<ScwOptionCSV> ParseJsonToObject(ScwOptChn chain, string ticker)
        {
            List<ScwOptionCSV> result = new List<ScwOptionCSV>();

            foreach (var child in chain.options)
            {
                string childJson = DBHelper.Serialize(child);
                ScwOptionCSV option = DBHelper.Deserialize<ScwOptionCSV>(childJson);

                ScwOptionSymbol os = ParseSymbol(option.symbol);
                option.ticker = os.ticker;
                option.stock = os.underlying;
                option.maturity = os.maturityStr;
                option.maturityYMD = os.maturityYMD;
                option.type = os.optionType;
                option.strike = os.price;

                option.quoteTimeUTC = Code.Utility.UnixTimestampToDateTime(option.quoteTimeInLong).ToString();
                option.tradeTimeUTC = Code.Utility.UnixTimestampToDateTime(option.tradeTimeInLong).ToString();

                if (ticker.Length == 0 || string.Compare(ticker, os.underlying, true) == 0)
                {
                    result.Add(option);
                }
            }
            return result;
        }
        #endregion

        #region API
        private async Task<string> GetWithAuth(string accessToken, string url, string method)
        {
            string result = string.Empty;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using (HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    if (response.Content.Headers.ContentType.MediaType == "application/gzip" || response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        result = ReadGZip(bytes);
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    string ticker = string.Empty;
                    if (url.Contains("="))
                    {
                        ticker = url.Split('=')[1];
                        if (ticker.Contains("&"))
                        {
                            ticker = ticker.Split('&')[0];
                        }
                    }

                    string statusCodeNum = ((int)response.StatusCode).ToString();
                    string errorMsg = $"{method}: ticker=\"{ticker}\" response.StatusCode=\"{statusCodeNum} {response.StatusCode}\" response.Content=\"{result}\"";
                    Exception ex = new Exception(errorMsg);

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        string argErrorMsg = $"FinanceDataConsumer invalid ticker \"{ticker}\"";
                        ArgumentException argEx = new ArgumentException(argErrorMsg, statusCodeNum.ToString(), ex);
                        throw argEx;
                    }

                    throw ex;
                }
            }

            return result;
        }

        private async Task<string> PostWithAuth(string accessToken, string url, ByteArrayContent content, string method)
        {
            string result = string.Empty;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using (HttpResponseMessage response = await _httpClient.PostAsync(url, content).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    if (response.Content.Headers.ContentType.MediaType == "application/gzip" || response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        result = ReadGZip(bytes);
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    string errorMsg = $"{method}:  {result}";
                    throw new Exception(errorMsg);
                }
            }

            return result;
        }

        private async Task<string> PutWithAuth(string accessToken, string url, string json, string method)
        {
            var content = new StringContent(json);

            string result = string.Empty;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using (HttpResponseMessage response = await _httpClient.PutAsync(url, content).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    if (response.Content.Headers.ContentType.MediaType == "application/gzip" || response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        result = ReadGZip(bytes);
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    string errorMsg = $"{method}:  {result}";
                    throw new Exception(errorMsg);
                }
            }

            return result;
        }

        private async Task<string> PostWithAuthHeader(AuthenticationHeaderValue authHeader, string url, FormUrlEncodedContent content, string method)
        {
            string result = string.Empty;

            //client.DefaultRequestHeaders.Authorization = authHeader;

            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(url);
            req.Headers.Authorization = authHeader;
            req.Content = content;

            using (HttpResponseMessage response = await _httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    if (response.Content.Headers.ContentType.MediaType == "application/gzip" || response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        result = ReadGZip(bytes);
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    string errorMsg = $"{method}:  {result}";
                    throw new Exception(errorMsg);
                }
            }

            return result;
        }

        private string ReadGZip(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress, true))
            using (StreamReader unzip = new StreamReader(zip))
                return unzip.ReadToEnd();
        }
        #endregion

        #region Watchlist
        public async Task<SchwabAccount> GetTradingAccount(string accessToken)
        {
            List<SchwabAccount> list = await GetAccounts(accessToken);
            var account = list.FirstOrDefault(a => a.AccountNumber.EndsWith("2788"));
            if (account == null) account = list.FirstOrDefault(a => a.AccountNumber.EndsWith("9418"));
            return account;
        }

        public async Task<List<SchwabAccount>> GetAccounts(string accessToken)
        {
            //string url = $"{_rootUrl}/v1/accounts";
            string url = $"{_rootUrl}/trader/v1/accounts/accountNumbers";
            string json = await GetWithAuth(accessToken, url, "GetAccounts");
            return DBHelper.Deserialize<List<SchwabAccount>>(json);
        }

        public async Task<string> GetAccountInfo(string accessToken)
        {
            string url = "https://ausgateway.schwab.com/api/is.TradeOrderManagementWeb/v1/TradeOrderManagementWebPort/customer/accounts";
            var resp = await GetWithAuth(accessToken, url, "");
            return resp;
        }

        public async Task<string> GetOrders(string accessToken, string accountId, bool allOrders = false)
        {
            string url = $"{_rootUrl}/trader/v1/accounts/{accountId}/orders";
            if (allOrders)
            {
                // date format 2024-12-01T00:00:00.000Z
                url = $"{_rootUrl}/trader/v1/orders?fromEnteredTime=2024-12-01T00%3A00%3A00.000Z&toEnteredTime=2024-12-07T00%3A00%3A00.000Z";
            }

            var json = await GetWithAuth(accessToken, url, "");
            return json;

            //var url = $"{_rootUrl}/accounts/watchlists";
            //var resp = await GetWithAuth(accessToken, url, "");
            //return resp;
        }

        public async Task<string> GetWatchList(string accessToken, string accountId)
        {
            var tradingAccount = await GetTradingAccount(accessToken);
            var url = $"{_rootUrl}/accounts/{tradingAccount.HashValue}/watchlists";
            var json = await GetWithAuth(accessToken, url, "");
            return json;
        }

        public async Task<string> CreateWatchListX(string accessToken, string accountId)
        {
            // https://www.reddit.com/r/tdameritrade/comments/nylj0t/how_to_create_a_watchlist_with_python_using_the/

            // https://www.reddit.com/r/Schwab/comments/1c2ioe1/the_unofficial_guide_to_charles_schwabs_trader/


            //Create a watchlist named "Tech Stocks"
            string name = $"Daily_{DateTime.UtcNow.ToString("yyyy_MM_dd")}";
            List<string> tickers = new List<string>() { "AAPL", "MSFT", "AMZN" };

            string list = "AAPL";

            Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "name", name },
                    { "watchlistItems", list}
                };
            string dataJson = DBHelper.Serialize(data);
            var content = new FormUrlEncodedContent(data);

            //string url = $"{_rootUrl}/trader/v1/accounts/{accountId}/create_watchlist";
            //string url = $"{_rootUrl}/trader/v1/accounts/{accountId}/watchlists";
            string url = $"{_rootUrl}/v1/accounts/{accountId}/watchlists";

            //endpoint = '/accounts/{}/watchlists'.format(account)
            //return self._make_request(method = 'put', endpoint = endpoint, mode = 'json', data = payload)

            string json = await PutWithAuth(accessToken, url, dataJson, "Create Watchlist");
            //string json = await PostWithAuth(accessToken, url, content, "Create Watchlist");
            return json;

        }

        public async Task<string> CreateWatchlist(string accessToken, string watchlistName, string[] symbols)
        {
            var tradingAccount = await GetTradingAccount(accessToken);
            var endpoint = $"{_rootUrl}/v1/accounts/{tradingAccount.HashValue}/watchlists";

            // Create the payload for the POST request
            var list = new List<WatchlistTicker>();
            foreach (var symbol in symbols)
            {
                list.Add(new WatchlistTicker { Symbol = symbol, InstrumentType = "EQUITY" });
            }

            var payload = new
            {
                name = watchlistName,
                watchlistItems = list.ToArray()
            };

            var jsonPayload = DBHelper.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var json = await PostWithAuth(accessToken, endpoint, content, "");
            return json;
        }
        #endregion
    }

    public class DateModel
    {
        public int Index { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateModel(int index, DateTime start, DateTime end)
        {
            this.Index = index;
            this.Start = start;
            this.End = end;
        }
    }

    public class WatchlistTicker
    {
        public string? Symbol { get; set; }
        public string? InstrumentType { get; set; }
    }

    public class SchwabAccount
    {
        public string? AccountNumber { get; set; }
        public string? HashValue { get; set; }
    }
}
