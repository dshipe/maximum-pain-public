using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Data;
using System.Xml;

namespace MaxPainInfrastructure.Services
{
    public class FinImportService : IFinImportService
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ILoggerService _logger;
        private readonly IConfigurationService _configuration;
        private readonly ICalculationService _calculation;
        private readonly IFinDataService _finData;
        private readonly IHistoryService _history;

        public List<OptChn> OptionChains { get; set; }
        public List<SdlChn> StraddleChains { get; set; }
        public List<Mx> Pains { get; set; }
        public OptChn UnitTestChain { get; set; }
        public List<string> Log { get; set; }


        public bool IsDebug { get; set; }
        public string TickersCSV { get; set; }
        public bool UseMessage { get; set; }
        public bool IncludeMaxPain { get; set; }
        public bool UseMostActiveCode { get; set; }
        public DateTime ImportDateUTC { get; set; }
        public bool IsMarketOpen { get; set; }
        public bool IsMorning { get; set; }

        public FinImportService(
            AwsContext awsContext,
            HomeContext homeContext,
            ILoggerService loggerService,
            IConfigurationService configurationService,
            ICalculationService _calculationService,
            IFinDataService finDataService,
            IHistoryService historyService
        )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _logger = loggerService;
            _configuration = configurationService;
            _calculation = _calculationService;
            _finData = finDataService;
            _history = historyService;

            Log = new List<string>();
            OptionChains = new List<OptChn>();

            ImportDateUTC = DateTime.MinValue;

            IncludeMaxPain = true;
            UseMostActiveCode = true;
        }


        public string GetTickersCSV(List<StockTicker> tickers)
        {
            string result = string.Empty;
            var enumer = tickers.Select(x => x.Ticker);
            return String.Join(",", enumer);
        }

        public async Task<bool> PostTickers(string csv)
        {
            string sql = @"EXEC spStockTickersPost @TickersCSV=@p1";

            List<SqlParameter> parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("p1", csv));
            await _awsContext.Execute(sql, parms, 60);

            parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("p1", csv));
            await _homeContext.Execute(sql, parms, 60);

            return true;
        }

        public async Task<string> RunImport()
        {
            await _logger.InfoAsync($"RunImport called: IsDebug={this.IsDebug} UseMessage={this.UseMessage} ImportDateUTC={this.ImportDateUTC}", "see Import Log for details");

            DateTime currentEST = Utility.CurrentDateEST();
            bool isWeekend = (currentEST.DayOfWeek == DayOfWeek.Saturday || currentEST.DayOfWeek == DayOfWeek.Sunday);
            if (isWeekend && !this.IsDebug)
            {
                await AddLog($"Weekend detected.  utc={currentEST}");
                return GetLog();
            }

            IsMorning = false;
            DateTime est;
            if (currentEST.Hour < 16)
            {
                IsMorning = true;
                est = await GetLastDayMarketOpen(currentEST.AddDays(-1));
            }
            else
            {
                est = await GetLastDayMarketOpen(currentEST);
            }
            DateTime utc = Utility.ESTToGMT(est);

            await AddLog($"FinEngine: RunImport begin IsDebug={IsDebug} UseMessage={UseMessage} utc={utc} UseMostActiveCode={UseMostActiveCode}");

            try
            {
                await ImportOptions(est);
                await ImportStocks(est);
            }
            catch (Exception ex)
            {
                await AddLog("IMPORT ERROR - ImportOptions or ImportStocks", ex.ToString());
            }

            try
            {
                await AddLog("HOME saving log to database");
                ImportLog importLogHOME = new ImportLog();
                importLogHOME.CreatedOn = utc;
                importLogHOME.Content = GetLog();
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
            return GetLog();
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

        public async Task<bool> ImportOptions(DateTime est)
        {
            // convert to EST
            DateTime utc = Utility.ESTToGMT(est);
            DateTime midnight = Convert.ToDateTime(est.ToString("MM/dd/yyyy"));

            // establish current & previous date
            // assuming it's after 4pm and we're getting data for the current day
            DateTime importDate = midnight;
            DateTime previousDate = await GetLastDayMarketOpen(midnight.AddDays(-1));

            await AddLog($"dates: IsMorning={IsMorning} utc={utc} est={est} importDate={importDate} previousDate={previousDate}");

            int limit = 20;
            if (IsDebug) limit = 20;

            await AddLog($"fetch tickers TickersCSV={TickersCSV}");
            List<StockTicker> tickers = new List<StockTicker>();
            if (!string.IsNullOrEmpty(TickersCSV))
            {
                string[] tickersArray = TickersCSV.Split(",".ToCharArray());
                for (int i = 0; i < tickersArray.Length; i++)
                {
                    tickers.Add(new StockTicker { StockTickerID = i + 1, Ticker = tickersArray[i], IsActive = true });
                }
            }
            if (tickers.Count == 0)
            {
                tickers = await GetStockTickers();
            }
            await AddLog($"tickers count={tickers.Count}");

            await _homeContext.Execute("TRUNCATE TABLE ImportStaging", null, 300);
            await AddLog($"truncated ImportStaging");

            // iterate each ticker and fetch the data
            int counter = 0;
            List<ImportStaging> quotes = new List<ImportStaging>();
            foreach (StockTicker t in tickers)
            {
                counter++;

                OptChn chain = null;
                if (UnitTestChain != null) chain = UnitTestChain;

                try
                {
                    chain = await _finData.FetchOptions(t.Ticker);
                    if (chain.Options.Count > 0) OptionChains.Add(chain);
                }
                catch (Exception ex)
                {
                    // do nothing
                    await AddLog("error", ex.ToString());
                }

                if (chain == null || chain.Options.Count == 0)
                {
                    await AddLog($"FinImportEngine ticker={t.Ticker} no nodes found");
                }
                else
                {
                    ImportStaging quote = new ImportStaging { Ticker = t.Ticker, CreatedOn = utc, ImportDate = importDate, Content = Utility.SerializeXmlClean<OptChn>(chain) };
                    quotes.Add(quote);
                }

                if (counter >= limit)
                {
                    await AddLog($"FinImportEngine partial save ticker={t.Ticker}");
                    bool partialSave = await SaveStage(quotes);
                    counter = 0;
                    quotes = new List<ImportStaging>();
                }
            }

            await AddLog($"FinImportEngine final save");
            bool finalSave = await SaveStage(quotes);
            quotes = null;

            // save data to the main table
            if (!IsDebug)
            {
                try
                {
                    await AddLog("HOME post from staging table to main table");
                    string json = await _homeContext.FetchJson("spHistoricalOptionQuoteXMLPostFromStaging", null, 3600);
                    await AddLog("HOME post from staging table to main table COMPLETE", json);
                }
                catch (Exception ex)
                {
                    await AddLog("ERROR spHistoricalOptionQuoteXMLPostFromStaging", ex.ToString());
                }
            }

            if (!IsDebug)
            {
                try
                {
                    await AddLog($"HOME PatchVolume Start utc={utc} est={est} importDate={importDate}");
                    DateTime start = DateTime.UtcNow;
                    await PatchVolume(importDate, null);
                    DateTime complete = DateTime.UtcNow;
                    var jsonObj = new
                    {
                        importDate = importDate,
                        start = start,
                        complete = complete,
                        count = 0
                    };
                    await AddLog($"HOME PatchVolume Complete", DBHelper.Serialize(jsonObj));
                }
                catch (Exception ex)
                {
                    await AddLog("ERROR PatchVolume", ex.ToString());
                }
            }


            if (IncludeMaxPain || UseMostActiveCode)
            {
                BuildChains(utc);
                if (IncludeMaxPain) await SavePains(utc);
                await AddLog($"HOME BuildChains OptionChains.Count={OptionChains.Count} StraddleChains.Count={StraddleChains.Count}");
            }

            try
            {
                HistoryDate history = await _history.GetHistoryDate();
                importDate = history.CurrentDate;
                previousDate = history.PreviousDate;
                //List<OptChn> OptionChains = await HistoryHelper.ChainGetByDate(_homeContext, importDate);

                if (UseMostActiveCode)
                {
                    await MostActive(OptionChains, previousDate);

                    //await OutsideOIWalls(StraddleChains);
                    await _homeContext.Execute("spMPOutsideOIWallsXML", null, 30 * 60);
                }
                else
                {
                    await AddLog("HOME spMPImportPostProcessing (MostActive & OI Walls");
                    await _homeContext.Execute("spMPImportPostProcessing", null, 60 * 60);
                }
            }
            catch (Exception ex)
            {
                await AddLog("ERROR MostActive / spMPImportPostProcessing", ex.ToString());
            }


            /*
			if (!IsDebug)
			{
				try
				{ 
					await AddLog("HOME MLDataSet");
					await _homeContext.Execute($"DELETE FROM MLDataSet WHERE [Date]>='{utc.ToString("MM/dd/yyyy")}'", null, 300);
					await _homeContext.Execute("spMLDataSet", null, 1800);
				}
				catch (Exception ex)
				{
					await AddLog("ERROR MLDataSet", ex.ToString());
				}
			}
			*/

            return true;
        }

        public async Task<bool> SaveStage(List<ImportStaging> quotes)
        {
            foreach (ImportStaging quote in quotes)
            {
                _homeContext.ImportStaging.Add(quote);
                _homeContext.Entry(quote).State = EntityState.Added;
            }

            try
            {
                await _homeContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                Sleep(500);
            }

            try
            {
                await _homeContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (UseMessage) await _logger.InfoAsync("IMPORT ERROR - SaveStage Home", ex.ToString());
            }

            return true;
        }

        private bool BuildChains(DateTime utc)
        {
            DateTime midnightUtc = Convert.ToDateTime(utc.ToString("MM/dd/yyyy"));

            // build out the chains
            StraddleChains = new List<SdlChn>();
            Pains = new List<Mx>();

            foreach (OptChn chain in OptionChains)
            {
                string chainJson = DBHelper.Serialize(chain);

                OptChn? oc = DBHelper.Deserialize<OptChn>(chainJson);
                if (oc != null)
                {
                    SdlChn sc = _calculation.BuildStraddle(oc);
                    StraddleChains.Add(sc);

                    if (IncludeMaxPain)
                    {
                        oc = DBHelper.Deserialize<OptChn>(chainJson);
                        oc = _calculation.FilterOptionChain(oc);

                        string mstr = oc.Options?[0].Maturity().ToString("MM/dd/yyyy");

                        SdlChn sc2 = _calculation.BuildStraddle(oc);
                        MPChain mpc = _calculation.Calculate(sc2);
                        Pains.Add(new Mx(chain.Stock, mstr, chain.StockPrice, mpc.MaxPain, mpc.TotalCallOI, mpc.TotalPutOI, mpc.HighCallOI, mpc.HighPutOI));
                    }
                }
            }

            return true;
        }

        public async Task<string> ImportStocks(DateTime est)
        {
            DateTime midnight = Convert.ToDateTime(est.ToString("MM/dd/yyyy"));
            DateTime utc = Utility.ESTToGMT(est);

            await AddLog("Fetch Stock Quote data");
            List<Stock> stocks = await GetStocks();
            string xml = Utility.SerializeXml<List<Stock>>(stocks);

            bool isNew = false;
            HistoricalStockQuoteXML historicalStock = await _homeContext.HistoricalStockQuoteXML
                .Where(x => x.CreatedOn.Value.Date == utc)
                .FirstOrDefaultAsync();
            if (historicalStock == null) isNew = true;

            if (isNew) historicalStock = new HistoricalStockQuoteXML();
            historicalStock.Content = xml;
            historicalStock.CreatedOn = utc;

            await AddLog("Save Stock Quote data");

            if (isNew)
            {
                _homeContext.HistoricalStockQuoteXML.Add(historicalStock);
                _homeContext.Entry(historicalStock).State = EntityState.Added;
            }
            else
            {
                _homeContext.Entry(historicalStock).State = EntityState.Modified;
            }

            try
            {
                await _homeContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                Sleep(5000);
                await _homeContext.SaveChangesAsync();
            }

            await AddLog("Cleanup HistoricalStockQuoteXML");
            await _homeContext.Execute("DELETE FROM HistoricalStockQuoteXML WHERE CreatedOn < DATEADD(yy, -1, GETUTCDATE())", null, 60);

            return GetLog();
        }

        public async Task<string> DoTransform(DateTime start)
        {
            string xml = string.Empty;

            DateTime loopDate = start; //Convert.ToDateTime("03/01/2020");
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            while (loopDate < DateTime.UtcNow)
            {
                await _logger.InfoAsync($"transform loopDate={loopDate}", string.Empty);
                for (int i = 0; i < alphabet.Length; i++)
                {
                    string c = alphabet.Substring(i, 1);

                    List<HistoricalOptionQuoteXML> items = await _homeContext.HistoricalOptionQuoteXML
                        .Where(x => x.CreatedOn >= loopDate && x.CreatedOn < loopDate.AddDays(1) && x.Ticker.Substring(0, 1) == c)
                        .ToListAsync();
                    foreach (HistoricalOptionQuoteXML x in items)
                    {
                        // transform
                        string original = x.Content;
                        xml = original;

                        xml = xml.Replace("<root src=\"TDA\">", "<OptChn Source=\"TDA\" Stock=\"\" StockPrice=\"\" InterestRate=\"\" Volatility=\"\" CreatedOn=\"\"><Options>");
                        xml = xml.Replace("<root src=\"Yahoo\">", "<OptChn Source=\"Yahoo\" Stock=\"\" StockPrice=\"\" InterestRate=\"\" Volatility=\"\" CreatedOn=\"\"><Options>");
                        xml = xml.Replace("</root>", "</Options></OptChn>");
                        xml = xml.Replace("<x", "<Opt");
                        xml = xml.Replace("s=", "ot=");
                        xml = xml.Replace("/>", "/>\r\n");

                        try
                        {
                            XmlDocument xmlDom = new XmlDocument();
                            xmlDom.LoadXml(xml);
                        }
                        catch (Exception ex)
                        {
                            await _logger.InfoAsync($"transform error", ex.ToString());
                            throw;
                        }

                        List<Transform> trns = await _homeContext.Transform.Where(t => t.RefID == x.ID).ToListAsync();
                        if (trns.Count == 0)
                        {
                            Transform trn = new Transform();
                            trn.RefID = x.ID;
                            trn.Ticker = x.Ticker;
                            trn.CreatedOn = x.CreatedOn;
                            trn.Content = xml;

                            _homeContext.Transform.Add(trn);
                            _homeContext.Entry(trn).State = EntityState.Added;
                            await _homeContext.SaveChangesAsync();
                        }
                    }
                }

                DateTime prevDate = loopDate;
                loopDate = loopDate.AddDays(1);
                if (loopDate < prevDate) throw new Exception($"Loop date {loopDate} less than prev date {prevDate}");
            }
            return xml;
        }

        public async Task<int> PatchVolume(DateTime importDate, string? ticker)
        {
            List<ImportCache> srcList = new List<ImportCache>();
            List<HistoricalOptionQuoteXML> dstList = new List<HistoricalOptionQuoteXML>();

            if (string.IsNullOrEmpty(ticker))
            {
                srcList =
                    await _homeContext.ImportCache
                    .Where(x => x.ImportDate == importDate && x.Hour > 16)
                    .OrderBy(x => x.Ticker)
                    .ToListAsync();
                if (srcList.Count == 0) return -1;

                dstList =
                    await _homeContext.HistoricalOptionQuoteXML
                    .Where(x => x.CreatedOn == importDate)
                    .OrderBy(x => x.Ticker)
                    .ToListAsync();
                if (dstList.Count == 0) return -1;
            }
            else
            {
                srcList =
                    await _homeContext.ImportCache
                    .Where(x => x.ImportDate == importDate && x.Hour > 16 && x.Ticker.Equals(ticker))
                    .OrderBy(x => x.Ticker)
                    .ToListAsync();
                if (srcList.Count == 0) return -1;

                dstList =
                    await _homeContext.HistoricalOptionQuoteXML
                    .Where(x => x.CreatedOn == importDate && x.Ticker.Equals(ticker))
                    .OrderBy(x => x.Ticker)
                    .ToListAsync();
                if (dstList.Count == 0) return -1;
            }

            int updated = 0;
            foreach (HistoricalOptionQuoteXML dst in dstList)
            {
                ImportCache src = srcList.Find(s => s.Ticker == dst.Ticker);
                if (src == null) continue;

                XmlDocument dstDoc = new XmlDocument();
                dstDoc.LoadXml(dst.Content);

                Hashtable hash = new Hashtable();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(src.Content);
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        reader.ReadToFollowing("Opt");
                        do
                        {
                            string? ot = reader.GetAttribute("ot");
                            string? v = reader.GetAttribute("v");
                            if (ot != null) hash.Add(ot, v);
                        } while (reader.ReadToFollowing("Opt"));
                    }
                }

                bool isDirty = false;

                foreach (XmlElement dstElm in dstDoc.SelectNodes("OptChn/Options/Opt"))
                {
                    string dstOt = dstElm.GetAttribute("ot");
                    string dstVolume = dstElm.GetAttribute("v");

                    string? srcVolume = null;
                    if (hash.ContainsKey(dstOt))
                    {
                        srcVolume = (string?)hash[dstOt];
                    }

                    if (!string.IsNullOrEmpty(srcVolume) && string.Compare(srcVolume, dstVolume, true) != 0)
                    {
                        isDirty = true;
                        updated++;
                        dstElm.SetAttribute("v", srcVolume);
                    }
                }

                if (isDirty)
                {
                    dst.Content = dstDoc.OuterXml;

                    _homeContext.HistoricalOptionQuoteXML.Add(dst);
                    _homeContext.Entry(dst).State = EntityState.Modified;
                    await _homeContext.SaveChangesAsync();
                }
            }

            return updated;
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
        private async Task<bool> SavePains(DateTime utc)
        {
            DateTime midnightUtc = Convert.ToDateTime(utc.ToString("MM/dd/yyyy"));

            // save the pains
            ImportMaxPainXml? nosql = null;
            List<ImportMaxPainXml> nosqls = await _homeContext.ImportMaxPainXml
                .Where(x => x.CreatedOn > midnightUtc)
                .ToListAsync();
            if (nosqls.Count == 0)
            {
                nosql = new ImportMaxPainXml();
                nosql.ID = 0;
                nosql.Content = Utility.SerializeXmlClean<List<Mx>>(Pains);
                nosql.CreatedOn = utc;

                _homeContext.ImportMaxPainXml.Add(nosql);
                _homeContext.Entry(nosql).State = EntityState.Added;
            }
            else
            {
                long index = nosqls[0].ID;
                nosql = _homeContext.ImportMaxPainXml.Find(index);
                if (nosql != null)
                {
                    nosql.Content = Utility.SerializeXmlClean<List<Mx>>(Pains);
                    nosql.CreatedOn = utc;

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
                if (quote.Content.IndexOf("<OptChn") == 0)
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
        public async Task<List<MostActive>> MostActive(List<OptChn> currentList, DateTime previousDate)
        {
            DateTime utc = currentList[0].CreatedOn;
            List<Opt> previousList = await _history.GetByDate(previousDate);
            ILookup<string, Opt> previousLookup = previousList.ToLookup(o => o.ot);

            Lookup<string, Opt>? twoDaysLookup = null;
            if (!IsMorning)
            {
                DateTime twoDays = await _history.PreviousMarketCalendar(previousDate);
                List<Opt> twoDaysList = await _history.GetByDate(twoDays);
                twoDaysLookup = (Lookup<string, Opt>)twoDaysList.ToLookup(o => o.ot);
            }

            await AddLog($"MostActive currentList count={currentList.Count}, previousList count={previousList.Count}");

            // find the earliest Maturity for current options
            OptChn firstChain = currentList.First(c => c.Stock.Equals("AAPL"));
            Opt firstOpt = firstChain.Options.OrderBy(o => o.Mint()).Take(1).ToList()[0];
            DateTime nextMaturity = firstOpt.Maturity();
            await AddLog($"MostActive nextMaturity={nextMaturity}");

            List<MostActive> activeList = new List<MostActive>();
            foreach (OptChn chain in currentList)
            {
                foreach (Opt current in chain.Options)
                {
                    // find matching previous day optionTicker
                    Opt? previous = previousLookup[current.ot]?.FirstOrDefault();
                    if (previous != null)
                    {
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

                        if (twoDaysLookup != null)
                        {
                            // find matching previous day optionTicker
                            previous = twoDaysLookup[current.ot]?.FirstOrDefault();
                            if (previous != null)
                            {
                                ma.PrevOpenInterest = previous.oi;
                            }
                        }

                        ma.ChangePrice = ma.GetChangePrice();
                        ma.ChangeOpenInterest = ma.GetChangeOpenInterest();
                        ma.ChangeVolume = ma.GetChangeVolume();

                        activeList.Add(ma);
                    }
                }
            }

            if (!IsDebug)
            {
                string sql = $"TRUNCATE TABLE MostActive";
                await _homeContext.Execute(sql, null, 1800);
            }

            await BuildMA(activeList, QueryType.ChangeOpenInterest, nextMaturity, true);
            await BuildMA(activeList, QueryType.ChangeOpenInterest, nextMaturity, false);
            await BuildMA(activeList, QueryType.ChangePrice, nextMaturity, true);
            await BuildMA(activeList, QueryType.ChangePrice, nextMaturity, false);
            await BuildMA(activeList, QueryType.ChangeVolume, nextMaturity, true);
            await BuildMA(activeList, QueryType.ChangeVolume, nextMaturity, false);
            await BuildMA(activeList, QueryType.OpenInterest, nextMaturity, true);
            await BuildMA(activeList, QueryType.OpenInterest, nextMaturity, false);
            await BuildMA(activeList, QueryType.Volume, nextMaturity, true);
            await BuildMA(activeList, QueryType.Volume, nextMaturity, false); ;

            List<MostActive> actives = await _homeContext.MostActive.ToListAsync();
            actives.ForEach(a => a.QueryType = a.GetQueryType());

            await AddLog($"MostActive count={actives.Count}");

            return actives;
        }

        private async Task<bool> BuildMA(List<MostActive> activeList, QueryType qt, DateTime nextMaturity, bool isNextMaturity)
        {
            int records = 25;

            // filter by maturity
            List<MostActive> filteredList = activeList;
            if (isNextMaturity)
            {
                filteredList = activeList.Where(a => a.Maturity == nextMaturity).ToList();
            }

            List<MostActive>? sortedList = null;
            switch (qt)
            {
                case QueryType.ChangeOpenInterest:
                    sortedList = filteredList.FindAll(a => a.ChangeOpenInterest > 0);
                    sortedList = sortedList.OrderByDescending(a => a.ChangeOpenInterest).Take(records).ToList();
                    break;
                case QueryType.ChangePrice:
                    sortedList = filteredList.FindAll(a => a.ChangePrice > 0);
                    sortedList = sortedList.OrderByDescending(a => a.ChangePrice).Take(records).ToList();
                    break;
                case QueryType.ChangeVolume:
                    sortedList = filteredList.FindAll(a => a.ChangeOpenInterest > 0);
                    sortedList = sortedList.OrderByDescending(a => a.ChangeVolume).Take(records).ToList();
                    break;
                case QueryType.OpenInterest:
                    sortedList = filteredList.OrderByDescending(a => a.OpenInterest).Take(records).ToList();
                    break;
                case QueryType.Volume:
                    sortedList = filteredList.OrderByDescending(a => a.Volume).Take(records).ToList();
                    break;
            }

            // update the Sort
            List<MostActive>? result = sortedList?.Take(records).ToList();
            for (int i = 0; i < result?.Count; i++)
            {
                result[i].SortID = i + 1;
                result[i].QueryType = result[i].GetQueryType();
                result[i].Type = qt;
                result[i].NextMaturity = isNextMaturity;
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

            return true;
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
                foreach (OutsideOIWalls wall in wallsList)
                {
                    _homeContext.OutsideOIWalls.Add(wall);
                    _homeContext.Entry(wall).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
                await _homeContext.SaveChangesAsync();
            }

            return wallsList;
        }
        #endregion

        public void Sleep(int milliseconds)
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                if (stopwatch.ElapsedMilliseconds >= milliseconds)
                {
                    break;
                }
                System.Threading.Thread.Sleep(1); //so processor can rest for a while
            }
        }

        #region Log
        public async Task<bool> AddLog(string subject)
        {
            return await AddLog(subject, string.Empty);
        }

        public async Task<bool> AddLog(string subject, string body)
        {
            if (UseMessage) await _logger.InfoAsync(subject, body);

            string timestamp = DateTime.UtcNow.ToString("MM/dd/yy hh:mm:ss");
            Log.Add($"{timestamp} {subject}");
            if (body != null) Log.Add($"{body}");

            return true;
        }

        public string GetLog()
        {
            return string.Join("\r\n", Log);
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
    }
}
