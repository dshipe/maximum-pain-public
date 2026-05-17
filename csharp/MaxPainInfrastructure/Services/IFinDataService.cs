using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Models.Schwab;

namespace MaxPainInfrastructure.Services
{
    public interface IFinDataService
    {
        public Task<DateTime> GetCreatedOn();

        #region Controller
        public Task<OptChn> FetchOptionChain(string ticker, DateTime maturity);

        public Task<OptChn> FetchOptionChain(string ticker, DateTime maturity, bool useNearestExpiration);
        #endregion

        #region Generic
        public Task<bool> IsMarketOpen(DateTime dt);

        public Task<OptChn> FetchOptions(string ticker, bool useCachedToken = false);

        public Task<List<Stock>> FetchStock(string tickers);

        public Task<OptChn> FetchOptionData(string ticker);

        public Task<List<ScwOptionCSV>> FetchOptionCSV(string ticker);
        #endregion

        public Task<List<SchwabAccount>> Schwab_Account(bool useCachedToken = false);

        public Task<string> Schwab_Watchlist(bool useCachedToken = false);

        public Task<ScwToken> Schwab_Init(bool useCachedToken, bool force = false);
    }
}
