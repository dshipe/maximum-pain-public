using MaxPainInfrastructure.Models;

namespace MaxPainInfrastructure.Services
{
    public interface IFinImportService
    {
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


        public string GetTickersCSV(List<StockTicker> tickers);
        public Task<List<StockTicker>> GetStockTickers();

        public Task<bool> PostTickers(string csv);

        public Task<string> RunImport();

        public Task<bool> ImportOptions(DateTime utc);

        public Task<bool> SaveStage(List<ImportStaging> quotes);

        public Task<string> ImportStocks(DateTime utc);
        public Task<string> DoTransform(DateTime start);
        public Task<int> PatchVolume(DateTime importDate, string ticker);
        public Task<DateTime?> FetchMarketDate();

        public Task<DateTime> GetLastDayMarketOpen(DateTime est);

        #region Max Pain
        public Task<List<Mx>> RebuildPains(DateTime beginDate, DateTime endDate);
        public Task<List<Mx>> RebuildPain(DateTime currentDate);
        #endregion

        #region Most Active
        public Task<List<MostActive>> MostActive(List<OptChn> currentList, DateTime previousDate);
        public Task<List<OutsideOIWalls>> OutsideOIWalls(List<SdlChn> straddles);
        #endregion

        public void Sleep(int milliseconds);

        #region Log
        public Task<bool> AddLog(string subject);
        public Task<bool> AddLog(string subject, string body);
        public string GetLog();
        #endregion
    }
}
