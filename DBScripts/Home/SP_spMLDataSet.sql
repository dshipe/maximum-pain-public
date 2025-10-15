USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMLDataSet]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMLDataSet]	
GO

CREATE PROCEDURE dbo.spMLDataSet
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	rebuild ML Data Set

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.


	Revision History:

	Date		Name	Description
	----		----	-----------
	2020.11.08	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN	
	SET NOCOUNT ON

	DECLARE @Dates TABLE (Ticker VARCHAR(10), [Date] DATETIME)

	INSERT INTO @Dates
	(
		Ticker
		,[Date]
	)	
	SELECT DISTINCT
		hoq.Ticker
		,dateadd(dd,0, datediff(dd,0,hoq.CreatedOn))
	FROM HistoricalOptionQuoteXml hoq WITH(NOLOCK)
	LEFT JOIN MLDataSet ml WITH(NOLOCK)
		ON hoq.Ticker=ml.Ticker
		AND dateadd(dd,0, datediff(dd,0,hoq.CreatedOn))=ml.[Date]
	WHERE ml.Ticker IS NULL 
	AND hoq.CreatedOn >'4/1/2020'
	--AND hoq.Ticker='aapl'

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
	FROM vwHistoryMaturity hm WITH(NOLOCK)
	INNER JOIN @Dates d
		ON hm.Ticker=d.Ticker
		AND hm.[Date]=d.[Date]
	LEFT JOIN #MLDataSet ml
		ON hm.Ticker=ml.Ticker
		AND hm.Maturity=ml.Maturity
		AND hm.[Date]=ml.[Date]
	WHERE ml.Ticker IS NULL 
	AND DATEDIFF(day, hm.[Date], hm.Maturity) BETWEEN 0 AND 7
	AND hm.Maturity <= DATEADD(dd,7,GETDATE())

	-- add the daily close
	UPDATE #MLDataSet SET
		LastPrice = s.LastPrice
		,ClosePrice = s.ClosePrice
	FROM #MLDataSet ml
	INNER JOIN vwHistoricalStockQuoteXML s WITH(NOLOCK)
		ON ml.Ticker=s.Ticker
		AND ml.[Date]=s.[Date]

	DELETE FROM #MLDataSet WHERE ClosePrice IS NULL

	-- add the close price on expiration
	UPDATE #MLDataSet SET
		TargetPrice = s.ClosePrice
	FROM #MLDataSet ml
	INNER JOIN vwHistoricalStockQuoteXML s WITH(NOLOCK)
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
	INNER JOIN vwImportMaxPain imp WITH(NOLOCK)
		ON ml.Ticker=imp.Ticker
		AND ml.Maturity=imp.Maturity

	-- is success
	-- when stock price closes within walls
	UPDATE #MLDataSet SET
		IsOutside = CASE
			WHEN ClosePrice BETWEEN HighPutOI AND HighCallOI THEN 1
			ELSE 0
		END			
		,IsSuccess = CASE
			WHEN TargetPrice BETWEEN HighPutOI AND HighCallOI THEN 1
			ELSE 0
		END			

	INSERT INTO MLDataSet
	(
		Ticker 
		,Maturity 
		,[Date] 
		,DaysToExp 
		,LastPrice 
		,ClosePrice 
		,TargetPrice 
		,MaxPain 
		,HighCallOI 
		,HighPutOI 
		,TotalCallOI 
		,TotalPutOI 
		,IsOutside 
		,IsSuccess 
	)
	SELECT
		Ticker 
		,Maturity 
		,[Date] 
		,DaysToExp 
		,LastPrice 
		,ClosePrice 
		,TargetPrice 
		,MaxPain 
		,HighCallOI 
		,HighPutOI 
		,TotalCallOI 
		,TotalPutOI 
		,IsOutside 
		,IsSuccess 
	FROM #MLDataSet

	DROP TABLE #MLDataSet

	RETURN 1
END
GO

