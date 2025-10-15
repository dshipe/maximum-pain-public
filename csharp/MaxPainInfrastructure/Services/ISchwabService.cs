using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Models.Schwab;

namespace MaxPainInfrastructure.Services
{
    public interface ISchwabService
    {
        #region Auth
        public Task<string> GetSchwabLoginUrl(string callbackUrl);
        public Task<ScwToken> GetRefreshToken(string code, string callbackUrl);
        public Task<ScwToken> UpdateToken(ScwToken token);
        #endregion

        #region Options
        public Task<ScwExpirationList> GetExpirations(string accessToken, string ticker);
        public Task<OptChn> GetOptions(string accessToken, string ticker);
        public Task<List<ScwOptionCSV>> GetOptionsCSV(string accessToken, string ticker);
        public ScwOptChn ParseOptions(string json);
        public ScwOptionSymbol ParseSymbol(string content);
        public OptChn MapOptions(ScwOptChn chain);
        #endregion

        #region stocks
        public Task<List<Stock>> GetStocks(string accessToken, string tickers);
        public List<ScwStockQuote> ParseStocks(string json);
        public List<Stock> MapStocks(List<ScwStockQuote> quotes);
        #endregion

        #region Market
        public Task<bool> IsMarketOpen(string accessToken, DateTime dte);
        #endregion

        public Task<SchwabAccount> GetTradingAccount(string accessToken);
        public Task<List<SchwabAccount>> GetAccounts(string accessToken);
        public Task<string> GetOrders(string accessToken, string accountId, bool allOrders = false);

        public Task<string> GetAccountInfo(string accessToken);
        public Task<string> GetWatchList(string accessToken, string accountId);

        public Task<string> CreateWatchlist(string accessToken, string watchlistName, string[] symbols);
    }
}
