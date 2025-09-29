using MaxPainInfrastructure.Code;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MaxPainInfrastructure.Models
{
    public class HomeContext : DbContext
    {
        public HomeContext() : base()
        {
        }

        public HomeContext(DbContextOptions<HomeContext> options) : base(options)
        {
        }

        public DbSet<MostActive> MostActive { get; set; }
        public DbSet<HistoricalVolatility> HistoricalVolatility { get; set; }
        public DbSet<OutsideOIWalls> OutsideOIWalls { get; set; }
        public DbSet<StockTicker> StockTicker { get; set; }
        public DbSet<HistoricalStockQuoteXML> HistoricalStockQuoteXML { get; set; }
        public DbSet<HistoricalOptionQuoteXML> HistoricalOptionQuoteXML { get; set; }
        public DbSet<ImportStaging> ImportStaging { get; set; }
        public DbSet<ImportCache> ImportCache { get; set; }
        public DbSet<ImportMaxPainXml> ImportMaxPainXml { get; set; }
        public DbSet<ImportLog> ImportLog { get; set; }
        public DbSet<Transform> Transform { get; set; }
        public DbSet<MarketCalendar> MarketCalendar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> Execute(string sql, List<SqlParameter>? parms, int timeout)
        {
            return await DBHelper.Execute(this.Database, sql, parms, timeout);
        }

        public async Task<string> FetchJson(string sql, List<SqlParameter>? parms, int timeout)
        {
            return await DBHelper.FetchJson(this.Database, sql, parms, timeout);
        }

        public async Task<object> FetchScalar(string sql, List<SqlParameter>? parms, string fieldName)
        {
            return await DBHelper.FetchScalar(this.Database, sql, parms, fieldName);
        }

        public async Task<List<HistoricalOptionQuoteXML>> GetHistoricalOptionQuoteXML(string ticker, DateTime past)
        {
            string sql = @"
                SELECT Id, Ticker, CreatedOn, Content 
                FROM HistoricalOptionQuoteXML WITH(NOLOCK) 
                WHERE Ticker = @ticker
                AND CreatedOn > @past
                ORDER BY CreatedOn
            ";

            var parms = new List<SqlParameter>
            {
                new SqlParameter("ticker", ticker),
                new SqlParameter("past", past)
            };

            string json = await DBHelper.FetchJson(this.Database, sql, parms, 60);
            return DBHelper.Deserialize<List<HistoricalOptionQuoteXML>>(json);
        }

        public async Task<DateTime> GetLastOptionsDate()
        {
            string sql = "SELECT MAX(CreatedOn) AS CreatedOn FROM vwHistoricalOptionQuoteXML WITH(NOLOCK)";
            object result = await DBHelper.FetchScalar(this.Database, sql, null, "CreatedOn");
            return Convert.ToDateTime(result);
        }

        public async Task<List<HighOpenInterest>?> HighOpenInterestRead(DateTime maturity)
        {
            string sql = "exec spMPHighOpenInterestJson";
            var parms = new List<SqlParameter>
            {
                new SqlParameter("@Maturity", maturity)
            };

            string json = await DBHelper.FetchContent(this.Database, sql, parms);
            return DBHelper.Deserialize<List<HighOpenInterest>>(json);
        }

        public async Task<List<HistoricalVolatility>?> HistoricalVolatilityRead(string ticker)
        {
            DateTime queryDate = DateTime.Now;

            string sql = @"
            SELECT 
                st.Ticker
                , hsq.ClosePrice
                , Convert(varchar, hv.[Date], 101) as [Date]
                , hv.HistoricalVol20*100.0000 as Day20
                , HistoricalVol40*100.0000 as Day40
                , HistoricalVol60*100.0000 as Day60
                , HistoricalVol120*100.0000 as Day120
                , HistoricalVol240*100.0000 as Day240
            FROM Fin.dbo.HistoricalVolatility hv WITH(NOLOCK)
            INNER JOIN Fin.dbo.StockTicker st WITH(NOLOCK)
                ON hv.StockTickerID=st.StockTickerID
            INNER JOIN HistoricalStockQuote hsq WITH(NOLOCK)
                ON st.StockTickerID=hsq.StockTickerID
                AND hv.[Date]=hsq.[Date]
            WHERE st.Ticker = @ticker
            AND hv.[Date] BETWEEN @StartDate AND @EndDate
            ORDER BY [Date]
            FOR JSON AUTO
            ";

            var parms = new List<SqlParameter>
            {
                new SqlParameter("Ticker", ticker),
                new SqlParameter("StartDate", queryDate.AddMonths(-12).ToString("MM/dd/yyyy")),
                new SqlParameter("EndDate", queryDate.AddDays(-1).ToString("MM/dd/yyyy"))
            };

            string json = await DBHelper.FetchContent(this.Database, sql, parms);
            return DBHelper.Deserialize<List<HistoricalVolatility>>(json);
        }

        public async Task<OptChn> HistoricalOptionQuoteReadJson(string ticker, DateTime maturity, decimal strike)
        {
            string sql = @"spMPOptionHistory @Ticker=@ticker";

            var parms = new List<SqlParameter>
            {
                new SqlParameter("Ticker", ticker)
            };
            if (maturity != DateTime.MinValue)
            {
                sql = string.Concat(sql, ", @Maturity=@maturity");
                parms.Add(DBHelper.CreateParm("Maturity", SqlDbType.DateTime, maturity));
            }
            if (strike != 0)
            {
                sql = string.Concat(sql, ", @Strike=strike");
                parms.Add(DBHelper.CreateParm("Strike", SqlDbType.Money, strike));
            }

            string json = await DBHelper.FetchContent(this.Database, sql, parms);

            return new OptChn
            {
                Stock = ticker,
                Options = DBHelper.Deserialize<List<Opt>>(json)
            };
        }

        public async Task<List<MaxPainHistory>?> MaxPainHistoryRead(string ticker, DateTime maturity)
        {
            string sql = @"spMPHistoricalMaxPainJson @Ticker=@ticker";

            var parms = new List<SqlParameter>
            {
                new SqlParameter("ticker", ticker)
            };
            if (maturity != DateTime.MinValue)
            {
                sql = string.Concat(sql, ", @Maturity=@maturity");
                parms.Add(DBHelper.CreateParm("Maturity", SqlDbType.DateTime, maturity));
            }

            string json = await DBHelper.FetchContent(this.Database, sql, parms);
            return DBHelper.Deserialize<List<MaxPainHistory>>(json);
        }

        public async Task<List<Mx>?> ImportMaxPainRecentRead()
        {
            string sql = @"SELECT * FROM vwImportMaxPainRecent WITH(NOLOCK)";
            string json = await DBHelper.FetchJson(this.Database, sql, null);
            return DBHelper.Deserialize<List<Mx>>(json);
        }

        public async Task<bool> SaveMarketCalendar(List<DateTime> cal)
        {
            DateTime min = cal.Min();
            DateTime max = cal.Max();

            var records = await this.MarketCalendar
                .Where(mc => mc.Date >= min && mc.Date <= max).ToListAsync();

            foreach (DateTime dt in cal)
            {
                var record = records.FirstOrDefault(mc => mc.Date == dt);
                if (record == null)
                {
                    record = new MarketCalendar
                    {
                        ID = 0,
                        Date = dt
                    };

                    this.MarketCalendar.Add(record);
                    this.Entry(record).State = EntityState.Added;
                }
            }
            await this.SaveChangesAsync();
            return true;
        }

        public async Task<List<CupWithHandleHistory>?> GetCupWithHandleHistory(DateTime? midnight)
        {
            string sql = @"
                SELECT 
                    h.Id
                    ,h.Ticker
                    ,h.StartDate
                    ,h.EndDate
                    ,h.[RightPrice]
                    ,h.[HandlePrice]
                    ,h.[CurrentPrice]
                    ,h.IsFailure
                    ,ISNULL(h.Gamma,0) AS Gamma
                    ,h.CreatedOn
                    ,h.Midnight
                FROM Python.dbo.vwCupWithHandleHistory h WITH(NOLOCK)
                ORDER BY Midnight DESC, Gamma DESC
            ";
            var parms = new List<SqlParameter>();

            if (midnight.HasValue)
            {
                sql = sql.Replace("FROM", ",[Base64] FROM");
                sql = sql.Replace("ORDER BY", "WHERE h.Midnight = @parmMidnight ORDER BY");
                parms.Add(new SqlParameter("parmMidnight", midnight.Value));
            }

            string json = await DBHelper.FetchJson(this.Database, sql, parms);
            return DBHelper.Deserialize<List<CupWithHandleHistory>>(json);
        }

        public async Task<List<DailyScan>?> DailyScanDates()
        {
            string sql = @"
                SELECT DISTINCT 
                    dr.[Date]
                FROM Python.dbo.DailyResult dr WITH(NOLOCK)
                WHERE dr.[Date] >= DATEADD(dd, -30, GETUTCDATE())
                ORDER BY dr.[Date] DESC
            ";

            string json = await DBHelper.FetchJson(this.Database, sql, null);
            return DBHelper.Deserialize<List<DailyScan>>(json);
        }

        public async Task<DateTime> DailyScanMaxDate()
        {
            string sql = @"
                SELECT MAX(dr.[Date]) AS [Date]
                FROM Python.dbo.DailyResult dr WITH(NOLOCK)
            ";

            var result = await DBHelper.FetchScalar(this.Database, sql, null, "Date");
            return Convert.ToDateTime(result);
        }


        public async Task<List<DailyScan>?> DailyScan(DateTime midnight)
        {
            string sql = @"
                SELECT TOP 50
                    dr.[Id]
                    ,dr.[Ticker]
                    ,t.[Source]
                    ,dr.[CurrentPrice]
                    ,dr.[RSRating]
                    ,dr.[Sma10Day]
                    ,dr.[Sma20Day]
                    ,dr.[Sma50Day]
                    ,dr.[Sma150Day]
                    ,dr.[Sma200Day]
                    ,dr.[Week52Low]
                    ,dr.[Week52High]
                    ,dr.[Volume]
                    ,dr.[Volume20]
                    ,dr.[VolumePerc]
                    ,dr.[ADR]
                    ,dr.[BBUpper]
                    ,dr.[BBMiddle]
                    ,dr.[BBLower]
                    ,dr.[BBW]
                    ,dr.[Date]
                    ,dr.[CreatedOn]
                    ,dr.[Base64]
                    ,p.[Base64] AS ProgressBase64
                    ,p.[CurrentPrice] AS ProgressCurrentPrice
                    ,p.[ModifiedOn] AS ProgressModifiedOn
                    ,dr.WatchFlag 
                FROM Python.dbo.DailyResult dr WITH(NOLOCK)
                INNER JOIN Python.dbo.Ticker t WITH(NOLOCK) ON dr.Ticker = t.Ticker
                LEFT JOIN Python.dbo.DailyResultProgress p WITH(NOLOCK) ON dr.Ticker = p.Ticker
                WHERE dr.[Date] = @parmMidnight
                ORDER BY dr.[RSRating] DESC
            ";

            var parms = new List<SqlParameter>
            {
                new SqlParameter("parmMidnight", midnight)
            };

            //string json = await DBHelper.FetchJson(this.Database, sql, parms);
            //var result = DBHelper.Deserialize<List<DailyScan>>(json);
            var result = await DBHelper.FetchModel<DailyScan>(this.Database, sql, parms);
            return result.OrderBy(x => x.Ticker).ToList();
        }

        public async Task<List<DailyScan>?> DailyScanUpdateWatch(int id, bool flag)
        {
            string sql = @"
                UPDATE Python.dbo.DailyResult SET WatchFlag = @parmFlag WHERE Id = @parmId

                SELECT 
                    dr.[Id]
                    ,dr.[Ticker]
                    ,t.[Source]
                    ,dr.[CurrentPrice]
                    ,dr.[RSRating]
                    ,dr.[Sma10Day]
                    ,dr.[Sma20Day]
                    ,dr.[Sma50Day]
                    ,dr.[Sma150Day]
                    ,dr.[Sma200Day]
                    ,dr.[Week52Low]
                    ,dr.[Week52High]
                    ,dr.[Volume]
                    ,dr.[Volume20]
                    ,dr.[VolumePerc]
                    ,dr.[ADR]
                    ,dr.[BBUpper]
                    ,dr.[BBMiddle]
                    ,dr.[BBLower]
                    ,dr.[BBW]
                    ,dr.[Date]
                    ,dr.[CreatedOn]
                    ,dr.WatchFlag
                FROM Python.dbo.DailyResult dr WITH(NOLOCK)
                INNER JOIN Python.dbo.Ticker t WITH(NOLOCK) ON dr.Ticker = t.Ticker
                WHERE Id = @parmId
            ";

            var parms = new List<SqlParameter>
            {
                new SqlParameter("parmId", id),
                new SqlParameter("parmFlag", flag ? "1" : "0")
            };

            string json = await DBHelper.FetchJson(this.Database, sql, parms);
            return DBHelper.Deserialize<List<DailyScan>>(json);
        }

        public async Task<string> DailyScanAdd(string ticker)
        {
            string sql = @"
DECLARE @CurrenDateEST DATETIME = CAST(GETDATE() AT TIME ZONE 'UTC' AT TIME ZONE 'Eastern Standard Time' AS DATETIME2)
DECLARE @Yesterday DATETIME = DATEADD(dd, -1, @CurrenDateEST)
DECLARE @Midnight DATETIME = CONVERT(DateTime, DATEDIFF(DAY, 0, @Yesterday))

SELECT @Midnight = MAX([Date]) FROM Python.dbo.DailyResult

--SELECT @CurrenDateEST, @Yesterday, @Midnight

--DELETE FROM Python.dbo.DailyResult WHERE Ticker=@parmTicker AND [Date]=@Midnight

IF NOT EXISTS (
	SELECT Id FROM Python.dbo.DailyResult WHERE Ticker=@parmTicker AND [Date]=@Midnight
)
BEGIN
	INSERT INTO Python.dbo.DailyResult (
	Ticker,CurrentPrice,RSRating,SMA10Day,SMA20Day
	,SMA50Day,SMA150Day,SMA200Day,Week52Low,Week52High
	,Volume,Volume20,ADR,BBUpper,BBMiddle
	,BBLower,BBW,[Date],CreatedOn,[Base64]
	,VolumePerc,WatchFlag,HasAlerted
	) VALUES (
	@parmTicker,0,99,0,0
	,0,0,0,0,0
	,0,100000,9,0,0
	,0,0,@Midnight,GetUTCDate(),NULL
	,0,1,0
	)
END

UPDATE Python.dbo.DailyResult SET HasAlerted=0 WHERE Ticker=@parmTicker AND [Date]=@Midnight

--SELECT * FROM Python.dbo.DailyResult WHERE [Date]=@Midnight ORDER BY Ticker
            ";


            var parms = new List<SqlParameter>
            {
                new SqlParameter("parmTicker", ticker)
            };

            string json = await DBHelper.FetchJson(this.Database, sql, parms);
            return "{\"Result\":\"true\"}";
        }


        public async Task<List<MarketDirection>?> MarketDirection()
        {
            string sql = @"
                SELECT 
                    md.[Ticker]
                    ,md.[CreatedOn]
                    ,md.[Base64]
                    ,md.[CandlestickBase64]
                FROM Python.dbo.MarketDirection md WITH(NOLOCK)
            ";

            string json = await DBHelper.FetchJson(this.Database, sql, null);
            return DBHelper.Deserialize<List<MarketDirection>>(json);
        }
    }
}
