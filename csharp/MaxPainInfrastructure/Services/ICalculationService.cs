using MaxPainInfrastructure.Models;

namespace MaxPainInfrastructure.Services
{
    public interface ICalculationService
    {
        public string FlipOptionTicker(string content, string srch, string repl);

        #region Calc
        public SdlChn BuildStraddle(OptChn chain);

        public List<MaxPainHistory> CalculateMaxPainHistory(SdlChn sc);

        public MPChain Calculate(SdlChn sc);
        #endregion

        #region Spread
        public List<Spread> BuildSpread(SdlChn sc);

        public List<Spread> BuildSpread(OptChn chain, OptionTypes optionType);
        #endregion

        #region filter
        public OptChn FilterOptionChainMaturity(OptChn chain);

        public OptChn FilterOptionChainFutureOnly(OptChn chain, bool allowEmpty = true);

        public OptChn FilterOptionChain(OptChn chain);

        public OptChn FilterOptionChain(OptChn chain, DateTime maturity);

        public OptChn FilterOptionChain(OptChn chain, int m);

        public SdlChn FilterSdlChn(SdlChn sc);

        public SdlChn FilterSdlChn(SdlChn sc, int m);
        #endregion

        #region Debug
        public OptChn DebugOptions(bool oneMaturity);

        public SdlChn DebugStraddle(bool oneMaturity);
        #endregion
    }
}
