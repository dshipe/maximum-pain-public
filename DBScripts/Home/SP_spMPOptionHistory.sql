USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMPOptionHistory]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMPOptionHistory]	
GO

CREATE PROCEDURE dbo.spMPOptionHistory
	@Ticker VARCHAR(10)
	,@Maturity DATETIME = NULL
	,@Strike MONEY = NULL
	,@Debug BIT = 0
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	Gather historical stock & option data

	EXEC spMPOptionHistory @Ticker='AAPL'

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

	IF OBJECT_ID('tempdb..#OH_Market') IS NOT NULL DROP TABLE #OH_Market
	IF OBJECT_ID('tempdb..#OH_Maturities') IS NOT NULL DROP TABLE #OH_Maturities
	
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
	ref
	********* ********* ********* ********* *********
	*/
	CREATE TABLE #OH_Market ( Id BIGINT IDENTITY, Ticker CHAR(10), CreatedOn SMALLDATETIME, Maturity SMALLDATETIME, CallPut CHAR(1), Strike MONEY ) 
	--CREATE CLUSTERED INDEX IDX_OH_REF ON #OH_Market(Id)
	
	SET IDENTITY_INSERT #OH_Market ON

	INSERT INTO #OH_Market(Id, Ticker, CreatedOn) 
	SELECT Id, Ticker, CreatedOn 
	FROM HistoricalOptionQuoteXML hoq WITH(NOLOCK)
	WHERE Ticker = @Ticker
	AND CreatedOn >= DATEADD(dd,-30, GETUTCDATE())

	SET IDENTITY_INSERT #OH_Market OFF

	UPDATE #OH_Market 
		SET Maturity = v.Maturity
		, Strike = v.Strike
		, CallPut = v.CallPut
	FROM #OH_Market m
	INNER JOIN vwHistory v WITH(NOLOCK) ON m.Id=v.Id
		
	/*
	********* ********* ********* ********* *********
	collect data
	********* ********* ********* ********* *********
	*/
	CREATE TABLE #OH_Maturities ( ID INT IDENTITY, Maturity DATETIME ) 

	INSERT INTO #OH_Maturities (Maturity) 
	SELECT DISTINCT TOP 4 m.Maturity 
	FROM #OH_Market m
	WHERE m.Maturity <= DATEADD(dd,1,@Maturity)
	ORDER BY m.Maturity DESC

	IF @Ticker='SPX'
	BEGIN
		DELETE FROM #OH_Maturities WHERE ID>3 
	END

	/*
	FROM #OH_Ref r
	INNER JOIN #OH_Maturities m ON r.Maturity=m.Maturity
	AND r.CreatedOn > DATEADD(dd,-30, r.Maturity)
	AND r.CreatedOn > DATEADD(dd,-30, GETUTCDATE())
	*/

	DELETE
	FROM #OH_Market
	WHERE Maturity NOT IN (
		SELECT Maturity
		FROM #OH_Maturities
	)

	DELETE
	FROM #OH_Market
	WHERE CreatedOn < DATEADD(dd,-30, Maturity)
	--OR CreatedOn < DATEADD(dd,-30, GETUTCDATE())

	IF @Strike IS NOT NULL
	BEGIN
		DELETE FROM #OH_Market
		WHERE Strike <> @Strike
	END

	/*
	********* ********* ********* ********* *********
	results
	********* ********* ********* ********* *********
	*/
	SELECT
		1 as ID
		,(
			SELECT
				v.ot
				,v.LastPrice as lp
				,v.Change as c
				,v.bid as b
				,v.Ask as a
				,v.Volume as v
				,v.OpenInterest as oi
				,v.ImpliedVolatility as iv
				,v.delta as de
				,v.Gamma as ga
				,v.Theta as th
				,v.Vega as ve
				,v.Rho as rh
				,CONVERT(VARCHAR(10), m.CreatedOn, 101) AS d
			FROM #OH_Market m
			INNER JOIN vwHistory v WITH(NOLOCK) ON m.Id=v.Id
			ORDER BY m.Maturity, m.Strike, m.CallPut, m.CreatedOn ASC
			FOR JSON AUTO
		) AS Content	

	--SELECT DISTINCT Maturity FROM #OH_Market ORDER BY Maturity

	DROP TABLE #OH_Market
	DROP TABLE #OH_Maturities

	RETURN 1
END
GO

EXEC spMPOptionHistory @Ticker='GE'--, @Maturity='8/9/2019'
