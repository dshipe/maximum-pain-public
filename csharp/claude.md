# maximum-pain.com — Project Overview

## Purpose
This solution powers **maximum-pain.com**, a financial options-analysis platform. It collects live options chain data from the Schwab API, calculates *Max Pain* (the strike price at which the most options expire worthless), and surfaces related analytics including Most Active options, Outside OI Walls, straddle chains, and stock quotes.

---

## Solution Structure

| Project | Target | Role |
|---|---|---|
| `MaxPainInfrastructure` | .NET 10 | Core library — models, EF Core contexts, all services, business logic |
| `MaxPainLambda` | .NET 10 | AWS Lambda entry point — routes API Gateway requests, orchestrates service calls |
| `MaxPainUI` | .NET 8 | ASP.NET Core Razor + Angular SPA (front-end) |
| `UnitTestProject1` | .NET 10 | Unit / misc tests |

---

## Key Technologies

- **C# 14 / .NET 10** (Infrastructure & Lambda); **C# / .NET 8** (UI)
- **Entity Framework Core 10** with two SQL Server `DbContext` classes
- **AWS Lambda** (`Amazon.Lambda.APIGatewayEvents`) — serverless API
- **AWS Secrets Manager** — credentials storage
- **AWS SNS** — SMS notifications
- **Schwab API** — live options and stock quote data source (replaced TDA/TD Ameritrade)
- **Serilog** — structured logging in Lambda
- **Angular** — SPA within `MaxPainUI/ClientApp`
- **ClickSend** — SMS delivery
- **MailerLite** — email marketing integration

---

## Data Layer

### Two EF Core DbContexts

**`HomeContext`** (`ContextHome.cs`) — primary operational database:
- `ImportStaging` — temporary staging table (truncated each import run)
- `ImportCache` — cached raw option chain data keyed by ticker + import date
- `ImportMaxPainXml` — serialized Max Pain results (XML, one row per day)
- `HistoricalOptionQuote` / `HistoricalOptionQuoteXML` — historical options snapshots
- `HistoricalStockQuoteXML` — historical stock quotes
- `MostActive` — computed most-active options records
- `OutsideOIWalls` — tickers whose stock price is outside max OI call/put strikes
- `MarketCalendar` — trading day calendar
- `ImportLog` — import run logs (retained 30 days)

**`AwsContext`** (`ContextAws.cs`) — AWS-side / shared database:
- `StockTicker` / `PythonTicker` — master list of tracked tickers
- `EmailAccount`, `EmailStat`, `Message`, `Hop` — email/messaging data
- `BlogEntry`, `TwitterXml` — content

### Helper pattern
Raw SQL is executed via `DBHelper.Execute`, `DBHelper.FetchJson`, `DBHelper.FetchModel<T>`, and `DBHelper.FetchScalar` — not through LINQ where stored procedures are needed. Many writes use `_homeContext.Entry(entity).State = EntityState.Added/Modified` explicitly.

---

## Core Domain Models

| Model | Description |
|---|---|
| `OptChn` | Options chain for a single ticker (serializable XML) |
| `Opt` | Individual option contract within a chain |
| `SdlChn` | Straddle chain — pairs of calls/puts at each strike |
| `Sdl` | Individual straddle (call + put at same strike) |
| `Mx` | Max Pain result row: ticker, maturity, stock price, max pain strike, OI totals |
| `MPChain` | Full Max Pain calculation output |
| `MostActive` | Ranked option by volume / OI / price change |
| `OutsideOIWalls` | Tickers where stock price has broken through highest-OI call or put |
| `StockTicker` | Ticker symbol + metadata |
| `Stock` | Stock quote snapshot |

### Serialization
Domain objects are serialized to/from XML using `Utility.SerializeXml<T>()` and `Utility.SerializeXmlClean<T>()`. JSON serialization uses `DBHelper.Serialize` / `DBHelper.Deserialize<T>`.

---

## Import Pipeline

The nightly/intraday data import runs in two distinct phases:

### Phase 1 — `IO_PreProcess`
1. Calculate EST date, determine `MarketDate`, `IsMorning`, `IsWeekend`
2. Refresh Schwab OAuth token
3. Load stock tickers from `AwsContext`
4. Truncate `ImportStaging`
5. Upsert `MarketCalendar`
6. Purge old `ImportCache` and `ImportLog` rows

### Phase 2 — `IO_ProcessChar` (per letter A–Z, parallelized by ticker)
1. Filter tickers whose symbol starts with the given character
2. Call `FetchChain` ? `IFinDataService.FetchOptions` (Schwab API)
3. Persist raw JSON option chains to `ImportStaging`

### Phase 3 — `IO_PostProcess`
1. Call `spHistoricalOptionQuotePostFromStaging` stored procedure (moves staging ? history)
2. Load all chains from `ImportStaging` via `FetchImportStaging`
3. `BuildChains` — parallel computation of `SdlChn` straddles and `Mx` Max Pain results
4. `SavePains` — upsert `ImportMaxPainXml`
5. `MostActive` — compute and persist ranked most-active options via `spMostActivePost`
6. `OutsideOIWalls` — detect tickers outside OI walls, persist to `OutsideOIWalls` table
7. `Screener` — generate and email HTML screener report
8. Save `ImportLog`

---

## Key Services

| Interface | Implementation | Responsibility |
|---|---|---|
| `IFinImportService` | `FinImportService` | Orchestrates the full import pipeline |
| `IFinDataService` | `FinDataService` | Schwab API calls (options, stocks, token) |
| `ICalculationService` | `CalculationService` | Max Pain math, straddle building, chain filtering |
| `IHistoryService` | `HistoryService` | Historical option quote retrieval, market calendar |
| `IEmailService` | `EmailService` | Screener email generation and delivery |
| `IControllerService` | `ControllerService` | Business logic for UI API endpoints |
| `IChartService` | `ChartService` | Chart data construction |
| `ISchwabService` | `SchwabService` | Schwab OAuth and watchlist operations |
| `ISecretService` | `SecretService` | AWS Secrets Manager access |
| `IConfigurationService` | `ConfigurationService` | App configuration access |
| `ILoggerService` | `LoggerService` | Structured logging wrapper |
| `ISMSService` | `SMSService` | SMS via ClickSend / AWS SNS |

---

## AWS Lambda Entry Point

`MaxPainLambda/Functions.cs` is the Lambda bootstrap. It uses `Host.CreateDefaultBuilder` + `ConfigureServices` to set up DI, then delegates all request routing to `LambdaService.HandleRequest`.

`LambdaService` routes `APIGatewayHttpApiV2ProxyRequest` by URL path (`/api/{resource}/{action}`) to the appropriate service method and returns `APIGatewayHttpApiV2ProxyResponse`.

---

## Date / Time Conventions

- All internal timestamps stored in **UTC**.
- EST is computed from UTC via `Utility.GMTToEST` / `Utility.ESTToGMT`.
- "Market date" logic: if current EST hour < 16 (4 PM), the market date is yesterday.
- `IsWeekend`, `IsMorning`, `MarketDate`, `EST`, `UTC` are state properties on `FinImportService`.
- The platform uses `GetLastDayMarketOpen` to skip non-trading days by checking `IFinDataService.IsMarketOpen`.

---

## Coding Conventions

- All service classes are registered via DI; no static service access.
- `async`/`await` throughout; no `.Result` or `.Wait()` calls.
- `Parallel.ForEach` / `Task.WhenAll` used for ticker-level parallelism (degree = `Environment.ProcessorCount`).
- `ConcurrentBag<T>` used when collecting results from `Parallel.ForEach`.
- `IsDebug` flag on `FinImportService` skips destructive DB writes (truncates, deletes) and external calls during development.
- XML serialization attributes (`[XmlAttribute]`, `[XmlElement]`) on domain models.
- Stored procedures named with prefix `sp` (e.g., `spStockTickersPost`, `spMostActivePost`, `spHistoricalOptionQuotePostFromStaging`).
- Embedded resources: `HealthCheck.xsl`, `Screener.xsl`, `secret.json`.
