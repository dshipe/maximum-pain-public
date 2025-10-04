using MaxPainInfrastructure.Models;
using System.Xml;

namespace MaxPainInfrastructure.Services
{
    public interface IControllerService
    {
        #region stock
        public Task<List<Stock>> GetStocks();
        #endregion

        #region Scheduled Task
        public Task<List<string>> ScheduledTask(bool debug);

        public Task<bool> HealthCheckUnitTest(string xsltFile, XmlDocument xmlSettings, bool debug);

        public Task<bool> HealthCheck(XmlDocument xmlSettings, bool debug);

        public Task<bool> HealthCheck(string xsltFile, bool debug);

        public DateTime ExpectedOptionDate(DateTime estDate);
        #endregion

        public Task<string> DailyMonitor();
    }
}
