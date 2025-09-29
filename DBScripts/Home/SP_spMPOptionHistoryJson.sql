USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMPOptionHistoryJson]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMPOptionHistoryJson]	
GO

CREATE PROCEDURE dbo.spMPOptionHistoryJson
	@Ticker VARCHAR(10)
	,@Maturity DATETIME = NULL
	,@Strike MONEY = NULL
	,@Debug BIT = 0
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	Gather historical stock & option data

	EXEC spMPOptionHistoryJson @Ticker='AAPL'

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.
	
	Revision History:

	Date		Name	Description
	----		----	-----------
	2020.03.20	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN	
	SET NOCOUNT ON

	IF OBJECT_ID('tempdb..#HOQ_Market') IS NOT NULL DROP TABLE #HOQ_Market
	IF OBJECT_ID('tempdb..#HOQ_Maturities') IS NOT NULL DROP TABLE #HOQ_Maturities
	
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

	DECLARE @MostRecentDate SMALLDATETIME
	SELECT @MostRecentDate = MAX(hoq.[CreatedOn]) FROM HistoricalOptionQuoteXML hoq
	SELECT @MostRecentDate = dateadd(dd,0, datediff(dd,0, @MostRecentDate))

	--IF @Maturity IS NULL AND @Ticker='SPX' SELECT @Maturity = DATEADD(d, 100, @MostRecentDate)
	IF @Maturity IS NULL SELECT @Maturity = DATEADD(d, 14, @MostRecentDate)

	--SELECT @Maturity
		
	/*
	********* ********* ********* ********* *********
	collect data
	********* ********* ********* ********* *********
	*/
	CREATE TABLE #HOQ_Maturities ( ID INT IDENTITY, Maturity DATETIME ) 

	INSERT INTO #HOQ_Maturities (Maturity) 
	SELECT DISTINCT TOP 7 Maturity 
	FROM vwHistoricalOptionQuoteXML hoq WITH(NOLOCK)
	WHERE hoq.UnderLyingSymbol = @Ticker
	AND hoq.Maturity <= DATEADD(dd,1,@Maturity)
	ORDER BY Maturity DESC

	IF @Ticker='SPX'
	BEGIN
		DELETE FROM #HOQ_Maturities WHERE ID>5 
	END


	CREATE TABLE #HOQ_Market 
	(
		Ticker VARCHAR(10)
		,Maturity SMALLDATETIME
		,CallPut CHAR(1)
		,Strike MONEY
		,StockPrice MONEY
		,OptionPrice MONEY
		,OpenInterest INT
		,Volume INT
		,IV FLOAT
		,CreatedOn SMALLDATETIME
	)

	CREATE CLUSTERED INDEX IDX_Market_All ON #HOQ_Market(Ticker, Maturity, CreatedOn)

	INSERT INTO #HOQ_Market
	(
		Ticker
		,Maturity
		,CallPut
		,Strike
		,StockPrice
		,OptionPrice
		,OpenInterest
		,Volume
		,IV
		,CreatedOn
	)
	SELECT 
		hoq.UnderLyingSymbol as Ticker
		,hoq.Maturity
		,hoq.CallPut
		,hoq.Strike
		,NULL as StockPrice
		,hoq.LastPrice as OptionPrice
		,hoq.OpenInterest
		,hoq.Volume
		,hoq.ImpliedVolatility
		,hoq.[Date] AS CreatedOn
	FROM vwHistoricalOptionQuoteXML hoq WITH(NOLOCK)
	INNER JOIN #HOQ_Maturities m ON hoq.Maturity=m.Maturity
	WHERE hoq.UnderLyingSymbol = @Ticker
	AND hoq.CreatedOn > DATEADD(dd,-30, hoq.Maturity)
	AND hoq.CreatedOn > DATEADD(dd,-30, GETUTCDATE())
	
	IF @Strike IS NOT NULL
	BEGIN
		DELETE FROM #HOQ_Market
		WHERE Strike <> @Strike
	END

	UPDATE #HOQ_Market
		SET StockPrice=HSQ.ClosePrice
	FROM #HOQ_Market M
	INNER JOIN StockTicker ST WITH(NOLOCK) ON M.Ticker=ST.Ticker
	INNER JOIN HistoricalStockQuote HSQ WITH(NOLOCK)
		ON ST.StockTickerID=HSQ.StockTickerID
		AND HSQ.Date=M.CreatedOn



	/*
	********* ********* ********* ********* *********
	results
	********* ********* ********* ********* *********
	*/
	
	SELECT
		1 as ID
		,(
			SELECT
				Ticker AS TK
				,CONVERT(VARCHAR(10), Maturity, 101) AS M
				,CallPut AS TY
				,Strike AS S
				,StockPrice AS SP
				,OptionPrice AS OP
				,OpenInterest AS OI
				,Volume AS V
				,IV
				,CONVERT(VARCHAR(10), CreatedOn, 101) AS D
			FROM #HOQ_Market
			ORDER BY Maturity, Strike, CallPut, CreatedOn ASC
			FOR JSON AUTO
		) AS Content	

	--SELECT DISTINCT Maturity FROM #HOQ_Market ORDER BY Maturity

	DROP TABLE #HOQ_Market
	DROP TABLE #HOQ_Maturities

	RETURN 1
END
GO

EXEC spMPOptionHistoryJson @Ticker='amzn'--, @Maturity='8/9/2019'
