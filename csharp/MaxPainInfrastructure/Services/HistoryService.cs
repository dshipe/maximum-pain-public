using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace MaxPainInfrastructure.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;
        private readonly ILoggerService _logger;
        private readonly IConfigurationService _configuration;
        private readonly ICalculationService _calculation;

        public HistoryService(
            AwsContext awsContext,
            HomeContext homeContext,
            ILoggerService loggerService,
            IConfigurationService configurationService,
            ICalculationService _calculationService
            )
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _logger = loggerService;
            _configuration = configurationService;
            _calculation = _calculationService;
        }


        #region option
        public List<Opt> BuildOptList(List<HistoricalOptionQuoteXML> historyXmls)
        {
            List<Opt> options = new List<Opt>();
            foreach (HistoricalOptionQuoteXML historyXml in historyXmls)
            {
                OptChn chain = Utility.Deserialize<OptChn>(historyXml.Content);

                // manually set the "date"
                string d = chain.CreatedOn.ToString("MM/dd/yy");
                chain.Options.ForEach(c => c.d = d);

                options.AddRange(chain.Options);
            }
            return options;
        }

        public async Task<List<Opt>> GetByDate(DateTime utc)
        {
            DateTime midnight = Convert.ToDateTime(utc.ToString("MM/dd/yyyy"));

            List<HistoricalOptionQuoteXML> historyXmls = await _homeContext.HistoricalOptionQuoteXML.Where(q => q.CreatedOn == midnight).ToListAsync();
            //List<HistoricalOptionQuoteXML> historyXmls = await _homeContext.HistoricalOptionQuoteXML.Where(q => q.CreatedOn == midnight && q.Ticker.Equals("HRL")).ToListAsync();
            if (historyXmls == null)
            {
                historyXmls = await _homeContext.HistoricalOptionQuoteXML
                   .Where(q => q.Ticker.Equals("AAPL") && q.CreatedOn >= midnight && q.CreatedOn < midnight.AddDays(1))
                   .ToListAsync();
            }
            return BuildOptList(historyXmls);
        }

        public async Task<OptChn> GetByTicker(string ticker, int pastDays, int futureDays)
        {
            OptChn chain = new OptChn();
            chain.Stock = ticker;
            chain.CreatedOn = DateTime.UtcNow;

            DateTime past = DateTime.UtcNow.AddDays(0 - pastDays);
            DateTime future = DateTime.UtcNow.AddDays(futureDays);

            /*
            IEnumerable<HistoricalOptionQuoteXML> quotes =
                await _homeContext.HistoricalOptionQuoteXML
                .Where(x => x.Ticker == ticker && x.CreatedOn >= past)
                .OrderBy(x => x.CreatedOn)
                .ToListAsync();
            */

            List<HistoricalOptionQuoteXML> quotes = await _homeContext.GetHistoricalOptionQuoteXML(ticker, past);

            try
            {
                //Parallel.ForEach(quotes, quote =>
                foreach (HistoricalOptionQuoteXML quote in quotes)
                {
                    OptChn oc = Utility.Deserialize<OptChn>(quote.Content);
                    List<Opt>? options = null;
                    if (futureDays == 0)
                    {
                        options = oc.Options;
                    }
                    else
                    {
                        options = oc.Options.FindAll(x => x.Maturity() <= future);
                    }

                    // manually set the "date"
                    string d = oc.CreatedOn.ToString("yyMMdd");
                    options.ForEach(c => c.d = d);

                    chain.Options.AddRange(options);

                    // build out stock prices
                    StkPrc price = new StkPrc()
                    {
                        d = oc.CreatedOn.ToString("MM/dd/yy"),
                        p = oc.StockPrice
                    };
                    chain.Prices.Add(price);
                }//);
            }
            catch (Exception ex)
            {
                //string outputFile = string.Format(@"{0}\OptionHistoryChain.xml", Directory.GetCurrentDirectory());
                //File.WriteAllText(outputFile, quote.Content);
                await _logger.InfoAsync($"HistoryService.cs GetByTicker method error.  ticker={ticker} pastDays={pastDays} futureDays={futureDays}", ex.ToString());
            }

            return chain;
        }
        #endregion

        #region chain
        public List<OptChn> BuildChnList(List<HistoricalOptionQuoteXML> historyXmls)
        {
            List<OptChn> chains = new List<OptChn>();
            foreach (HistoricalOptionQuoteXML historyXml in historyXmls)
            {
                OptChn chain = Utility.Deserialize<OptChn>(historyXml.Content);
                chains.Add(chain);
            }
            return chains;
        }

        public async Task<List<OptChn>> ChainGetByDate(DateTime utc)
        {
            return await ChainGetByDateAndTicker(utc, null);
        }
        public async Task<List<OptChn>> ChainGetByDateAndTicker(DateTime utc, string? ticker)
        {
            DateTime midnight = Convert.ToDateTime(utc.ToString("MM/dd/yyyy"));

            List<HistoricalOptionQuoteXML> historyXmls = new List<HistoricalOptionQuoteXML>();
            if (string.IsNullOrEmpty(ticker))
            {
                historyXmls = await _homeContext.HistoricalOptionQuoteXML
                    .Where(q => q.CreatedOn == midnight)
                    .ToListAsync();
            }
            else
            {
                historyXmls = await _homeContext.HistoricalOptionQuoteXML
                    .Where(q => q.Ticker.Equals(ticker) && q.CreatedOn == midnight)
                    .ToListAsync();
            }

            /*
			if (historyXmls == null)
			{
				historyXmls = await _homeContext.HistoricalOptionQuoteXML
				   .Where(q => q.Ticker.Equals("AAPL") && q.CreatedOn >= midnight && q.CreatedOn < midnight.AddDays(1))
				   .ToListAsync();
			}
			*/

            return BuildChnList(historyXmls);
        }

        public async Task<List<OptChn>> ChainGetByTicker(string ticker, DateTime createdOn, int days)
        {
            List<HistoricalOptionQuoteXML> historyXmls = await _homeContext.HistoricalOptionQuoteXML
                .Where(q => q.Ticker.Equals(ticker) && q.CreatedOn >= createdOn && q.CreatedOn < createdOn.AddDays(days))
                .ToListAsync();
            return BuildChnList(historyXmls);
        }
        #endregion

        #region Dates
        public async Task<DateTime> RecentMarketCalendar()
        {
            DateTime midnight = Convert.ToDateTime(DateTime.UtcNow.ToString("MM/dd/yyyy"));
            midnight = midnight.AddDays(-30);

            List<MarketCalendar> calendars = await _homeContext.MarketCalendar
                .Where(mc => mc.Date > midnight)
                .OrderByDescending(mc => mc.Date)
                .ToListAsync();
            MarketCalendar record = calendars.First();
            return record.Date.HasValue ? record.Date.Value : DateTime.MinValue;
        }

        public async Task<DateTime> PreviousMarketCalendar(DateTime utc)
        {
            DateTime midnight = Convert.ToDateTime(DateTime.UtcNow.ToString("MM/dd/yyyy"));
            midnight = midnight.AddDays(-30);

            List<MarketCalendar> calendars = await _homeContext.MarketCalendar
                .Where(mc => mc.Date > midnight && mc.Date < utc)
                .OrderByDescending(mc => mc.Date)
                .ToListAsync();
            MarketCalendar record = calendars.First();
            return record.Date.HasValue ? record.Date.Value : DateTime.MinValue;
        }

        public async Task<HistoryDate> GetHistoryDate()
        {
            string sql = $@"
				DECLARE @HistoryDate TABLE (Id INT IDENTITY, CreatedOn SMALLDATETIME)
				INSERT INTO @HistoryDate (CreatedOn)
				SELECT TOP 2 CreatedOn
				FROM HistoricalOptionQuoteXML
				GROUP BY CreatedOn
				ORDER BY CreatedOn DESC

				DECLARE @CurrentDate SMALLDATETIME
				SELECT @CurrentDate = MAX(CreatedOn)
				FROM @HistoryDate

				DECLARE @PreviousDate SMALLDATETIME
				SELECT @PreviousDate = MAX(CreatedOn)
				FROM @HistoryDate
				WHERE CreatedOn<@CurrentDate

				SELECT @CurrentDate AS CurrentDate, @PreviousDate as PreviousDate	
			";

            string json = await _homeContext.FetchJson(sql, null, 30);
            List<HistoryDate>? histories = DBHelper.Deserialize<List<HistoryDate>>(json);
            if (histories != null) return histories[0];
            return new HistoryDate();
        }
        #endregion

        #region stock
        public async Task<List<HistoricalStock>> HistoricalStock(string ticker, int pastDays, int futureDays)
        {
            DateTime current = DateTime.Today.Date;

            List<HistoricalStockQuoteXML> historyXmls = await _homeContext.HistoricalStockQuoteXML
                .Where(q => q.CreatedOn >= current.AddDays(0 - pastDays) && q.CreatedOn <= current.AddDays(futureDays))
                .ToListAsync();

            List<HistoricalStock> stocks = new List<HistoricalStock>();
            foreach (HistoricalStockQuoteXML historyXml in historyXmls)
            {
                string xml = historyXml.Content.Replace("Stock", "HistoricalStock");

                ArrayOfHistoricalStock aos = Utility.Deserialize<ArrayOfHistoricalStock>(xml);
                HistoricalStock? s = aos.HistoricalStock.Where(s => string.Compare(s.symbol, ticker, true) == 0).FirstOrDefault();
                if (s != null)
                {
                    s.createdOn = historyXml.CreatedOn.HasValue ? historyXml.CreatedOn.Value : DateTime.MinValue;
                    stocks.Add(s);
                }
            }

            return stocks;
        }
        #endregion

    }
}
