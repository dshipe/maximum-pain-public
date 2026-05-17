using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Models.Schwab;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;
using System.Diagnostics;


namespace MaxPainInfrastructure.Services
{
    public class FinDataService : IFinDataService
    {
        private readonly AwsContext _awsContext;
        private readonly ILoggerService _logger;
        private readonly IConfigurationService _configuration;
        private readonly ICalculationService _calculation;
        private readonly ISchwabService _schwab;
        private readonly ISecretService _secret;

        private ScwToken? _cachedToken;
        private DateTime _tokenCacheExpiry;
        private const int _tokenExpiryMinutes = 10;

        private static readonly HashSet<string> _loggedTickers = new(StringComparer.OrdinalIgnoreCase)
        {
            "$SPX", "SPX", "SPXW", "SPY"
        };
                
        public FinDataService(
            AwsContext awsContext,
            IConfigurationService configurationService,
            ILoggerService loggerService,
            ICalculationService calculationService,
            ISchwabService schwabService,
            ISecretService secretService
        )
        {
            _awsContext = awsContext;
            _configuration = configurationService;
            _logger = loggerService;
            _calculation = calculationService;
            _secret = secretService;
            _schwab = schwabService;
        }

        public async Task<DateTime> GetCreatedOn()
        {
            ScwToken token = await Schwab_GetToken(false);
            return token.access_created_on_utc;
        }

        #region Controller
        public async Task<OptChn> FetchOptionChain(string ticker, DateTime maturity)
        {
            return await FetchOptionChain(ticker, maturity, true);
        }

        public async Task<OptChn> FetchOptionChain(string ticker, DateTime maturity, bool useNearestExpiration)
        {
            return await FetchOptionChainWithoutTimer(ticker, maturity, useNearestExpiration);
        }

        private async Task<OptChn> FetchOptionChainWithoutTimer(string ticker, DateTime maturity, bool useNearestExpiration)
        {
            // Normalize the ticker once so both the cache lookup and any subsequent save
            // operate on the same canonical key (e.g. "spx", "SPX", "SPXW", "$SPX" -> "SPX").
            string canonicalTicker = NormalizeTicker(ticker);

            DateTime staleThreshold = DateTime.UtcNow.AddMinutes(-Constants.STALE_MINUTES);

            // Cheap probe: pull only ModifiedOn first. Avoids transferring/deserializing the
            // (potentially multi-MB) Content blob when we're about to refresh it anyway.
            string probeSql = "SELECT TOP 1 ModifiedOn FROM OptionChainJson WITH(NOLOCK) WHERE Ticker=@ParmTicker";
            var probeParams = new List<SqlParameter>
            {
                DBHelper.CreateParm("ParmTicker", SqlDbType.VarChar, canonicalTicker)
            };
            string probeJson = await _awsContext.FetchJson(probeSql, probeParams, 60);

            DateTime modifiedOn = DateTime.MinValue;
            if (probeJson.Length > 10)
            {
                var probeRecords = DBHelper.Deserialize<List<OptionChainJson>>(probeJson);
                if (probeRecords?.Count > 0 && probeRecords[0]?.ModifiedOn != null)
                {
                    modifiedOn = probeRecords[0].ModifiedOn!.Value;
                }
            }

            OptChn? chain = null;
            bool isStale = modifiedOn < staleThreshold;
            if (!isStale)
            {
                // Fresh enough: now pull the Content payload and deserialize once.
                string contentSql = "SELECT TOP 1 Content FROM OptionChainJson WITH(NOLOCK) WHERE Ticker=@ParmTicker";
                var contentParams = new List<SqlParameter>
                {
                    DBHelper.CreateParm("ParmTicker", SqlDbType.VarChar, canonicalTicker)
                };
                string contentJson = await _awsContext.FetchJson(contentSql, contentParams, 60);
                if (contentJson.Length > 10)
                {
                    var contentRecords = DBHelper.Deserialize<List<OptionChainJson>>(contentJson);
                    if (contentRecords?.Count > 0 && contentRecords[0] != null)
                    {
                        chain = DBHelper.Deserialize<OptChn>(contentRecords[0].Content);
                    }
                }

                // If the Content was missing/empty we still need to refresh.
                isStale = chain == null || chain.Options.Count == 0;
            }

            if (isStale)
            {
                // scrape from the web
                chain = await FetchOptionData(ticker);

                // save to the database
                await SaveToDatabase(chain);
            }

            if (_loggedTickers.Contains(canonicalTicker))
            {
                //await _logger.InfoAsync($"FinDataService.cs FetchOptionChain ticker=\"{ticker}\"", "");
            }

            // filter by maturity
            if (maturity != DateTime.MinValue) return FilterOptionChain(chain!, Utility.DateToYMD(maturity));
            if (useNearestExpiration) return FilterOptionChain(chain!);
            return chain!;
        }

        private async Task<OptChn> FetchOptionChainWithTimer(string ticker, DateTime maturity, bool useNearestExpiration)
        {
            Stopwatch timer = Stopwatch.StartNew();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{timer.ElapsedMilliseconds} ms: FinDataService.cs FetchOptionChain method Begin");

            string upper = ticker.ToUpper();
            sb.AppendLine($"{timer.ElapsedMilliseconds} ms: ticker=\"{upper}\" maturity=\"{maturity}\" useNearestExpiration=\"{useNearestExpiration}\"");

            // fetch data from DB            
            sb.AppendLine($"{timer.ElapsedMilliseconds} ms: fetch data from DB");
            string sql = "SELECT Id, Ticker, ModifiedOn, Content FROM OptionChainJson WITH(NOLOCK) WHERE Ticker=@ParmTicker";
            var parameters = new List<SqlParameter>
            {
                DBHelper.CreateParm("ParmTicker", SqlDbType.VarChar, ticker)
            };
            string json = await _awsContext.FetchJson(sql, parameters, 60);

            OptChn? chain = null;
            DateTime modifiedOn = DateTime.MinValue;
            if (json.Length > 10)
            {
                var records = DBHelper.Deserialize<List<OptionChainJson>>(json);
                if (records?.Count > 0 && records[0] != null)
                {
                    chain = DBHelper.Deserialize<OptChn>(records[0].Content);
                    modifiedOn = records[0].ModifiedOn.Value;
                }
            }

            // if there are no quotes or the data is stale then fetch from the web
            bool isStale = chain == null || chain.Options.Count == 0 || modifiedOn < DateTime.UtcNow.AddMinutes(-Constants.STALE_MINUTES);
            sb.AppendLine($"{timer.ElapsedMilliseconds} ms: check if quotes are stale.  isStale=\"{isStale}\" minutes=\"{Constants.STALE_MINUTES}\" stale=\"{DateTime.UtcNow.AddMinutes(-Constants.STALE_MINUTES)}\" modifiedOn=\"{modifiedOn}\"");
            if (isStale)
            {
                // scrape from the web
                sb.AppendLine($"{timer.ElapsedMilliseconds} ms: fetch from the web");
                chain = await FetchOptionData(ticker);

                // save to the database using EF and OptionChainJson
                sb.AppendLine($"{timer.ElapsedMilliseconds} ms: save to the database");
                await SaveToDatabase(chain);
            }

            sb.AppendLine($"{timer.ElapsedMilliseconds} ms: method completed");
            List<string> loggedTickers = new List<string> { "$SPX", "SPX", "SPXW", "SPY" };
            if (loggedTickers.Contains(upper))
            {
                //await _logger.InfoAsync($"{timer.ElapsedMilliseconds} ms: FinDataService.cs FetchOptionChain ticker=\"{ticker}\"", sb.ToString());
                //Utility.OpenInNotepad(sb.ToString());
            }
            timer.Stop();

            // filter by maturity
            if (maturity != DateTime.MinValue) return FilterOptionChain(chain, Utility.DateToYMD(maturity));
            if (useNearestExpiration) return FilterOptionChain(chain);
            return chain;
        }

        private async Task<bool> SaveToDatabase(OptChn chain)
        {
            if (chain == null || chain.Options.Count == 0) return false;

            string ticker = NormalizeTicker(chain.Options[0].Ticker());

            string sql = @"
MERGE OptionChainJson AS target
USING (SELECT @ParmTicker AS Ticker) AS source
ON target.Ticker = source.Ticker
WHEN MATCHED THEN
    UPDATE SET ModifiedOn = GETUTCDATE(), Content = @ParmContent
WHEN NOT MATCHED THEN
    INSERT (Ticker, ModifiedOn, Content)
    VALUES (@ParmTicker, GETUTCDATE(), @ParmContent);
            ";

            var parameters = new List<SqlParameter>
            {
                DBHelper.CreateParm("ParmTicker", SqlDbType.VarChar, ticker),
                DBHelper.CreateParm("ParmContent", SqlDbType.Text, DBHelper.Serialize(chain))
            };

            await _awsContext.Execute(sql, parameters, 60);

            return true;
        }

        private async Task<bool> SaveToDatabaseEF(OptChn chain, long databaseIndexId, bool hasDatabaseRecords)
        {
            if (chain == null || chain.Options.Count == 0) return false;

            if (hasDatabaseRecords)
            {
                // the database has data for this Ticker
                // update the record
                var nosql = await _awsContext.OptionChainJson.FindAsync(databaseIndexId);
                if (nosql != null)
                {
                    nosql.Content = DBHelper.Serialize(chain);
                    nosql.ModifiedOn = DateTime.UtcNow;

                    _awsContext.Entry(nosql).State = EntityState.Modified;
                    await _awsContext.SaveChangesAsync();
                }
            }
            else
            {
                // the database does not have data for this Ticker
                // add the first time
                var nosql = new OptionChainJson
                {
                    Id = 0,
                    Ticker = chain.Options[0].Ticker(),
                    Content = DBHelper.Serialize(chain),
                    ModifiedOn = DateTime.UtcNow
                };

                _awsContext.OptionChainJson.Add(nosql);
                _awsContext.Entry(nosql).State = EntityState.Added;
                await _awsContext.SaveChangesAsync();
            }
            return true;
        }
        #endregion

        #region filter
        private OptChn FilterOptionChainMaturity(OptChn chain)
        {
            DateTime current = DateTime.Now;
            DateTime future = current.AddMonths(6);
            if (current.AddMonths(6) > new DateTime(current.Year + 1, 2, 1)) future = new DateTime(current.Year + 1, 2, 1);
            int next = Utility.DateToYMD(future);

            chain.Options = chain.Options.FindAll(x => x.Mint() < next);
            return chain;
        }

        private OptChn FilterOptionChainFutureOnly(OptChn chain, bool allowEmpty = true)
        {
            var current = Utility.DateToYMD(DateTime.Now);
            if (allowEmpty)
            {
                var mint = chain.Options.Where(x => x.Mint() > current).Min(x => x.Mint());
                return _calculation.FilterOptionChain(chain, mint);
            }
            else
            {
                var mint = chain.Options
                    .Where(x => x.Mint() > current && x.oi > 0)
                    .GroupBy(x => x.Mint())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .FirstOrDefault();
                return _calculation.FilterOptionChain(chain, mint);
            }
        }
        private OptChn FilterOptionChain(OptChn chain)
        {
            if (chain.Options.Count == 0) return chain;
            int m = chain.Options.Min(x => x.Mint());
            chain.Options = chain.Options.Where(x => x.Mint() == m).ToList();
            return chain;
        }

        private OptChn FilterOptionChain(OptChn chain, DateTime maturity)
        {
            int m = maturity != DateTime.MinValue ? Utility.DateToYMD(maturity) : chain.Options.Min(x => x.Mint());
            chain.Options = chain.Options.Where(x => x.Mint() == m).ToList();
            return chain;
        }

        private OptChn FilterOptionChain(OptChn chain, int m)
        {
            chain.Options = chain.Options.Where(x => x.Mint() == m).ToList();
            return chain;
        }

        private SdlChn FilterSdlChn(SdlChn sc)
        {
            var m = sc.Straddles.Min(x => x.Mint());
            return FilterSdlChn(sc, m);
        }

        private SdlChn FilterSdlChn(SdlChn sc, int m)
        {
            sc.Straddles = sc.Straddles.Where(x => x.Mint() == m).ToList();
            return sc;
        }
        #endregion

        #region Generic
        public async Task<bool> IsMarketOpen(DateTime dt)
        {
            return await Schwab_IsMarketOpen(_awsContext, dt);
        }

        public async Task<OptChn> FetchOptions(string ticker, bool useCachedToken = false)
        {
            return await Schwab_FetchOptions(_awsContext, ticker, useCachedToken);
        }

        public async Task<List<Stock>> FetchStock(string tickers)
        {
            return await Schwab_FetchStock(_awsContext, tickers);
        }

        public async Task<OptChn> FetchOptionData(string ticker)
        {
            OptChn result = await FetchOptions(ticker);
            if (result.Options.Count == 0 && !string.Equals(result.HttpStatusCode, "400", StringComparison.Ordinal))
            {
                await Task.Delay(250);
                result = await FetchOptions(ticker, useCachedToken: true);
            }
            return result;
        }

        public async Task<List<ScwOptionCSV>> FetchOptionCSV(string ticker)
        {
            return await Schwab_FetchOptions_CSV(ticker);
        }
        #endregion

        #region Schwab
        private async Task<bool> UseSchwab()
        {
            return true;
            //return Convert.ToBoolean(await _configuration.Get("UseSchwab"));
        }

        public async Task<ScwToken> Schwab_Init(bool useCachedToken, bool force = false)
        {
            var currentToken = await Schwab_GetToken(useCachedToken);

            var token = await _schwab.UpdateToken(currentToken, force);

            // Fast path: if UpdateToken returned the same instance (no refresh happened),
            // skip the double-serialize equality check and the config write entirely.
            if (ReferenceEquals(token, currentToken)) return token;

            // Structural comparison on the fields that actually change on refresh
            // avoids serializing both tokens just to compare strings.
            if (!TokensEqual(token, currentToken))
            {
                await _configuration.Set("SchwabTokens", DBHelper.Serialize(token));
                _cachedToken = token;
                _tokenCacheExpiry = DateTime.UtcNow.AddMinutes(_tokenExpiryMinutes);
            }

            return token;
        }

        private static bool TokensEqual(ScwToken? a, ScwToken? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return string.Equals(a.access_token, b.access_token, StringComparison.Ordinal)
                && string.Equals(a.refresh_token, b.refresh_token, StringComparison.Ordinal)
                && a.access_created_on_utc == b.access_created_on_utc
                && a.refresh_created_on_utc == b.refresh_created_on_utc;
        }

        private async Task<ScwToken> Schwab_GetToken(bool useCachedToken)
        {
            if (useCachedToken && _cachedToken != null && DateTime.UtcNow < _tokenCacheExpiry)
            {
                return _cachedToken;
            }

            string json = await _configuration.Get("SchwabTokens");
            var token = DBHelper.Deserialize<ScwToken>(json);

            _cachedToken = token;
            _tokenCacheExpiry = DateTime.UtcNow.AddMinutes(_tokenExpiryMinutes);

            return token;
        }

        public async Task<bool> Schwab_IsMarketOpen(AwsContext awsContext, DateTime dt, bool useCachedToken = false)
        {
            var token = await Schwab_Init(useCachedToken);
            return await _schwab.IsMarketOpen(token.access_token, dt);
        }

        public async Task<OptChn> Schwab_FetchOptions(AwsContext awsContext, string ticker, bool useCachedToken = false)
        {
            var token = await Schwab_Init(useCachedToken);

            string mappedTicker = MapTicker(ticker);

            try
            {
                var chain = await _schwab.GetOptions(token.access_token, mappedTicker);
                if (chain != null)
                {
                    chain.Source = "Schwab";
                    chain.HttpStatusCode = "200";
                }
                return chain;
            }
            catch (ArgumentException argEx)
            {
                //await _logger.InfoAsync(argEx.Message, argEx.InnerException.ToString());
                string? httpStatusCode = argEx.ParamName;
                var oc = new OptChn { HttpStatusCode = httpStatusCode };
                return oc;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ScwOptionCSV>> Schwab_FetchOptions_CSV(string ticker, bool useCachedToken = false)
        {
            var token = await Schwab_Init(useCachedToken);

            string mappedTicker = MapTicker(ticker);

            return await _schwab.GetOptionsCSV(token.access_token, mappedTicker);
        }

        public async Task<List<Stock>> Schwab_FetchStock(AwsContext awsContext, string tickers, bool useCachedToken = false)
        {
            var token = await Schwab_Init(useCachedToken);
            return await _schwab.GetStocks(token.access_token, tickers);
        }

        private static string MapTicker(string ticker) => ticker.ToUpper() switch
        {
            "SPX" or "SPXW" => "$SPX",
            _ => ticker
        };

        // Canonical storage key used for the OptionChainJson cache table.
        // Keeps lookup/save in sync so case- and alias-variants share a single row.
        private static string NormalizeTicker(string ticker)
        {
            if (string.IsNullOrEmpty(ticker)) return ticker;
            string upper = ticker.ToUpperInvariant();
            return upper switch
            {
                "SPXW" or "$SPX" => "SPX",
                _ => upper
            };
        }

        public async Task<List<SchwabAccount>> Schwab_Account(bool useCachedToken = false)
        {
            var token = await Schwab_Init(useCachedToken);
            return await _schwab.GetAccounts(token.access_token);
        }
        public async Task<string> Schwab_Watchlist(bool useCachedToken = false)
        {
            var token = await Schwab_Init(useCachedToken);
            return await _schwab.CreateWatchlist(token.access_token, "W250930", new string[] { "appl", "orcl" });
        }
        #endregion
    }
}
