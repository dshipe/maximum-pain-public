using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using System.Data;
using System.Text.RegularExpressions;

namespace MaxPainInfrastructure.Services
{
    public class CalculationService : ICalculationService
    {
        public string FlipOptionTicker(string content, string srch, string repl)
        {
            string pattern = string.Concat(@"([A-Z]+)(\d{6})", srch);
            return Regex.Replace(content, pattern, string.Concat("$1$2", repl));
        }


        #region Calc
        public SdlChn BuildStraddle(OptChn chain)
        {
            bool useLookup = true;

            bool hasDate = chain.Options.Count != 0 && chain.Options[0].d != null && chain.Options[0].d.Length > 0;

            SdlChn sc = new SdlChn
            {
                Source = chain.Source,
                Stock = chain.Stock,
                StockPrice = chain.StockPrice,
                InterestRate = chain.InterestRate,
                Prices = chain.Prices
            };

            var calls = chain.Options.Where(x => x.OptionType() == OptionTypes.Call).ToList();
            var callsLookup = calls.ToLookup(o => o.ot);

            var puts = chain.Options.Where(x => x.OptionType() == OptionTypes.Put).ToList();
            var putsLookup = puts.ToLookup(o => o.ot);

            foreach (var call in calls)
            {
                var straddle = new Sdl
                {
                    ot = call.ot,
                    d = call.d,
                    clp = call.p,
                    cb = call.b,
                    ca = call.a,
                    coi = call.oi,
                    cv = call.v,
                    civ = call.iv,
                    cde = call.de,
                    cga = call.ga,
                    cth = call.th,
                    cve = call.ve,
                    crh = call.rh
                };

                Opt? put = null;
                if (useLookup)
                {
                    string symbol = FlipOptionTicker(call.ot, "C", "P");
                    put = putsLookup[symbol].FirstOrDefault(x => x.d == call.d);
                }
                else
                {
                    put = chain.Options.FirstOrDefault(x => x.Mint() == call.Mint() && x.Strike() == call.Strike() && x.OptionType() == OptionTypes.Put && x.d == call.d);
                }
                if (put != null)
                {
                    straddle.plp = put.p;
                    straddle.pb = put.b;
                    straddle.pa = put.a;
                    straddle.poi = put.oi;
                    straddle.pv = put.v;
                    straddle.piv = put.iv;
                    straddle.pde = put.de;
                    straddle.pga = put.ga;
                    straddle.pth = put.th;
                    straddle.pve = put.ve;
                    straddle.prh = put.rh;

                    chain.Options.Remove(put);
                }

                sc.Straddles.Add(straddle);
                chain.Options.Remove(call);
            }

            var straddleLookup = sc.Straddles.ToLookup(s => s.ot);
            foreach (var put in puts)
            {
                Sdl? straddle = null;
                string? symbol = null;
                if (useLookup)
                {
                    symbol = FlipOptionTicker(put.ot, "P", "C");
                    straddle = straddleLookup[symbol].FirstOrDefault(x => x.d == put.d);
                }
                else
                {
                    straddle = sc.Straddles.FirstOrDefault(x => x.Mint() == put.Mint() && x.Strike() == put.Strike() && x.d == put.d);
                }
                if (straddle == null)
                {
                    straddle = new Sdl
                    {
                        ot = put.ot,
                        d = put.d,
                        plp = put.p,
                        pb = put.b,
                        pa = put.a,
                        poi = put.oi,
                        pv = put.v,
                        piv = put.iv,
                        pde = put.de,
                        pga = put.ga,
                        pth = put.th,
                        pve = put.ve,
                        prh = put.rh
                    };

                    Opt? call = null;
                    if (useLookup)
                    {
                        call = callsLookup[symbol].FirstOrDefault(x => x.d == put.d);
                    }
                    else
                    {
                        call = chain.Options.FirstOrDefault(x => x.Mint() == put.Mint() && x.Strike() == put.Strike() && x.OptionType() == OptionTypes.Call && x.d == put.d);
                    }
                    if (call != null)
                    {
                        straddle.clp = call.p;
                        straddle.cb = call.b;
                        straddle.ca = call.a;
                        straddle.coi = call.oi;
                        straddle.cv = call.v;
                        straddle.civ = call.iv;
                        straddle.cde = call.de;
                        straddle.cga = call.ga;
                        straddle.cth = call.th;
                        straddle.cve = call.ve;
                        straddle.crh = call.rh;
                    }

                    sc.Straddles.Add(straddle);
                }
            }

            sc.Straddles = hasDate
                ? sc.Straddles.OrderBy(x => x.Ticker()).ThenBy(x => x.d).ThenBy(x => x.Mint()).ThenBy(x => x.Strike()).ToList()
                : sc.Straddles.OrderBy(x => x.Ticker()).ThenBy(x => x.Mint()).ThenBy(x => x.Strike()).ToList();

            return sc;
        }

        public List<MaxPainHistory> CalculateMaxPainHistory(SdlChn sc)
        {
            var histories = new List<MaxPainHistory>();

            var temp = DBHelper.Deserialize<SdlChn>(DBHelper.Serialize(sc));

            var dates = sc.Straddles.Select(x => x.d).Distinct();
            foreach (var date in dates)
            {
                temp.Straddles = sc.Straddles.Where(x => x.d == date).ToList();
                var mp = Calculate(temp);

                var price = sc.Prices.FirstOrDefault(x => x.d == date);

                var history = new MaxPainHistory
                {
                    CC = mp.TotalCallOI,
                    COI = mp.HighCallOI,
                    D = date,
                    M = temp.Straddles[0].Mstr(),
                    MP = mp.MaxPain,
                    PC = mp.TotalPutOI,
                    POI = mp.HighPutOI,
                    SP = price?.p ?? 0,
                    TK = temp.Stock
                };

                histories.Add(history);
            }

            return histories;
        }

        public MPChain Calculate(SdlChn sc)
        {
            var maturities = sc.Straddles.Select(x => x.Mint()).Distinct();
            if (maturities.Count() > 1) throw new Exception("calculate method can only handle a single maturity.");

            var ticker = sc.Straddles[0].Ticker();
            var result = new MPChain
            {
                Stock = ticker,
                StockPrice = sc.StockPrice,
                CreatedOn = sc.CreatedOn,
                Maturity = sc.Straddles[0].Maturity(),
                Mint = sc.Straddles[0].Mint()
            };

            decimal maxCash = 0;
            decimal minCash = 0;

            var strikes = sc.Straddles.Select(x => x.Strike()).Distinct();

            decimal maxCallOI = 0;
            decimal maxPutOI = 0;
            int totalCallOI = 0;
            int totalPutOI = 0;

            foreach (var stockPrice in strikes)
            {
                decimal callCash = 0;
                decimal putCash = 0;

                var mp = new MPItem { s = stockPrice };

                var straddleStrike = sc.Straddles.FirstOrDefault(x => x.Strike() == stockPrice);

                if (straddleStrike != null)
                {
                    mp.coi = straddleStrike.coi;
                    totalCallOI += straddleStrike.coi;
                    maxCallOI = Math.Max(maxCallOI, straddleStrike.coi);

                    mp.poi = straddleStrike.poi;
                    totalPutOI += straddleStrike.poi;
                    maxPutOI = Math.Max(maxPutOI, straddleStrike.poi);
                }

                result.Items.Add(mp);

                foreach (var straddle in sc.Straddles)
                {
                    var strike = straddle.Strike();

                    var callIntrinsic = Math.Max(0, stockPrice - strike);
                    callCash += callIntrinsic * (straddle.coi * 100);

                    var putIntrinsic = Math.Max(0, strike - stockPrice);
                    putCash += putIntrinsic * (straddle.poi * 100);
                }

                var totalCash = callCash + putCash;
                if (minCash == 0) minCash = totalCash;
                if (totalCash < minCash) minCash = totalCash;
                if (totalCash > maxCash) maxCash = totalCash;

                mp.cch = callCash;
                mp.pch = putCash;
            }

            var highCallOI = sc.Straddles.FirstOrDefault(x => x.coi == maxCallOI)?.Strike() ?? 0;
            var highPutOI = sc.Straddles.FirstOrDefault(x => x.poi == maxPutOI)?.Strike() ?? 0;

            var maxPain = result.Items.FirstOrDefault(x => x.TotalCash() == minCash)?.s ?? 0;
            result.MaxPain = maxPain;
            result.MinCash = minCash;
            result.MaxCash = maxCash;
            result.HighCallOI = highCallOI;
            result.HighPutOI = highPutOI;
            result.TotalCallOI = totalCallOI;
            result.TotalPutOI = totalPutOI;
            result.PutCallRatio = totalCallOI == 0 ? 0 : (decimal)totalPutOI / totalCallOI;

            if (maxCash != 0)
            {
                foreach (var mp in result.Items)
                {
                    mp.cpd = mp.cch / maxCash;
                    mp.ppd = mp.pch / maxCash;

                    var percentDiff = mp.cch + mp.pch - minCash;
                    mp.pd = minCash == 0 ? 0 : percentDiff / minCash * 100;

                    var straddle = sc.Straddles.FirstOrDefault(x => x.Strike() == mp.s);
                    mp.coi = straddle?.coi ?? 0;
                    mp.poi = straddle?.poi ?? 0;
                }
            }

            result.Items = result.Items.OrderBy(x => x.s).ToList();

            return result;
        }
        #endregion

        #region Spread
        public List<Spread> BuildSpread(SdlChn sc)
        {
            List<Spread> spreads = new List<Spread>();

            IEnumerable<int> maturities = sc.Straddles.Select(x => x.Mint()).Distinct();
            foreach (int maturity in maturities)
            {
                List<Sdl> subset = sc.Straddles.FindAll(x => x.Mint() == maturity).OrderBy(x => x.Strike()).ToList();

                for (int i = 0; i < subset.Count; i++)
                {
                    // do not calc for last Call
                    if (i < subset.Count - 1)
                    {
                        Spread spread = new Spread();
                        spread.Ticker = subset[i].Ticker();
                        spread.Maturity = subset[i].Maturity();
                        spread.OptionType = OptionTypes.Call;
                        spread.ModifiedOn = sc.CreatedOn;

                        spread.LongStrike = subset[i].Strike();
                        spread.LongPrice = subset[i].clp;
                        spread.LongBid = subset[i].cb;
                        spread.LongAsk = subset[i].ca;

                        int j = i + 1;
                        spread.ShortStrike = subset[j].Strike();
                        spread.ShortPrice = subset[j].clp;
                        spread.ShortBid = subset[j].cb;
                        spread.ShortAsk = subset[j].ca;

                        spreads.Add(spread);
                    }

                    // do not calc for first Put
                    if (i > 0)
                    {
                        Spread spread = new Spread();
                        spread.Ticker = subset[i].Ticker();
                        spread.Maturity = subset[i].Maturity();
                        spread.OptionType = OptionTypes.Put;
                        spread.ModifiedOn = sc.CreatedOn;

                        spread.LongStrike = subset[i].Strike();
                        spread.LongPrice = subset[i].plp;
                        spread.LongBid = subset[i].pb;
                        spread.LongAsk = subset[i].pa;

                        int j = i - 1;
                        spread.ShortStrike = subset[j].Strike();
                        spread.ShortPrice = subset[j].plp;
                        spread.ShortBid = subset[j].pb;
                        spread.ShortAsk = subset[j].pa;

                        spreads.Add(spread);
                    }
                }
            }
            return spreads;
        }

        public List<Spread> BuildSpread(OptChn chain, OptionTypes optionType)
        {
            List<Spread> spreads = new List<Spread>();

            List<Opt> options = chain.Options
                .FindAll(x => x.OptionType() == optionType).OrderBy(x => x.Strike())
                .ToList();
            for (int i = 0; i < options.Count; i++)
            {
                Opt point = options[i];
                Opt lower = (i == 0) ? null : options[i - 1];
                Opt higher = (i == options.Count - 2) ? null : options[i + 1];
                Opt next = (optionType == OptionTypes.Call) ? higher : lower;

                Spread spread = new Spread();

                spread.LongStrike = point.Strike();
                spread.LongPrice = point.a;
                spread.ShortStrike = next.Strike();
                spread.ShortPrice = next.b;
            }

            spreads.Sort((x, y) =>
            {
                int compare = x.Ticker.CompareTo(y.Ticker);
                if (compare != 0)
                    return compare;

                compare = x.Maturity.CompareTo(y.Maturity);
                if (compare != 0)
                    return compare;

                return x.LongStrike.CompareTo(y.LongStrike);
            });

            return spreads;
        }
        #endregion

        #region filter
        public OptChn FilterOptionChainMaturity(OptChn chain)
        {
            var current = DateTime.Now;
            var future = new DateTime(current.Year + 1, 2, 1);
            if (current.AddMonths(6) > future) future = current.AddMonths(6);
            var next = Utility.DateToYMD(future);

            chain.Options = chain.Options.Where(x => x.Mint() < next).ToList();
            return chain;
        }

        public OptChn FilterOptionChainFutureOnly(OptChn chain, bool allowEmpty = true)
        {
            var current = Utility.DateToYMD(DateTime.Now);
            if (allowEmpty)
            {
                var mint = chain.Options.Where(x => x.Mint() > current).Min(x => x.Mint());
                return FilterOptionChain(chain, mint);
            }
            else
            {
                var nonZero = chain.Options.Where(x => x.Mint() > current && x.oi > 0).ToList();
                var mint = nonZero.GroupBy(x => x.Mint()).FirstOrDefault(g => g.Count() > 1)?.Key ?? 0;
                return FilterOptionChain(chain, mint);
            }
        }

        public OptChn FilterOptionChain(OptChn chain)
        {
            if (chain.Options.Count == 0) return chain;
            var m = chain.Options.Min(x => x.Mint());
            return FilterOptionChain(chain, m);
        }

        public OptChn FilterOptionChain(OptChn chain, DateTime maturity)
        {
            var m = chain.Options.Min(x => x.Mint());
            if (maturity != DateTime.MinValue) m = Utility.DateToYMD(maturity);
            return FilterOptionChain(chain, m);
        }

        public OptChn FilterOptionChain(OptChn chain, int m)
        {
            chain.Options = chain.Options.Where(x => x.Mint() == m).ToList();
            return chain;
        }

        public SdlChn FilterSdlChn(SdlChn sc)
        {
            var m = sc.Straddles.Min(x => x.Mint());
            return FilterSdlChn(sc, m);
        }

        public SdlChn FilterSdlChn(SdlChn sc, int m)
        {
            sc.Straddles = sc.Straddles.Where(x => x.Mint() == m).ToList();
            return sc;
        }
        #endregion

        #region Debug
        public OptChn? DebugOptions(bool oneMaturity)
        {
            string jsonFile = string.Format(@"{0}\json\OptionChain.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(jsonFile);
            OptChn? chain = DBHelper.Deserialize<OptChn>(json);

            if (oneMaturity)
            {
                int m = chain.Options[0].Mint();
                List<Opt> options = chain.Options
                    .FindAll(x => x.Mint() == m)
                    .ToList();
                chain.Options = options;
            }

            return chain;

        }

        public SdlChn? DebugStraddle(bool oneMaturity)
        {
            string jsonFile = string.Format(@"{0}\json\StraddleChain.json", Directory.GetCurrentDirectory());
            string json = File.ReadAllText(jsonFile);
            SdlChn? sc = DBHelper.Deserialize<SdlChn>(json);

            if (oneMaturity)
            {
                int m = sc.Straddles[0].Mint();
                List<Sdl> straddles = sc.Straddles
                    .FindAll(x => x.Mint() == m)
                    .ToList();
                sc.Straddles = straddles;
            }

            return sc;
        }
        #endregion
    }
}
