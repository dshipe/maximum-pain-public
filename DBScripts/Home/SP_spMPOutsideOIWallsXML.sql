USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMPOutsideOIWallsXML]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMPOutsideOIWallsXML]	
GO

CREATE PROCEDURE dbo.spMPOutsideOIWallsXML
	@NextMaturity BIT = 1
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	Locate the stock whose option are outside the next weekly exp OI walls

	compare to: http://finance.yahoo.com/options/lists/?mod_id=mediaquotesoptions&tab=tab1&rcnt=50


	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.


	Revision History:

	Date		Name	Description
	----		----	-----------
	2015.10.04	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN	
	SET NOCOUNT ON

	/*
	********* ********* ********* ********* *********
	Find the most recent date in the database
	also find the day before that so OI and volume changes can be gathered
	********* ********* ********* ********* *********
	*/
	DECLARE @Current SMALLDATETIME
	SELECT @Current = GetUTCDate()
	-- remove timestamp
	SELECT @Current = dateadd(dd,0, datediff(dd,0, @Current))
	--SELECT @Current ='10/30/2015'

	DECLARE @Temp SMALLDATETIME
	SELECT @Temp = DATEADD(dd,1,@Current)
	-- convert to Friday
	DECLARE @days INT
	SELECT @days = 6-DATEPART(dw,@Temp)
	DECLARE @Maturity SMALLDATETIME
	SELECT @Maturity = DATEADD(dd,@days,@Temp)
	-- remove timestamp
	SELECT @Maturity = dateadd(dd,0, datediff(dd,0, @Maturity))
		
	DECLARE @MonthlyExpDate SMALLDATETIME
	SELECT @MonthlyExpDate = dbo.fnGetNthWeekdayOfMonth(@Maturity, 5, 3) -- 5=friday, 3=3rd week of month
	IF @Maturity > @MonthlyExpDate
		SELECT @MonthlyExpDate = dbo.fnGetNthWeekdayOfMonth(DATEADD(mm, 1, @Maturity), 5, 3) 
	DECLARE @IsMonthlyExp BIT
	SELECT @IsMonthlyExp = CASE WHEN @Maturity=@MonthlyExpDate THEN 1 ELSE 0 END

	--SELECT @Maturity as Maturity, @MonthlyExpDate as MonthlyExpDate, @Current as CurrentDate
	
	/*
	********* ********* ********* ********* *********
	collect data
	********* ********* ********* ********* *********
	*/
	CREATE TABLE #Market 
	(
		Ticker VARCHAR(10)
		,Maturity SmallDateTime
		,CallPut CHAR(1)
		,Strike MONEY
		,[Date] SmallDateTime
		,OpenInterest INT
	)
		
	--CREATE CLUSTERED INDEX IDX_Market_All ON #Market(Ticker, Maturity, CallPut, Strike, [Date])

	INSERT INTO #Market
	(
		Ticker
		,Maturity
		,CallPut
		,Strike
		,[Date]
		,OpenInterest
	)
	SELECT 
		v.Ticker
		,v.Maturity
		,v.CallPut
		,v.Strike
		,v.[Date]
		,ISNULL(v.OpenInterest,0)
	FROM vwHistoryRecent v WITH(NOLOCK)
	WHERE v.Maturity>=@Current
	AND (v.Maturity=@Maturity OR v.Maturity=DATEADD(dd,-1,@Maturity) OR @NextMaturity=0)

	/*
	********* ********* ********* ********* *********
	find the OI walls
	********* ********* ********* ********* *********
	*/
	CREATE TABLE #HighOI 
	(
		Ticker VARCHAR(10)
		,Maturity SmallDateTime
		,[Date] SmallDateTime
		,SumOI INT
		,PutOI INT
		,CallOI INT
		,PutStrike MONEY
		,StockPrice MONEY
		,StockDate SmallDateTime
		,CallStrike MONEY
	)

	INSERT INTO #HighOI
	(
		Ticker
		,Maturity
		,[Date]
		,CallOI
	)
	SELECT
		Ticker
		,Maturity
		,[Date]
		,MAX(OpenInterest) AS MaxOI
	FROM #Market
	WHERE CallPut='C'
	GROUP BY Ticker, Maturity, [Date]

	UPDATE #HighOI
		SET CallStrike = M.Strike
	FROM #HighOI H
	INNER JOIN #Market M
		ON H.Ticker = M.Ticker
		AND H.CallOI = M.OpenInterest
		AND M.CallPut='C'

	UPDATE #HighOI
		SET PutOI = X.MaxOI
	FROM #HighOI H
	INNER JOIN (
		SELECT
			Ticker
			,Maturity
			,MAX(OpenInterest) AS MaxOI
		FROM #Market
		WHERE CallPut='P'
		GROUP BY Ticker, Maturity
	) AS X
		ON X.Ticker=H.Ticker

	UPDATE #HighOI
		SET PutStrike = M.Strike
	FROM #HighOI H
	INNER JOIN #Market M
		ON H.Ticker = M.Ticker
		AND H.PutOI = M.OpenInterest
		AND M.CallPut='P'

	UPDATE #HighOI
		SET SumOI = M.SumOI
	FROM #HighOI H
	INNER JOIN (
		SELECT 			
			Ticker
			,Maturity
			,SUM(OpenInterest) AS SumOI
		FROM #Market
		WHERE CallPut='C'
		GROUP BY Ticker, Maturity
	) AS M
		ON H.Ticker = M.Ticker
		AND H.Maturity = M.Maturity


	UPDATE #HighOI
		SET StockPrice = X.lastPrice,
		StockDate = X.[Date]
	FROM #HighOI H
	INNER JOIN vwHistoricalStockQuoteXMLRecentClose X WITH(NOLOCK)
		ON H.Ticker = X.Ticker


	IF OBJECT_ID('TempMarket') IS NOT NULL
		DROP TABLE TempMarket

	/*
	********* ********* ********* ********* *********
	create table to hold result
	********* ********* ********* ********* *********
	*/
	IF OBJECT_ID('OutsideOIWalls') IS NOT NULL
		DROP TABLE OutsideOIWalls

	CREATE TABLE OutsideOIWalls 
	(
		ID INT IDENTITY
		,Ticker VARCHAR(10)
		,[Date] SmallDateTime
		,Maturity CHAR(8)
		,IsMonthlyExp BIT
		,SumOI INT
		,PutOI INT
		,CallOI INT
		,PutStrike MONEY
		,StockPrice MONEY
		,StockDate SmallDateTime
		,CallStrike MONEY
	)

	INSERT INTO OutsideOIWalls
	(
		Ticker 
		,Maturity
		,[Date]
		,IsMonthlyExp
		,SumOI 
		,PutOI 
		,CallOI 
		,PutStrike 
		,StockPrice
		,StockDate 
		,CallStrike 
	)
	SELECT
		Ticker 
		,CONVERT(VARCHAR, Maturity, 1)
		,[Date]
		,@IsMonthlyExp
		,SumOI 
		,PutOI 
		,CallOI 
		,PutStrike 
		,StockPrice
		,StockDate 
		,CallStrike 
	FROM #HighOI
	WHERE (StockPrice<PutStrike OR StockPrice>CallStrike)
	--AND ((@IsMonthlyExp=1 AND SumOI>100000) OR (@IsMonthlyExp=0 AND SumOI>20000))
	ORDER BY Maturity, SumOI DESC, Ticker

	--SELECT * FROM #HighOI --WHERE (StockPrice<PutStrike OR StockPrice>CallStrike)

	DROP TABLE #Market
	DROP TABLE #HighOI

	SELECT 
		Ticker 
		,Maturity
		,[Date]
		,IsMonthlyExp
		,SumOI 
		,PutOI 
		,CallOI 
		,PutStrike 
		,StockPrice 
		,StockDate
		,CallStrike 
	FROM OutsideOIWalls



	RETURN 1
END
GO

BEGIN TRANSACTION
EXEC spMPOutsideOIWallsXML
ROLLBACK
