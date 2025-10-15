USE Fin
GO

IF OBJECT_ID('tempdb..#MLDataSet') IS NOT NULL DROP TABLE #MLDataSet

CREATE TABLE #MLDataSet (
	Ticker VARCHAR(10)
	,Maturity SMALLDATETIME
	,[Date] SMALLDATETIME
	,DaysToExp INT
	,LastPrice MONEY
	,ClosePrice MONEY
	,TargetPrice MONEY
	,TargetPriceBottom MONEY
	,TargetPriceTop MONEY
	,MaxPain MONEY
	,HighCallOI INT
	,HighPutOI INT
	,TotalCallOI INT
	,TotalPutOI INT
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
	Ticker
	,Maturity
	,[Date]
	,DATEDIFF(day, [Date], Maturity)
FROM vwHistoryMaturity
WHERE DATEDIFF(day, [Date], Maturity) BETWEEN 1 AND 7
AND Maturity <= GETDATE()
AND Ticker='GE'

-- add the daily close
UPDATE #MLDataSet SET
	LastPrice = s.LastPrice
	,ClosePrice = s.ClosePrice
FROM #MLDataSet ds
INNER JOIN vwHistoricalStockQuoteXML s
	ON ds.Ticker=s.Ticker
	AND ds.[Date]=s.[Date]

DELETE FROM #MLDataSet WHERE ClosePrice IS NULL

-- add the close price on expiration
UPDATE #MLDataSet SET
	TargetPrice = s.ClosePrice
FROM #MLDataSet ds
INNER JOIN vwHistoricalStockQuoteXML s
	ON ds.Ticker=s.Ticker
	AND ds.Maturity=s.[Date]

-- add max pain data
UPDATE #MLDataSet SET 
	MaxPain = imp.MaxPain
	,HighCallOI = imp.HighCallOI
	,HighPutOI = imp.HighPutOI
	,TotalCallOI = imp.TotalCallOI
	,TotalPutOI = imp.TotalPutOI
FROM #MLDataSet ds
INNER JOIN vwImportMaxPain imp
	ON ds.Ticker=imp.Ticker
	AND ds.Maturity=imp.Maturity

-- is success
UPDATE #MLDataSet SET TargetPriceBottom = CONVERT(INT, CONVERT(FLOAT, TargetPrice))
UPDATE #MLDataSet SET TargetPriceTop = TargetPriceBottom+1
UPDATE #MLDataSet SET
	IsSuccess = CASE
		WHEN MaxPain BETWEEN TargetPriceBottom AND TargetPriceTop THEN 1
		ELSE 0
	END			

-- output results
SELECT * FROM #MLDataSet
DROP TABLE #MLDataSet


