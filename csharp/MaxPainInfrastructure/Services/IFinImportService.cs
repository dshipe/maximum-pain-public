using MaxPainInfrastructure.Models;
using System.Text;

namespace MaxPainInfrastructure.Services
{
    public interface IFinImportService
    {
        public bool IsDebug { get; set; }
        public string TickersCSV { get; set; }
        public bool UseMessage { get; set; }
        public bool IsMarketOpen { get; set; }
        public bool IsMorning { get; set; }
        public bool IsWeekend { get; set; }
        public DateTime MarketDate { get; set; }
        public DateTime EST { get; set; }
        public DateTime UTC { get; set; }

        public string GetTickersCSV(List<StockTicker> tickers);
        public Task<List<StockTicker>> GetStockTickers();

        public Task<bool> PostTickers(string csv);

        public Task<string> RunImport();
        public Task<string> ImportStocks();

        public Task<DateTime?> FetchMarketDate();

        public Task<DateTime> GetLastDayMarketOpen(DateTime est);

        #region Max Pain
        public Task<List<Mx>> RebuildPains(DateTime beginDate, DateTime endDate);
        public Task<List<Mx>> RebuildPain(DateTime currentDate);
        #endregion

        #region Most Active
        public Task<List<MostActive>> MostActive(List<OptChn> currentList, StringBuilder sb, DateTime importDate, DateTime previousDate, bool isMorning);
        public Task<List<OutsideOIWalls>> OutsideOIWalls(List<SdlChn> straddles);

        public Task<List<OptChn>> FetchImportStaging();
        #endregion

        #region Log
        public Task<bool> AddLog(string subject);
        public Task<bool> AddLog(string subject, string body);
        #endregion

        public Task<DateTime> IO_PreProcess();
        public Task<List<ImportStaging>> IO_ProcessChar(DateTime marketDate, Char c);
        public Task<string> IO_PostProcess(DateTime marketDate, bool isMorning);

        public Task IO_CalcESTDate(DateTime est);

        public Task<int> IO_PatchVolume(DateTime importDate, string ticker);
    }
}
