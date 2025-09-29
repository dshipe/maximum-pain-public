USE Fin
GO

/*
IF EXISTS (SELECT * FROM Sys.Tables WHERE Name='MLDataSet') DROP TABLE MLDataSet
CREATE TABLE MLDataSet 
(
	Ticker VARCHAR(10)
	,Maturity SMALLDATETIME
	,[Date] SMALLDATETIME
	,DaysToExp INT

	--,CallPut CHAR(1)
	--,Strike MONEY
	--,Price MONEY
	--,Change FLOAT
	--,Bid MONEY
	--,Ask MONEY
	--,Volume INT
	--,OpenInterest INT
	--,ImpliedVolatility FLOAT
	--,Delta FLOAT
	--,Gamma FLOAT
	--,Theta FLOAT
	--,Vega FLOAT
	--,Rho FLOAT

	,LastPrice MONEY
	,ClosePrice MONEY
	,TargetPrice MONEY

	,MaxPain MONEY
	,HighCallOI INT
	,HighPutOI INT
	,TotalCallOI INT
	,TotalPutOI INT
	,IsOutside BIT
	,IsSuccess BIT

	,IsNew BIT
)
*/

IF OBJECT_ID('tempdb..#MLDataSet') IS NOT NULL DROP TABLE #MLDataSet
CREATE TABLE #MLDataSet 
(
	Ticker VARCHAR(10)
	,Maturity SMALLDATETIME
	,[Date] SMALLDATETIME
	,DaysToExp INT

	--,CallPut CHAR(1)
	--,Strike MONEY
	--,Price MONEY
	--,Change FLOAT
	--,Bid MONEY
	--,Ask MONEY
	--,Volume INT
	--,OpenInterest INT
	--,ImpliedVolatility FLOAT
	--,Delta FLOAT
	--,Gamma FLOAT
	--,Theta FLOAT
	--,Vega FLOAT
	--,Rho FLOAT

	,LastPrice MONEY
	,ClosePrice MONEY
	,TargetPrice MONEY

	,MaxPain MONEY
	,HighCallOI INT
	,HighPutOI INT
	,TotalCallOI INT
	,TotalPutOI INT
	,IsOutside BIT
	,IsSuccess BIT
)

-- collect option data
INSERT INTO #MLDataSet
(
	Ticker
	,Maturity
	,[Date]
	,DaysToExp
)	
SELECT DISTINCT
	hm.Ticker
	,hm.Maturity
	,hm.[Date]
	,DATEDIFF(day, hm.[Date], hm.Maturity)
FROM vwHistoryMaturity hm
WHERE hm.Ticker = 'GE'
AND DATEDIFF(day, hm.[Date], hm.Maturity) BETWEEN 1 AND 7
--AND hm.Maturity <= GETDATE()

-- add the daily close
UPDATE #MLDataSet SET
	LastPrice = s.LastPrice
	,ClosePrice = s.ClosePrice
FROM #MLDataSet ml
INNER JOIN vwHistoricalStockQuoteXML s
	ON ml.Ticker=s.Ticker
	AND ml.[Date]=s.[Date]

DELETE FROM #MLDataSet WHERE ClosePrice IS NULL

-- add the close price on expiration
UPDATE #MLDataSet SET
	TargetPrice = s.ClosePrice
FROM #MLDataSet ml
INNER JOIN vwHistoricalStockQuoteXML s
	ON ml.Ticker=s.Ticker
	AND ml.Maturity=s.[Date]

-- add max pain data
UPDATE #MLDataSet SET 
	MaxPain = imp.MaxPain
	,HighCallOI = imp.HighCallOI
	,HighPutOI = imp.HighPutOI
	,TotalCallOI = imp.TotalCallOI
	,TotalPutOI = imp.TotalPutOI
FROM #MLDataSet ml
INNER JOIN vwImportMaxPain imp
	ON ml.Ticker=imp.Ticker
	AND ml.Maturity=imp.Maturity

-- is success
-- when stock price closes within walls
UPDATE #MLDataSet SET
	IsOutside = CASE
		WHEN ClosePrice BETWEEN HighPutOI AND HighCallOI THEN 0
		ELSE 1
	END			
	,IsSuccess = CASE
		WHEN TargetPrice BETWEEN HighPutOI AND HighCallOI THEN 1
		ELSE 0
	END			


USE Fin
GO

-- SELECT * FROM MLDataSet WHERE Maturity>='2020-11-06'

SELECT ml.Ticker, ml.Maturity, ml.[Date], ml.LastPrice, ml.ClosePrice, s.LastPrice, s.ClosePrice
FROM MLDataSet ml
INNER JOIN vwHistoricalStockQuoteXML s
	ON ml.Ticker = s.Ticker
	AND ml.[Date] = s.[Date]
WHERE ml.Ticker='AAPL' 
ORDER BY ml.[Maturity], ml.[Date]

SELECT * FROM MLDataSet WHERE Ticker='AAPL' ORDER BY [Maturity],[Date]

SELECT
	ds.*
	,CASE WHEN x.Ticker IS NULL THEN 0 ELSE 1 END AS WasOutside
FROM MLDataSet ds
LEFT JOIN (
	SELECT Ticker, Maturity
	FROM MLDataSet
	WHERE IsOutside=1
	GROUP BY Ticker, Maturity
) AS x
	ON ds.Ticker=x.Ticker
	AND ds.Maturity=x.Maturity
WHERE ds.Ticker='AAPL'
ORDER BY ds.Ticker, ds.[Maturity], ds.[Date]

