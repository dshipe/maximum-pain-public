using MaxPainInfrastructure.Models;
using System.Xml;

namespace MaxPainInfrastructure.Services
{
    public interface IControllerService
    {
        #region OptionHistory
        public Task<OptChn> FetchOptionHistory(string ticker, DateTime maturity, decimal strike, int pastDays, int futureDays);
        #endregion

        #region MaxPainHistory
        public Task<List<MaxPainHistory>> FetchMaxPainHistory(string ticker, DateTime maturity);
        #endregion

        #region stock
        public Task<List<StockTicker>> GetStockTickers();

        public Task<List<Stock>> GetStocks();
        #endregion

        #region Scheduled Task
        public Task<string> GetScreenerImageTicker();

        public Task<List<string>> ScheduledTask(bool debug);

        public Task<string> ExecuteImport(XmlDocument xmlSettings, bool debug, string key, int militaryHour);

        public Task<string> ScreenerDistribute(bool debug, bool useShortUrls, bool runNow);

        public Task<string> ExecuteScreener(XmlDocument xmlSettings, string imageTicker, bool debug, bool useShortUrls, string message, int militaryHour);

        public Task<bool> HealthCheckUnitTest(string xsltFile, XmlDocument xmlSettings, bool debug);

        public Task<bool> HealthCheck(XmlDocument xmlSettings, bool debug);

        public Task<bool> HealthCheck(string xsltFile, bool debug);

        public DateTime ExpectedOptionDate(DateTime estDate);

        #endregion

        public Task<byte[]> GetEmailImage(string imageTicker);

        #region EmailList
        public string EmaiListUnsubscribeHTML(string message);

        public Task<bool> EmailListUpdate(string name, string email, EmailStatus status);
        #endregion

        public Task<string> DailyMonitor();
    }
}
