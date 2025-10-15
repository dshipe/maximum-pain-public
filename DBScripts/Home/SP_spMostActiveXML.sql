USE FIN
GO

IF EXISTS (SELECT * FROM SysObjects WHERE [Name]='spMPMostActiveXML')
BEGIN
	DROP PROCEDURE [dbo].[spMPMostActiveXML]
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spMPMostActiveXML]
	@QueryType VARCHAR(50) = 'ChangePrice'
	,@TickerCount INT = 10
	,@NextMaturity BIT = 0
	,@TruncateMarket BIT = 0
	,@TruncateResult BIT = 0
	,@Debug BIT = 0
	,@ProcessDate SMALLDATETIME = NULL
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	Locate the stock whose option have changed over the last market day

	compare to: http://finance.yahoo.com/options/lists/?mod_id=mediaquotesoptions&tab=tab1&rcnt=50
	
	EXEC spMPMostActive @QueryType='OpenInterestChange', @NextMaturity=1, @Debug=1--, @ProcessDate='4/18/2016'
	EXEC spMPMostActive @QueryType='ChangeOpenInterest'
	EXEC spMPMostActive @QueryType='Volume'
	EXEC spMPMostActive @QueryType='ChangeVolume'
	EXEC spMPMostActive @QueryType='Price'
	EXEC spMPMostActive @QueryType='ChangePrice'

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.
	
	Revision HistoHistoricalOptionQuoteXMLry:

	Date		Name	Description
	----		----	-----------
	2020.30.20	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN	

	SET NOCOUNT ON

	IF OBJECT_ID('tempdb..#MA_MostActiveDates') IS NOT NULL DROP TABLE #MA_MostActiveDates
	IF OBJECT_ID('tempdb..#MA_MostActivePrev') IS NOT NULL DROP TABLE #MA_MostActivePrev
	IF OBJECT_ID('tempdb..#MA_Sort') IS NOT NULL DROP TABLE #MA_Sort


	/*
	********* ********* ********* ********* *********
	Find the most recent date in the database
	also find the day before that so OI and volume changes can be gathered
	********* ********* ********* ********* *********
	*/
	DECLARE @Current SMALLDATETIME
	DECLARE @Maturity SMALLDATETIME
	DECLARE @MostRecentDate SMALLDATETIME
	DECLARE @PriorDate SMALLDATETIME
	DECLARE @Temp SMALLDATETIME
	DECLARE @days INT

	SELECT @Current = GetUTCDate()
	SELECT @Current = dateadd(dd,0, datediff(dd,0, @Current))

	-- ********* ********* ********* ********* *********
	-- find the most recent dates excluding weekends
	CREATE TABLE #MA_MostActiveDates ([Date] SMALLDATETIME)
	INSERT INTO #MA_MostActiveDates
	SELECT DISTINCT TOP 2 dateadd(dd,0, datediff(dd,0, CreatedOn))
	FROM HistoricalOptionQuoteXML
	WHERE datename(DW, CreatedOn) NOT IN ('Saturday', 'Sunday')
	ORDER BY dateadd(dd,0, datediff(dd,0, CreatedOn)) DESC

	SELECT @MostRecentDate=MAX([Date]) FROM #MA_MostActiveDates
	SELECT @PriorDate=MIN([Date]) FROM #MA_MostActiveDates

	IF @TruncateMarket=1
	BEGIN
		IF @Debug=0 TRUNCATE TABLE MostActive

		-- ********* ********* ********* ********* *********
		-- Build a table for all market data
		IF EXISTS(SELECT * FROM SYS.TABLES WHERE [Name]='MostActiveMarket')
		BEGIN
			DROP TABLE MostActiveMarket
		END

		CREATE TABLE MostActiveMarket
		(
			Ticker VARCHAR(10)
			,Maturity SMALLDATETIME
			,CallPut CHAR(1)
			,Strike MONEY
			,LastPrice MONEY
			,Volume INT
			,OpenInterest INT
			,ImpliedVolatility FLOAT NULL
			,[Date] SMALLDATETIME
		)

		CREATE CLUSTERED INDEX IDX_MostActiveMarket ON MostActiveMarket(Maturity, [Date])

		INSERT INTO MostActiveMarket
		SELECT 
			Ticker 
			,Maturity 
			,CallPut 
			,Strike 
			,LastPrice 
			,Volume 
			,OpenInterest 
			,ImpliedVolatility
			,[Date] 
		FROM vwHistory
		WHERE (
			CreatedOn BETWEEN @MostRecentDate AND DATEADD(dd,1,@MostRecentDate)
			OR CreatedOn BETWEEN @PriorDate AND DATEADD(dd,1,@PriorDate)
		)
		AND Maturity>=@Current
		AND (@Debug=0 OR Ticker IN ('AAPL', 'NFLX', 'MSFT', 'BAC', 'GE'))
	END

	SELECT @Maturity = MIN(Maturity) FROM MostActiveMarket WHERE Ticker NOT IN ('SPX','SPXW','QQQ')

	-- ********* ********* ********* ********* *********
	-- Build a table for result
	IF @TruncateMarket=1 OR @TruncateResult=1
	BEGIN
		
		IF EXISTS(SELECT * FROM SYS.TABLES WHERE [Name]='MostActiveResult')
		BEGIN
			DROP TABLE MostActiveResult
		END

		CREATE TABLE MostActiveResult
		(
			Ticker VARCHAR(10)
			,Maturity SmallDateTime
			,CallPut CHAR(1)
			,Strike MONEY
			,CreatedOn SmallDateTime
			,OpenInterest INT
			,Volume INT
			,Price MONEY
			,PrevCreatedOn SmallDateTime
			,PrevOpenInterest INT
			,PrevVolume INT
			,PrevPrice MONEY
			,ChangeOpenInterest MONEY
			,ChangeVolume MONEY
			,ChangePrice MONEY
			,QueryValue MONEY
		)
		
		CREATE CLUSTERED INDEX IDX_MostActiveResult ON MostActiveResult(Ticker, Maturity, CallPut, Strike, CreatedOn)

		INSERT INTO MostActiveResult
		(
			Ticker
			,Maturity
			,CallPut
			,Strike
			,Createdon
			,OpenInterest
			,Volume
			,Price
		)
		SELECT 
			x.Ticker
			,x.Maturity
			,x.CallPut
			,x.Strike
			,x.[Date] AS CreatedOn
			,ISNULL(x.OpenInterest,0)
			,ISNULL(x.Volume,0)
			,ISNULL(x.LastPrice,0)
		FROM MostActiveMarket x
		WHERE x.[Date] = @MostRecentDate
		AND (x.Maturity=@Maturity OR @NextMaturity=0)
			
		CREATE TABLE #MA_MostActivePrev 
		(
			Ticker VARCHAR(10)
			,Maturity SmallDateTime
			,CallPut CHAR(1)
			,Strike MONEY
			,CreatedOn SmallDateTime
			,OpenInterest INT
			,Volume INT
			,Price MONEY
			,PrevCreatedOn SmallDateTime
			,PrevOpenInterest INT
			,PrevVolume INT
			,PrevPrice MONEY
			,ChangeOpenInterest MONEY
			,ChangeVolume MONEY
			,ChangePrice MONEY
			,QueryValue MONEY
		)

		CREATE CLUSTERED INDEX IDX_MostActivePrev ON #MA_MostActivePrev(Ticker, Maturity, CallPut, Strike, CreatedOn)

		INSERT INTO #MA_MostActivePrev
		(
			Ticker
			,Maturity
			,CallPut
			,Strike
			,Createdon
			,OpenInterest
			,Volume
			,Price
		)
		SELECT 
			x.Ticker
			,x.Maturity
			,x.CallPut
			,x.Strike
			,x.[Date] AS CreatedOn
			,ISNULL(x.OpenInterest,0)
			,ISNULL(x.Volume,0)
			,ISNULL(x.LastPrice,0)
		FROM vwHistory x
		WHERE 1=1
		AND x.[Date] = @PriorDate
		AND (x.Maturity=@Maturity OR @NextMaturity=0)

		/*
		********* ********* ********* ********* *********
		using the prior day data, find out was the OI and Volume were before
		and calculate the percent change values
		********* ********* ********* ********* *********
		*/
		UPDATE MostActiveResult
			SET PrevOpenInterest = x.OpenInterest
			,PrevVolume = x.Volume
			,PrevPrice = x.Price
			,PrevCreatedOn = x.CreatedOn
		FROM MostActiveResult m
		INNER JOIN #MA_MostActivePrev x
			on m.Ticker = x.Ticker
			and m.Maturity = x.Maturity
			and m.CallPut = x.CallPut
			and m.Strike = x.Strike
	
		-- Increase = New Number - Original Number
		-- % increase = Increase u Original Number U 100
		UPDATE MostActiveResult SET ChangeOpenInterest=0, ChangeVolume=0, ChangePrice=0
		UPDATE MostActiveResult SET ChangeOpenInterest = ((OpenInterest-PrevOpenInterest)/PrevOpenInterest) WHERE PrevOpenInterest>0
		UPDATE MostActiveResult SET ChangeVolume = ((Volume-PrevVolume)/PrevVolume) WHERE PrevVolume>0
		UPDATE MostActiveResult SET ChangePrice = ((Price-PrevPrice)/PrevPrice) WHERE PrevPrice>0
	END

	IF @Debug = 1
	BEGIN
		SELECT 
			@MostRecentDate as MostRecentDate
			, datename(DW, @MostRecentDate) AS MostRecentDateWkdy
			, @PriorDate AS PriorDate
			, datename(DW, @PriorDate) AS PriorDateWkdy
			, @Current as CurrentDate
			, datename(DW, @Current) AS CurrentDateWkdy
			, @Maturity as Maturity
			, @days as [Days]
			, @NextMaturity as NextMaturity
	END	

	/*
	********* ********* ********* ********* *********
	sort
	1. capture the one value you want to qort by in the QueryValue field
	2. use a Group BY to insert the max QueryValues in descending order into a Sort Table
	3. discard everything but the top X rows
	********* ********* ********* ********* *********
	*/

	UPDATE MostActiveResult 
		SET QueryValue = CASE
			WHEN @QueryType='OpenInterest' THEN OpenInterest
			WHEN @QueryType='ChangeOpenInterest' THEN ChangeOpenInterest
			WHEN @QueryType='Volume' THEN Volume
			WHEN @QueryType='ChangeVolume' THEN ChangeVolume
			WHEN @QueryType='Price' THEN Price
			WHEN @QueryType='ChangePrice' THEN ChangePrice
		END
	

	CREATE TABLE #MA_Sort
	(
		SortID INT IDENTITY
		,Ticker VARCHAR(10)
		,Maturity SMALLDATETIME
		,CallPut CHAR(1)
		,Strike MONEY
		,QueryValue MONEY
	)

	CREATE CLUSTERED INDEX IDX_Sort_ID ON #MA_Sort(SortID)
	CREATE NONCLUSTERED INDEX IDX_Sort_All ON #MA_Sort(Ticker, Maturity, Strike)


	INSERT INTO #MA_Sort (Ticker, Maturity, CallPut, Strike, QueryValue)
	SELECT Ticker, Maturity, CallPut, Strike, MAX(QueryValue) 
	FROM MostActiveResult M
	GROUP BY Ticker, Maturity, CallPut, Strike
	HAVING MAX(QueryValue) > 0 
	ORDER BY MAX(QueryValue) DESC

	DELETE #MA_Sort 
	FROM #MA_Sort S
	WHERE S.SortID > @TickerCount

	--select * from #MA_Sort

	/*
	********* ********* ********* ********* *********
	results
	********* ********* ********* ********* *********
	*/

	IF @Debug=1
		SELECT
			S.SortID
			,@QueryType AS FieldName
			,@NextMaturity AS NextMaturity
			,M.Ticker
			,CONVERT(VARCHAR,M.Maturity,101) AS Maturity
			,M.CallPut
			,M.Strike
			,M.OpenInterest
			,M.PrevOpenInterest
			,M.ChangeOpenInterest
			,M.Volume
			,M.PrevVolume
			,M.ChangeVolume
			,M.Price
			,M.PrevPrice
			,M.ChangePrice
			,CONVERT(VARCHAR,M.CreatedOn,101) AS CreatedOn
		FROM MostActiveResult M
		INNER JOIN #MA_Sort S 
			ON M.Ticker = S.Ticker
			AND M.Strike = S.Strike
			AND M.QueryValue = S.QueryValue
		ORDER BY S.SortID, M.QueryValue, M.Maturity
	ELSE
		INSERT INTO MostActive
		(
			SortID 
			,[Type] 
			,Ticker 
			,Maturity
			,CallPut 
			,Strike 
			,OpenInterest 
			,PrevOpenInterest
			,ChangeOpenInterest
			,Volume
			,PrevVolume
			,ChangeVolume
			,Price
			,PrevPrice
			,ChangePrice
			,CreatedOn
			,NextMaturity
		)
		SELECT
			S.SortID
			,CASE
				WHEN @QueryType='ChangeOpenInterest' THEN 1
				WHEN @QueryType='ChangePrice' THEN 2
				WHEN @QueryType='ChangeVolume' THEN 3
				WHEN @QueryType='OpenInterest' THEN 4
				WHEN @QueryType='Volume' THEN 5
			END
			,M.Ticker
			,CONVERT(VARCHAR,M.Maturity,101) AS Maturity
			,M.CallPut
			,M.Strike
			,M.OpenInterest
			,M.PrevOpenInterest
			,M.ChangeOpenInterest
			,M.Volume
			,M.PrevVolume
			,M.ChangeVolume
			,M.Price
			,M.PrevPrice
			,M.ChangePrice
			,CONVERT(VARCHAR,M.CreatedOn,101) AS CreatedOn
			,@NextMaturity
		FROM MostActiveResult M
		INNER JOIN #MA_Sort S 
			ON M.Ticker = S.Ticker
			AND M.Strike = S.Strike
			AND M.QueryValue = S.QueryValue
		ORDER BY S.SortID, M.QueryValue, M.Maturity


	IF OBJECT_ID('tempdb..#MA_MostActiveDates') IS NOT NULL DROP TABLE #MA_MostActiveDates
	IF OBJECT_ID('tempdb..#MA_MostActivePrev') IS NOT NULL DROP TABLE #MA_MostActivePrev
	IF OBJECT_ID('tempdb..#MA_Sort') IS NOT NULL DROP TABLE #MA_Sort
	
	RETURN 1
END
GO

BEGIN TRANSACTION
EXEC spMPMostActiveXML @Debug=1, @QueryType='ChangePrice', @TickerCount=10, @NextMaturity=1, @TruncateMarket=1
EXEC spMPMostActiveXML @Debug=1, @QueryType='ChangePrice', @TickerCount=10, @TruncateResult=1
SELECT * FROM MostActive
ROLLBACK