using MaxPainInfrastructure.Models;

namespace MaxPainInfrastructure.Services
{
    public interface IChartService
    {
        #region Filter
        public SdlChn FilterStraddle(SdlChn sc, bool useStockPrice, int numberOfValues);
        #endregion

        #region MaxPain
        public ChartInfo MaxPain(SdlChn sc);

        public ChartInfo MaxPain(MPChain mpc);
        #endregion

        #region Price
        public ChartInfo Price(SdlChn sc, string title, bool isPut);
        #endregion

        #region IV
        public ChartInfo IVPredict(SdlChn sc, string title, string key, int degree);
        #endregion

        #region Generic Line
        public ChartInfo LineDouble(SdlChn sc, string title, string key, int? zoomIn);

        public ChartInfo LineSingle(SdlChn sc, string title, string key);
        #endregion

        #region History Line
        public ChartInfo HistoryLineDouble(SdlChn sc, string title, string key);

        public ChartInfo HistoryMaxPain(List<MaxPainHistory> quotes, bool showStockPrice);
        #endregion

        #region Other Charts
        public ChartInfo HistoricalVolatility(List<HistoricalVolatility> vols);

        public ChartInfo HighOpenInterest(List<HighOpenInterest> highs);
        #endregion

        #region Stacked
        public ChartInfo Stacked(SdlChn sc, string title, string key, bool isPut);
        #endregion


        #region Images
        public Task<byte[]> FetchImage(string route, string ticker, DateTime maturity);

        public Task<byte[]> FetchImage(ChartInfo info);

        public Task<byte[]> PostURI(string url, string payload);
        #endregion

    }
}
