using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Utility = MaxPainInfrastructure.Code.Utility;

namespace MaxPainInfrastructure.Services
{
    public class FinImportService : IFinImportService
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ILoggerService _logger;
        private readonly IConfigurationService _configuration;
        private readonly ICalculationService _calculation;
        private readonly IEmailService _email;
        private readonly IFinDataService _finData;
        private readonly IHistoryService _history;
        private readonly ISecretService _secret;
        private readonly ILogger<FinImportService> _log;

        private readonly List<string> _logs = new();
        private List<StockTicker> _tickers = new();

        public bool IsDebug { get; set; }
        public string TickersCSV { get; set; }
        public bool UseMessage { get; set; }
        public bool IsMarketOpen { get; set; }
        public bool IsMorning { get; set; }
        public bool IsWeekend { get; set; }
        public DateTime MarketDate { get; set; }
        public DateTime EST { get; set; }
        public DateTime UTC { get; set; }

        public FinImportService(
            AwsContext awsContext,
            HomeContext homeContext,
            ILoggerService loggerService,
            IConfigurationService configurationService,
            ICalculationService _calculationService,
            IEmailService emailService,
            IFinDataService finDataService,
            IHistoryService historyService,
            ISecretService secretService,
            ILogger<FinImportService> logger
        )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _logger = loggerService;
            _configuration = configurationService;
            _calculation = _calculationService;
            _finData = finDataService;
            _email = emailService;
            _history = historyService;
            _secret = secretService;
            _log = logger;

            this.UTC = DateTime.UtcNow;
            this.EST = Utility.GMTToEST(this.UTC);
        }


        public string GetTickersCSV(List<StockTicker> tickers)
        {
            return tickers.Count == 0 ? string.Empty : string.Join(',', tickers.Select(t => t.Ticker));
        }

        public async Task<bool> PostTickers(string csv)
        {
            string sql = @"EXEC spStockTickersPost @TickersCSV=@p1";

            List<SqlParameter> parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("p1", csv));
            await _awsContext.Execute(sql, parms, 60);

            return true;
        }

        public async Task<string> RunImport()
        {
            await _logger.InfoAsync($"RunImport called: IsDebug={this.IsDebug} UseMessage={this.UseMessage}", "see Import Log for details");

            await IO_CalcESTDate();

            if (this.IsWeekend && !this.IsDebug)
            {
                await AddLog($"Weekend detected");
                string log = GetAllLogs();
                ClearLogs();
                return log;
            }

            await AddLog($"FinEngine: RunImport begin utc={this.UTC} est={this.EST} marketDate={this.MarketDate}");

            try
            {
                await ImportOptions();
                await ImportStocks();
            }
            catch (Exception ex)
            {
                await AddLog("IMPORT ERROR - ImportOptions or ImportStocks", ex.ToString());
            }

            try
            {
                await AddLog("HOME saving log to database");
                ImportLog importLogHOME = new ImportLog();
                importLogHOME.CreatedOn = this.UTC;
                importLogHOME.Content = GetAllLogs();
                _homeContext.ImportLog.Add(importLogHOME);
                _homeContext.Entry(importLogHOME).State = EntityState.Added;
                await _homeContext.SaveChangesAsync();
                await _homeContext.Execute("DELETE FROM ImportLog WHERE CreatedOn < DATEADD(dd, -30, GETUTCDATE())", null, 60);
            }
            catch (Exception ex)
            {
                await AddLog("IMPORT ERROR - Saving Home Log", ex.ToString());
            }

            await AddLog("FinEngine: RunImport complete");
            string log2 = GetAllLogs();
            ClearLogs();
            return log2;
        }

        public async Task<DateTime> GetLastDayMarketOpen(DateTime est)
        {
            var flag = false;
            while (flag == false)
            {
                flag = await _finData.IsMarketOpen(est);
                if (!flag) est = est.AddDays(-1);
            }
            return est;
        }

        private async Task<bool> ImportOptions()
        {
            await AddLog($"ImportOptions: dates IsMorning={this.IsMorning} utc={this.EST} est={this.EST} marketDate={this.MarketDate}");

            await _homeContext.Execute("TRUNCATE TABLE ImportStaging", null, 300);

            List<StockTicker> tickers = await GetStockTickers();
            _tickers = tickers;
            string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // synchronous processing (takes about 10 minutes)
            foreach (char c in alpha)
            {
                await IO_ProcessChar(this.MarketDate, c);
            }

            return true;
        }

        private async Task<bool> SaveStage(List<ImportStaging> quotes)
        {
            if (quotes.Count == 0) return true;

            _homeContext.ChangeTracker.Clear();
            _homeContext.ImportStaging.AddRange(quotes);

            try
            {
                await _homeContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logger.InfoAsync($"SaveStage: ERROR", ex.ToString());
                await Task.Delay(3000);
                await _homeContext.SaveChangesAsync();
            }
            finally
            {
                _homeContext.ChangeTracker.Clear();
            }

            return true;
        }

        private Task<(List<SdlChn>, List<Mx>)> BuildChains(List<OptChn> chains)
        {
            var straddles = new ConcurrentBag<SdlChn>();
            var pains = new ConcurrentBag<Mx>();

            Parallel.ForEach(chains, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, chain =>
            {
                SdlChn sc = _calculation.BuildStraddle(chain);
                straddles.Add(sc);

                OptChn filtered = _calculation.FilterOptionChain(chain);
                if (filtered?.Options?.Count > 0)
                {
                    string mstr = filtered.Options[0].Maturity().ToString("MM/dd/yyyy");
                    SdlChn sc2 = _calculation.BuildStraddle(filtered);
                    MPChain mpc = _calculation.Calculate(sc2);
                    pains.Add(new Mx(chain.Stock, mstr, chain.StockPrice, mpc.MaxPain, mpc.TotalCallOI, mpc.TotalPutOI, mpc.HighCallOI, mpc.HighPutOI));
                }
            });

            return Task.FromResult((straddles.ToList(), pains.ToList()));
        }

        public async Task<string> ImportStocks()
        {
            //await AddLog("Fetch Stock Quote data");
            List<Stock> stocks = await GetStocks();
            string xml = Utility.SerializeXml<List<Stock>>(stocks);

            bool isNew = false;
            HistoricalStockQuoteXML historicalStock = await _homeContext.HistoricalStockQuoteXML
                .Where(x => x.CreatedOn.Value.Date == this.UTC)
                .FirstOrDefaultAsync();
            if (historicalStock == null) isNew = true;

            if (isNew) historicalStock = new HistoricalStockQuoteXML();
            historicalStock.Content = xml;
            historicalStock.CreatedOn = this.UTC;

            //await AddLog("Save Stock Quote data");

            if (isNew)
            {
                _homeContext.HistoricalStockQuoteXML.Add(historicalStock);
                _homeContext.Entry(historicalStock).State = EntityState.Added;
            }
            else
            {
                _homeContext.Entry(historicalStock).State = EntityState.Modified;
            }

            await _homeContext.SaveChangesAsync();

            //await AddLog("Cleanup HistoricalStockQuoteXML");
            await _homeContext.Execute("DELETE FROM HistoricalStockQuoteXML WHERE CreatedOn < DATEADD(yy, -1, GETUTCDATE())", null, 60);

            //string log = _cache.GetAllLogs();
            //ClearCache();
            //return log;
            return string.Empty;
        }

        public async Task<DateTime?> FetchMarketDate()
        {
            DateTime utc = DateTime.UtcNow;
            DateTime est = Utility.GMTToEST(utc);
            DateTime midnightEst = Convert.ToDateTime(est.ToString("MM/dd/yyyy"));

            bool isMarketOpen = await _finData.IsMarketOpen(est);
            if (!isMarketOpen) return null;

            List<DateTime> cal = new List<DateTime>();
            cal.Add(midnightEst);
            await _homeContext.SaveMarketCalendar(cal);

            return utc;
        }

        #region Max Pain
        private async Task<bool> SavePains(List<Mx> pains)
        {
            DateTime midnightUtc = Convert.ToDateTime(this.UTC.ToString("MM/dd/yyyy"));

            // save the pains
            ImportMaxPainXml? nosql = null;
            List<ImportMaxPainXml> nosqls = await _homeContext.ImportMaxPainXml
                .Where(x => x.CreatedOn > midnightUtc)
                .ToListAsync();
            if (nosqls.Count == 0)
            {
                nosql = new ImportMaxPainXml();
                nosql.ID = 0;
                nosql.Content = Utility.SerializeXmlClean<List<Mx>>(pains);
                nosql.CreatedOn = this.UTC;

                _homeContext.ImportMaxPainXml.Add(nosql);
                _homeContext.Entry(nosql).State = EntityState.Added;
            }
            else
            {
                long index = nosqls[0].ID;
                nosql = _homeContext.ImportMaxPainXml.Find(index);
                if (nosql != null)
                {
                    nosql.Content = Utility.SerializeXmlClean<List<Mx>>(pains);
                    nosql.CreatedOn = this.UTC;

                    //_homeContext.ImportMaxPainXml.Add(nosql);
                    _homeContext.Entry(nosql).State = EntityState.Modified;
                }
            }
            await _homeContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Mx>> RebuildPains(DateTime beginDate, DateTime endDate)
        {
            List<Mx> pains = new List<Mx>();
            string xml = string.Empty;

            string sql = $"DELETE FROM ImportMaxPainXml WHERE CreatedOn BETWEEN {beginDate.ToString("MM/dd/yyyy")} AND {endDate.ToString("MM/dd/yyyy")}";
            await _homeContext.Execute(sql, null, 1800);

            try
            {
                DateTime loopDate = beginDate;
                while (loopDate <= endDate)
                {
                    pains = await RebuildPain(loopDate);
                    loopDate = loopDate.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                await _logger.InfoAsync("FinImportEngine.cs RebuildPains ERROR", ex.ToString() + "\r\n" + xml);
                throw;
            }
            return pains;
        }

        public async Task<List<Mx>> RebuildPain(DateTime currentDate)
        {
            List<Mx> pains = new List<Mx>();

            List<HistoricalOptionQuoteXML> quotes =
                await _homeContext.HistoricalOptionQuoteXML
                .Where(x => x.CreatedOn >= currentDate && x.CreatedOn <= currentDate.AddDays(1).AddMinutes(-1))
                .OrderBy(x => x.Ticker)
                .ToListAsync();

            foreach (HistoricalOptionQuoteXML quote in quotes)
            {
                if (quote.Content.StartsWith("<OptChn"))
                {
                    OptChn? chain = DBHelper.Deserialize<OptChn>(quote.Content);
                    if (chain != null)
                    {
                        chain = _calculation.FilterOptionChain(chain);
                        string mstr = chain.Options[0].Maturity().ToString("MM/dd/yyyy");
                        SdlChn sc = _calculation.BuildStraddle(chain);
                        MPChain mpc = _calculation.Calculate(sc);
                        pains.Add(new Mx(quote.Ticker, mstr, chain.StockPrice, mpc.MaxPain, mpc.TotalCallOI, mpc.TotalPutOI, mpc.HighCallOI, mpc.HighPutOI));
                    }
                }
            }

            if (pains.Count > 0)
            {
                ImportMaxPainXml nosql = new ImportMaxPainXml();
                nosql.ID = 0;
                nosql.Content = Utility.SerializeXmlClean<List<Mx>>(pains);
                nosql.CreatedOn = currentDate;
                _homeContext.ImportMaxPainXml.Add(nosql);
                _homeContext.Entry(nosql).State = EntityState.Added;
                await _homeContext.SaveChangesAsync();

                await _logger.InfoAsync($"FinImportEngine.cs RebuildPain currentDate={currentDate.ToString("MM/dd/yy")}", string.Empty);
            }

            return pains;
        }
        #endregion


        #region Most Active
        public async Task<List<MostActive>> MostActive(List<OptChn> currentList, StringBuilder sb, DateTime importDate, DateTime previousDate, bool isMorning)
        {
            DateTime utc = currentList[0].CreatedOn;
            List<Opt> previousList = await _history.GetByDate(previousDate);

            // Item 3: Dictionary (first-wins, matching the previous ILookup + FirstOrDefault behavior)
            // is faster and lower-allocation than ILookup for unique-ish keys.
            var previousLookup = new Dictionary<string, Opt>(previousList.Count);
            foreach (Opt o in previousList)
                previousLookup.TryAdd(o.ot, o);

            Dictionary<string, Opt>? twoDaysLookup = null;
            if (!isMorning)
            {
                DateTime twoDays = await _history.PreviousMarketCalendar(previousDate);
                List<Opt> twoDaysList = await _history.GetByDate(twoDays);
                twoDaysLookup = new Dictionary<string, Opt>(twoDaysList.Count);
                foreach (Opt o in twoDaysList)
                    twoDaysLookup.TryAdd(o.ot, o);
            }

            sb.AppendLine($"MostActive currentList count={currentList.Count}, previousList count={previousList.Count}");

            // find the earliest Maturity for current options
            OptChn firstChain = currentList.First(c => c.Stock.Equals("AAPL"));
            Opt? firstOpt = firstChain.Options.OrderBy(o => o.Mint()).FirstOrDefault();
            if (firstOpt == null)
                throw new InvalidOperationException($"MostActive: No options found for AAPL in currentList.");
            DateTime nextMaturity = firstOpt.Maturity();
            sb.AppendLine($"MostActive nextMaturity={nextMaturity}");

            // Item 8: pre-size to roughly the total number of options to avoid List resizes.
            int totalOptions = 0;
            for (int i = 0; i < currentList.Count; i++)
                totalOptions += currentList[i].Options?.Count ?? 0;
            List<MostActive> activeList = new List<MostActive>(totalOptions);

            foreach (OptChn chain in currentList)
            {
                foreach (Opt current in chain.Options)
                {
                    // find matching previous day optionTicker
                    if (!previousLookup.TryGetValue(current.ot, out Opt? previous))
                        continue;

                    MostActive ma = new MostActive()
                    {
                        Ticker = current.Ticker(),
                        Maturity = current.Maturity(),
                        CallPut = current.Type(),
                        Strike = current.Strike(),
                        CreatedOn = utc,
                        PrevPrice = previous.p,
                        PrevOpenInterest = previous.oi,
                        PrevVolume = previous.v,
                        PrevIV = previous.iv,
                        Price = current.p,
                        OpenInterest = current.oi,
                        Volume = current.v,
                        IV = current.iv
                    };

                    if (twoDaysLookup != null && twoDaysLookup.TryGetValue(current.ot, out Opt? twoDays))
                    {
                        ma.PrevOpenInterest = twoDays.oi;
                    }

                    ma.ChangePrice = ma.GetChangePrice();
                    ma.ChangeOpenInterest = ma.GetChangeOpenInterest();
                    ma.ChangeVolume = ma.GetChangeVolume();

                    activeList.Add(ma);
                }
            }

            if (!IsDebug)
            {
                string sql = $"TRUNCATE TABLE MostActive";
                await _homeContext.Execute(sql, null, 1800);
            }

            // Item 5: Aggregate BuildMA results in memory and skip the
            // _homeContext.MostActive.ToListAsync() round-trip. BuildMA now returns
            // its (cloned) snapshot rows so each category has independent state.
            List<MostActive> actives = new List<MostActive>(10 * 25);
            actives.AddRange(await BuildMA(activeList, QueryType.ChangeOpenInterest, nextMaturity, true));
            actives.AddRange(await BuildMA(activeList, QueryType.ChangeOpenInterest, nextMaturity, false));
            actives.AddRange(await BuildMA(activeList, QueryType.ChangePrice, nextMaturity, true));
            actives.AddRange(await BuildMA(activeList, QueryType.ChangePrice, nextMaturity, false));
            actives.AddRange(await BuildMA(activeList, QueryType.ChangeVolume, nextMaturity, true));
            actives.AddRange(await BuildMA(activeList, QueryType.ChangeVolume, nextMaturity, false));
            actives.AddRange(await BuildMA(activeList, QueryType.OpenInterest, nextMaturity, true));
            actives.AddRange(await BuildMA(activeList, QueryType.OpenInterest, nextMaturity, false));
            actives.AddRange(await BuildMA(activeList, QueryType.Volume, nextMaturity, true));
            actives.AddRange(await BuildMA(activeList, QueryType.Volume, nextMaturity, false));

            sb.AppendLine($"MostActive count={actives.Count}");

            if (!IsDebug)
                await _homeContext.Execute("spMPOutsideOIWallsXML", null, 30 * 60);

            return actives;
        }

        private async Task<List<MostActive>> BuildMA(List<MostActive> activeList, QueryType qt, DateTime nextMaturity, bool isNextMaturity)
        {
            int records = 25;

            // filter by maturity
            IEnumerable<MostActive> filtered = isNextMaturity
                ? activeList.Where(a => a.Maturity == nextMaturity)
                : activeList;

            IEnumerable<MostActive> sorted;
            switch (qt)
            {
                case QueryType.ChangeOpenInterest:
                    sorted = filtered.Where(a => a.ChangeOpenInterest > 0).OrderByDescending(a => a.ChangeOpenInterest);
                    break;
                case QueryType.ChangePrice:
                    sorted = filtered.Where(a => a.ChangePrice > 0).OrderByDescending(a => a.ChangePrice);
                    break;
                case QueryType.ChangeVolume:
                    sorted = filtered.Where(a => a.ChangeOpenInterest > 0).OrderByDescending(a => a.ChangeVolume);
                    break;
                case QueryType.OpenInterest:
                    sorted = filtered.OrderByDescending(a => a.OpenInterest);
                    break;
                case QueryType.Volume:
                    sorted = filtered.OrderByDescending(a => a.Volume);
                    break;
                default:
                    return new List<MostActive>(0);
            }

            // Clone each picked row so categories don't share mutable state
            // (Type/QueryType/SortID/NextMaturity differ per call).
            List<MostActive> result = new List<MostActive>(records);
            int sortId = 1;
            foreach (MostActive src in sorted.Take(records))
            {
                result.Add(new MostActive
                {
                    Id = src.Id,
                    Ticker = src.Ticker,
                    Maturity = src.Maturity,
                    Strike = src.Strike,
                    CallPut = src.CallPut,
                    OpenInterest = src.OpenInterest,
                    PrevOpenInterest = src.PrevOpenInterest,
                    Volume = src.Volume,
                    PrevVolume = src.PrevVolume,
                    Price = src.Price,
                    PrevPrice = src.PrevPrice,
                    IV = src.IV,
                    PrevIV = src.PrevIV,
                    CreatedOn = src.CreatedOn,
                    ChangeOpenInterest = src.ChangeOpenInterest,
                    ChangeVolume = src.ChangeVolume,
                    ChangePrice = src.ChangePrice,
                    SortID = sortId++,
                    Type = qt,
                    QueryType = qt.ToString(),
                    NextMaturity = isNextMaturity,
                });
            }

            if (!IsDebug)
            {
                string xml = Utility.SerializeXmlClean<List<MostActive>>(result);
                //File.WriteAllText(@"c:\websites\workspaces\MostActive.xml", xml);

                string sql = @"EXEC spMostActivePost @xml=@p1";
                List<SqlParameter> parms = new List<SqlParameter>();
                parms.Add(new SqlParameter("p1", xml));
                await _homeContext.Execute(sql, parms, 3600);
            }

            return result;
        }

        public async Task<List<OutsideOIWalls>> OutsideOIWalls(List<SdlChn> straddles)
        {
            List<OutsideOIWalls> wallsList = new List<OutsideOIWalls>();

            // find the earliest Maturity for current options
            SdlChn firstChain = straddles.First(s => s.Stock.Equals("AAPL"));
            Sdl firstSdl = firstChain.Straddles.OrderBy(s => s.Mint()).Take(1).ToList()[0];
            DateTime nextMaturity = firstSdl.Maturity();
            if (DateTime.UtcNow > nextMaturity.AddDays(1)) nextMaturity = nextMaturity.AddDays(7);
            await AddLog($"OIWalls nextMaturity={nextMaturity}");

            foreach (SdlChn sc in straddles)
            {
                int priorMint = 0;
                int highCallOI = 0;
                int highPutOI = 0;
                decimal highCallStrike = 0;
                decimal highPutStrike = 0;
                int sumCallOI = 0;
                int sumPutOI = 0;

                bool isMonthly = Utility.IsThirdFriday(sc.CreatedOn);

                foreach (Sdl straddle in sc.Straddles.OrderBy(s => s.Maturity()))
                {
                    // is this a different maturity? if so the reset counters
                    if (straddle.Mint() != priorMint && straddle.Maturity() == nextMaturity)
                    {
                        // is this outside the OI walls
                        if (sc.StockPrice < highPutStrike || sc.StockPrice > highCallStrike)
                        {
                            // enforce minimum total OI 
                            int totalOI = sumCallOI + sumPutOI;
                            if ((isMonthly && totalOI >= 50000) || (!isMonthly && totalOI > 20000))
                            {
                                wallsList.Add(new OutsideOIWalls()
                                {
                                    Ticker = straddle.Ticker(),
                                    Maturity = straddle.Mstr(),
                                    IsMonthlyExp = isMonthly,
                                    SumOI = totalOI,
                                    CallOI = sumCallOI,
                                    PutOI = sumPutOI,
                                    StockPrice = sc.StockPrice,
                                    PutStrike = highPutStrike,
                                    CallStrike = highCallStrike
                                });
                            }
                        }

                        // reset counters
                        highCallOI = 0;
                        highPutOI = 0;
                        highCallStrike = 0;
                        highPutStrike = 0;
                        sumCallOI = 0;
                        sumPutOI = 0;
                    }

                    // find the high call and put
                    if (straddle.coi > highCallOI)
                    {
                        highCallOI = straddle.coi;
                        highCallStrike = straddle.Strike();
                    }
                    if (straddle.poi > highPutOI)
                    {
                        highPutOI = straddle.poi;
                        highPutStrike = straddle.Strike();
                    }

                    // increment totals
                    sumCallOI += straddle.coi;
                    sumPutOI += straddle.poi;

                    priorMint = straddle.Mint();
                }
            }

            await AddLog($"OIWalls count={wallsList.Count}");

            // save to the database
            if (!IsDebug && wallsList.Count > 0)
            {
                string sql = $"DELETE FROM OutsideOIWalls";
                await _homeContext.Execute(sql, null, 1800);
                _homeContext.OutsideOIWalls.AddRange(wallsList);
                await _homeContext.SaveChangesAsync();
            }

            return wallsList;
        }
        #endregion

        #region Log
        public async Task<bool> AddLog(string subject)
        {
            return await AddLog(subject, string.Empty);
        }

        public async Task<bool> AddLog(string subject, string body)
        {
            if (UseMessage) await _logger.InfoAsync(subject, body);

            string timestamp = DateTime.UtcNow.ToString("MM/dd/yy hh:mm:ss");
            _logs.Add($"{timestamp} {subject}");
            if (!string.IsNullOrEmpty(body)) _logs.Add(body);

            _log.LogInformation("{Subject}", subject);
            if (!string.IsNullOrEmpty(body)) _log.LogInformation("{Body}", body);

            return true;
        }

        private string GetAllLogs()
        {
            return string.Join("\r\n", _logs);
        }

        private void ClearLogs()
        {
            _logs.Clear();
        }
        #endregion

        #region stock
        public async Task<List<StockTicker>> GetStockTickers()
        {
            List<PythonTicker> python = await _awsContext.GetPythonTicker();
            string json = DBHelper.Serialize(python);
            return DBHelper.Deserialize<List<StockTicker>>(json);
        }

        public async Task<List<Stock>> GetStocks()
        {
            List<StockTicker> tickers = await GetStockTickers();
            int step = 50;
            var tasks = new List<Task<List<Stock>>>();

            for (int i = 0; i < tickers.Count; i += step)
            {
                int end = Math.Min(i + step, tickers.Count);
                string csv = string.Join(',', tickers.Skip(i).Take(end - i).Select(t => t.Ticker));
                tasks.Add(_finData.FetchStock(csv));
            }

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r).ToList();
        }
        #endregion

        #region IO
        public async Task<DateTime> IO_PreProcess()
        {
            this.UseMessage = false;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            try
            {
                _log.LogInformation("IO_PreProcess: Begin");

                await IO_CalcESTDate();
                _log.LogInformation("IO_PreProcess: dates est={EST} utc={UTC} marketDate={MarketDate} isMorning={IsMorning} isWeekend={IsWeekend}", this.EST, this.UTC, this.MarketDate, this.IsMorning, this.IsWeekend);

                var token = await _finData.Schwab_Init(false, true);
                _log.LogInformation("IO_PreProcess: Schwab token refreshed");

                List<StockTicker> tickers = await GetStockTickers();
                _tickers = tickers;

                await _homeContext.Execute("TRUNCATE TABLE ImportStaging", null, 300);

                _log.LogInformation("IO_PreProcess: Add to MarketCalendarmilliseconds = {milli}", timer.ElapsedMilliseconds);
                string sql = "IF NOT EXISTS (SELECT 1 FROM MarketCalendar WHERE [Date] = @p1) INSERT INTO MarketCalendar ([Date]) VALUES (@p1)";
                await _homeContext.Execute(sql, new List<SqlParameter>() { new SqlParameter("p1", this.MarketDate) }, 30);

                _log.LogInformation("IO_PreProcess: Delete old ImportCache milliseconds = {milli}", timer.ElapsedMilliseconds);
                sql = "DELETE FROM ImportCache WHERE CreatedOn<DATEADD(dd,-14,@p1)";
                await _homeContext.Execute(sql, new List<SqlParameter>() { new SqlParameter("p1", this.MarketDate) }, 120);

                _log.LogInformation("IO_PostProcess: delete old logs milliseconds = {milli}", timer.ElapsedMilliseconds);
                sql = "DELETE FROM ImportLog WHERE CreatedOn < DATEADD(dd, -30, GETUTCDATE())";
                await _homeContext.Execute(sql, null, 60);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "IO_PreProcess: ERROR");
                await AddLog($"IO_PreProcess: ERROR {ex}");
            }

            timer.Stop();
            _log.LogInformation("IO_PreProcess: End milliseconds = {milli}", timer.ElapsedMilliseconds);

            return this.MarketDate;
        }

        public async Task<List<ImportStaging>> IO_ProcessChar(DateTime marketDate, char c)
        {
            Stopwatch timer = Stopwatch.StartNew();

            if (_tickers?.Count == 0)
            {
                _log.LogInformation("IO_ProcessChar: character={Character} fetching tickers", c);
                await AddLog($"IO_ProcessChar: character={c} fetching tickers");
                _tickers = await GetStockTickers();
            }

            var list = _tickers.Where(t => t.Ticker[0] == c).ToList();
            var tasks = list.Select(t => FetchChain(t.Ticker, marketDate)).ToList();
            var results = await Task.WhenAll(tasks);
            var quotes = results.Where(r => r != null).ToList();

            await SaveStage(quotes);
            _log.LogInformation("IO_ProcessChar: marketDate={MarketDate} character={Character} count={Count} millisecond={Elapsed}", marketDate, c, quotes.Count, timer.ElapsedMilliseconds);
            await AddLog($"IO_ProcessChar: marketDate={marketDate} character={c} count={quotes.Count} millisecond={timer.ElapsedMilliseconds}");

            return quotes;
        }

        private async Task<ImportStaging> FetchChain(string ticker, DateTime marketDate)
        {
            ImportStaging staging = null;
            try
            {
                OptChn? chain = await _finData.FetchOptions(ticker, true);
                if (chain?.Options?.Count > 0)
                {
                    staging = new ImportStaging { Ticker = ticker, CreatedOn = this.UTC, ImportDate = marketDate, Content = DBHelper.Serialize(chain) };
                }
                else if (chain != null)
                {
                    _log.LogWarning("FetchChain: empty Options for ticker={Ticker} HttpStatusCode={HttpStatusCode}", ticker, chain.HttpStatusCode);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "FetchChain: ERROR ticker={Ticker}", ticker);
                await AddLog($"FetchChain: ERROR ticker={ticker} - {ex.Message}");
            }
            return staging;
        }

        public async Task<string> IO_PostProcess(DateTime marketDate, bool isMorning)
        {
            this.UseMessage = true;
            var sb = new StringBuilder();

            Stopwatch timer = new Stopwatch();
            timer.Start();

            string msg = $"IO_PostProcess: Begin marketDate={marketDate} isMorning={isMorning}";
            _log.LogInformation(msg);
            sb.AppendLine(msg);

            List<OptChn> chains = new List<OptChn>();
            List<SdlChn> straddles = new List<SdlChn>();
            List<Mx> pains = new List<Mx>();

            string method = "spHistoricalOptionQuotePostFromStaging";
            try
            {
                if (!IsDebug)
                {
                    msg = $"IO_PostProcess: {method}: marketDate={marketDate} milliseconds={timer.ElapsedMilliseconds}";
                    _log.LogInformation(msg);
                    sb.AppendLine(msg);

                    string json = await _homeContext.FetchJson("spHistoricalOptionQuotePostFromStaging", null, 3600);

                    msg = $"IO_PostProcess: {method}: complete milliseconds={timer.ElapsedMilliseconds}";
                    _log.LogInformation(msg);
                    sb.AppendLine(msg);
                }

                method = "FetchImportStaging";
                chains = await FetchImportStaging();
                msg = $"IO_PostProcess: {method}: chains.Count={chains.Count} milliseconds={timer.ElapsedMilliseconds}";
                _log.LogInformation(msg);
                sb.AppendLine(msg);

                // NOTE: MostActive must run BEFORE BuildChains because BuildStraddle/FilterOptionChain
                // mutate OptChn.Options in place (calls/puts are removed and the list is reduced to a
                // single maturity), which would leave MostActive with empty Options collections.
                method = "MostActive";
                HistoryDate history = await _history.GetHistoryDate();
                DateTime importDate = history.CurrentDate;
                DateTime previousDate = history.PreviousDate;
                msg = $"IO_PostProcess: {method}: importDate={importDate} previousDate={previousDate} milliseconds={timer.ElapsedMilliseconds}";
                _log.LogInformation(msg);
                sb.AppendLine(msg);

                await MostActive(chains, sb, importDate, previousDate, isMorning);
                msg = $"IO_PostProcess: {method}: complete milliseconds={timer.ElapsedMilliseconds}";
                _log.LogInformation(msg);
                sb.AppendLine(msg);

                method = "BuildChains";
                (straddles, pains) = await BuildChains(chains);
                if (pains.Count > 0)
                    await SavePains(pains);
                msg = $"IO_PostProcess: {method}: chains={chains.Count} straddles={straddles.Count} pains={pains.Count} milliseconds={timer.ElapsedMilliseconds}";
                _log.LogInformation(msg);
                sb.AppendLine(msg);

                if (!IsDebug)
                {
                    method = "Screener";
                    _log.LogInformation("IO_PostProcess: Screener Start");
                    string html = await _email.ScreenerGenerate(true, true, string.Empty, IsDebug);
                    msg = $"IO_PostProcess: {method}: complete milliseconds={timer.ElapsedMilliseconds}";
                    _log.LogInformation(msg);
                    sb.AppendLine(msg);
                }

                msg = $"IO_PostProcess: end milliseconds={timer.ElapsedMilliseconds}";
                _log.LogInformation(msg);
                sb.AppendLine(msg);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "IO_PostProcess: ERROR in {Method}", method);
                sb.AppendLine($"IO_PostProcess: ERROR {method} {ex}");
            }
            finally
            {
                if (!IsDebug)
                {
                    ImportLog importLogHOME = new ImportLog();
                    importLogHOME.CreatedOn = this.UTC;
                    importLogHOME.Content = sb.ToString();

                    _homeContext.ImportLog.Add(importLogHOME);
                    _homeContext.Entry(importLogHOME).State = EntityState.Added;
                    await _homeContext.SaveChangesAsync();

                    msg = $"IO_PostProcess: save log milliseconds={timer.ElapsedMilliseconds}";
                    _log.LogInformation(msg);
                }

                msg = $"IO_PostProcess: Complete";
                _log.LogInformation(msg);
            }

            return sb.ToString();
        }

        public async Task<List<OptChn>> FetchImportStaging()
        {
            var stg = await _homeContext.FetchModel<ImportStaging>("SELECT * FROM ImportStaging", null, 30);
            return stg.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(r => DBHelper.Deserialize<OptChn>(r.Content))
                .Where(c => c?.Options?.Count > 0)
                .ToList();
        }

        private async Task IO_CalcESTDate()
        {
            this.UTC = DateTime.UtcNow;
            this.EST = Utility.GMTToEST(UTC);
            await IO_CalcESTDate(this.EST);
        }

        public async Task IO_CalcESTDate(DateTime est)
        {
            this.EST = est;
            this.UTC = Utility.ESTToGMT(est);

            this.IsMorning = false;
            this.IsWeekend = (this.EST.DayOfWeek == DayOfWeek.Saturday || EST.DayOfWeek == DayOfWeek.Sunday);

            // is the current EST time before 4pm
            this.MarketDate = this.EST;
            if (this.EST.Hour < 16)
            {
                this.IsMorning = true;
                this.MarketDate = this.MarketDate.AddDays(-1);
            }
            this.MarketDate = await GetLastDayMarketOpen(this.MarketDate);
            DateTime midnight = Convert.ToDateTime(this.MarketDate.ToString("MM/dd/yyyy"));
            this.MarketDate = midnight;
        }

        public async Task<int> IO_PatchVolume(DateTime importDate, string? ticker)
        {
            var sql = @"
                UPDATE ImportCache 
                SET CreatedOnEST = CAST(CreatedOn AS DATETIMEOFFSET) AT TIME ZONE 'UTC' AT TIME ZONE 'Eastern Standard Time'
                WHERE CreatedOnEST IS NULL;

                UPDATE ImportCache
                SET [Hour] = DATEPART(HOUR, CreatedOnEST)
                WHERE [Hour] IS NULL;
            ";
            await _homeContext.Execute(sql, null, 3600);

            List<ImportCache> srcList = new List<ImportCache>();
            List<HistoricalOptionQuote> dstList = new List<HistoricalOptionQuote>();

            srcList =
                await _homeContext.ImportCache
                .Where(x => x.ImportDate == importDate && x.Hour > 16)
                .OrderBy(x => x.Ticker)
                .ToListAsync();
            if (srcList.Count == 0) return -1;

            dstList =
                await _homeContext.HistoricalOptionQuote
                .Where(x => x.CreatedOn == importDate)
                .OrderBy(x => x.Ticker)
                .ToListAsync();
            if (dstList.Count == 0) return -1;

            var srcLookup = srcList.ToDictionary(s => s.Ticker);
            int updated = 0;

            foreach (HistoricalOptionQuote dst in dstList)
            {
                if (!srcLookup.TryGetValue(dst.Ticker, out var src)) continue;

                var dstChain = DBHelper.Deserialize<OptChn>(dst.Content);
                var srcChain = DBHelper.Deserialize<OptChn>(src.Content);
                var srcOptionLookup = srcChain.Options.ToDictionary(o => o.ot);

                bool isDirty = false;
                foreach (var dstOption in dstChain.Options)
                {
                    if (srcOptionLookup.TryGetValue(dstOption.ot, out var srcOption) && srcOption.v != dstOption.v)
                    {
                        isDirty = true;
                        updated++;
                        dstOption.v = srcOption.v;
                    }
                }

                if (isDirty)
                {
                    dst.Content = DBHelper.Serialize(dstChain);
                    _homeContext.Entry(dst).State = EntityState.Modified;
                }
            }

            if (updated > 0)
            {
                await _homeContext.SaveChangesAsync();
                _homeContext.ChangeTracker.Clear();
            }

            return updated;
        }


        #endregion
    }
}
