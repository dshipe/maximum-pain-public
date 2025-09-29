using MaxPainInfrastructure.Models;

namespace MaxPainInfrastructure.Services
{
    public interface IHistoryService
    {
        List<OptChn> BuildChnList(List<HistoricalOptionQuoteXML> historyXmls);
        List<Opt> BuildOptList(List<HistoricalOptionQuoteXML> historyXmls);
        Task<List<OptChn>> ChainGetByDate(DateTime utc);
        Task<List<OptChn>> ChainGetByDateAndTicker(DateTime utc, string ticker);
        Task<List<OptChn>> ChainGetByTicker(string ticker, DateTime createdOn, int days);
        Task<List<Opt>> GetByDate(DateTime utc);
        Task<OptChn> GetByTicker(string ticker, int pastDays, int futureDays);
        Task<HistoryDate> GetHistoryDate();
        Task<List<HistoricalStock>> HistoricalStock(string ticker, int pastDays, int futureDays);
        Task<DateTime> PreviousMarketCalendar(DateTime utc);
        Task<DateTime> RecentMarketCalendar();
    }
}