using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Models.Schwab;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Text;

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
            ScwToken token = await Schwab_GetToken();
            return token.access_created_on_utc;
        }

        #region Controller
        public async Task<OptChn> FetchOptionChain(string ticker, DateTime maturity)
        {
            return await FetchOptionChain(ticker, maturity, true);
        }

        public async Task<OptChn> FetchOptionChain(string ticker, DateTime maturity, bool useNearestExpiration)
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
                if (records.Count > 0)
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

            string ticker = chain.Options[0].Ticker().ToUpper();
            if (ticker == "SPXW" || ticker == "$SPX") ticker = "SPX";

            string sql = @"
DECLARE @Id INT 

SELECT @Id = Id
FROM OptionChainJson   
WHERE Ticker = @ParmTicker

IF (@Id IS NULL)
BEGIN
    INSERT INTO OptionChainJson
    (Ticker, ModifiedOn, Content)
    VALUES
    (@ParmTicker, GetUTCDate(), @ParmContent)
END
ELSE
BEGIN
    UPDATE OptionChainJson
        SET ModifiedOn = GetUTCDate(), Content = @ParmContent
    WHERE Id = @Id
END
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
            int current = Utility.DateToYMD(DateTime.Now);
            if (allowEmpty)
            {
                int mint = chain.Options.FindAll(x => x.Mint() > current).Min(x => x.Mint());
                return _calculation.FilterOptionChain(chain, mint);
            }
            else
            {
                int mint = 0;
                var nonZero = chain.Options.FindAll(x => x.Mint() > current && x.oi > 0);
                var groups = nonZero.GroupBy(x => x.Mint());
                foreach (var group in groups)
                {
                    if (group.Count() > 1)
                    {
                        mint = group.Key;
                        break;
                    }
                }
                return _calculation.FilterOptionChain(chain, mint);
            }
        }
        private OptChn FilterOptionChain(OptChn chain)
        {
            if (chain.Options.Count == 0) return chain;
            int m = chain.Options.Min(x => x.Mint());
            return FilterOptionChain(chain, m);
        }

        private OptChn FilterOptionChain(OptChn chain, DateTime maturity)
        {
            int m = chain.Options.Min(x => x.Mint());
            if (maturity != DateTime.MinValue) m = Utility.DateToYMD(maturity);
            return FilterOptionChain(chain, m);
        }

        private OptChn FilterOptionChain(OptChn chain, int m)
        {
            chain.Options = chain.Options.FindAll(x => x.Mint() == m);
            return chain;
        }

        private SdlChn FilterSdlChn(SdlChn sc)
        {
            int m = sc.Straddles.Min(x => x.Mint());
            return FilterSdlChn(sc, m);
        }

        private SdlChn FilterSdlChn(SdlChn sc, int m)
        {
            sc.Straddles = sc.Straddles.FindAll(x => x.Mint() == m);
            return sc;
        }
        #endregion

        #region Generic
        public async Task<bool> IsMarketOpen(DateTime dt)
        {
            return await Schwab_IsMarketOpen(_awsContext, dt);
        }

        public async Task<OptChn> FetchOptions(string ticker)
        {
            return await Schwab_FetchOptions(_awsContext, ticker);
        }

        public async Task<List<Stock>> FetchStock(string tickers)
        {
            return await Schwab_FetchStock(_awsContext, tickers);
        }

        public async Task<OptChn> FetchOptionData(string ticker)
        {
            string statusMsg = string.Empty;

            // 1st attempt
            OptChn result = await FetchOptions(ticker);
            if (result.Options.Count == 0 && string.Compare(result.HttpStatusCode, "400") != 0)
            {
                // 2nd attempt
                await Task.Delay(250);
                result = await FetchOptions(ticker);
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

        private async Task<ScwToken> Schwab_Init()
        {
            var currentToken = await Schwab_GetToken();

            var token = await _schwab.UpdateToken(currentToken);
            string json = DBHelper.Serialize(token);
            if (!json.Equals(DBHelper.Serialize(currentToken)))
            {
                await _configuration.Set("SchwabTokens", json);
            }

            return token;
        }

        private async Task<ScwToken> Schwab_GetToken()
        {
            string json = await _configuration.Get("SchwabTokens");
            return DBHelper.Deserialize<ScwToken>(json);
        }

        public async Task<bool> Schwab_IsMarketOpen(AwsContext awsContext, DateTime dt)
        {
            var token = await Schwab_Init();
            return await _schwab.IsMarketOpen(token.access_token, dt);
        }

        public async Task<OptChn> Schwab_FetchOptions(AwsContext awsContext, string ticker)
        {
            var token = await Schwab_Init();

            string mappedTicker = MapTicker(ticker);

            try
            {
                var chain = await _schwab.GetOptions(token.access_token, mappedTicker);
                var oc = DBHelper.Deserialize<OptChn>(DBHelper.Serialize(chain));
                if (oc != null)
                {
                    oc.Source = "Schwab";
                    oc.HttpStatusCode = "200";
                }
                return oc;
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

        public async Task<List<ScwOptionCSV>> Schwab_FetchOptions_CSV(string ticker)
        {
            var token = await Schwab_Init();

            string mappedTicker = MapTicker(ticker);

            return await _schwab.GetOptionsCSV(token.access_token, mappedTicker);
        }

        public async Task<List<Stock>> Schwab_FetchStock(AwsContext awsContext, string tickers)
        {
            var token = await Schwab_Init();
            return await _schwab.GetStocks(token.access_token, tickers);
        }

        private string MapTicker(string ticker)
        {
            return ticker.ToUpper() switch
            {
                "SPX" => "$SPX",
                "SPXW" => "$SPX",
                _ => ticker
            };
        }

        public async Task<List<SchwabAccount>> Schwab_Account()
        {
            var token = await Schwab_Init();
            return await _schwab.GetAccounts(token.access_token);
        }
        public async Task<string> Schwab_Watchlist()
        {
            var token = await Schwab_Init();
            return await _schwab.CreateWatchlist(token.access_token, "W250930", new string[] { "appl", "orcl" });
        }
        #endregion

        #region miscellanous
        private DateTime ParseDateFromSymbol(string optionTicker)
        {
            string ymd = optionTicker.Substring(optionTicker.Length - 15, 6);
            string mdy = $"{ymd.Substring(2, 2)}/{ymd.Substring(4, 2)}/{ymd.Substring(0, 2)}";
            return Convert.ToDateTime(mdy);
        }
        private async Task Log(string subject, string body)
        {
            //await _logger.InfoAsync(subject, body);
        }
        #endregion
    }
}
