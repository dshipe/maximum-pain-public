USE Fin
GO

DECLARE
	@Ticker VARCHAR(10)
	,@Maturity DATETIME
	,@Strike MONEY
	,@Debug BIT

SELECT
	@Ticker = 'GE'
	,@Maturity = NULL
	,@Strike = NULL
	,@Debug = 0





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
	Maturity
	********* ********* ********* ********* *********
	*/

	CREATE TABLE #OH_Maturities ( ID INT IDENTITY, Maturity DATETIME ) 


	/*
	INSERT INTO #OH_Maturities (Maturity) 
	SELECT DISTINCT TOP 7 v.Maturity 
	FROM vwHistory v
	WHERE v.Ticker = @Ticker 
	AND v.Maturity <= DATEADD(dd,1,@Maturity)
	ORDER BY v.Maturity DESC

	IF @Ticker='SPX'
	BEGIN
		DELETE FROM #OH_Maturities WHERE ID>5 
	END
	*/

	DECLARE @Past60 DATETIME
	SELECT @Past60 = DATEADD(dd,-60,@Current)

	INSERT INTO #OH_Maturities (Maturity)
	SELECT DATEADD(dd, num, @Past60)
	FROM (
		SELECT TOP 75 num = ROW_NUMBER() OVER(ORDER BY a.[Name])-1
		FROM SysColumns a, SysColumns b
	) AS x
	WHERE DATENAME(weekday, DATEADD(dd, num, @Past60)) = 'Friday'

	DELETE FROM #OH_Maturities WHERE ID>7 

	SELECT * FROM #OH_Maturities

	/*
	********* ********* ********* ********* *********
	ref
	********* ********* ********* ********* *********
	*/
	CREATE TABLE #OH_Market 
	(
		Ticker VARCHAR(10)
		,CreatedOn SMALLDATETIME

		,Maturity SMALLDATETIME
		,CallPut CHAR(1)
		,Strike MONEY
		,StockPrice MONEY

		,ot VARCHAR(30)
		,LastPrice MONEY
		,Change MONEY
		,Bid MONEY
		,Ask MONEY
		,OpenInterest INT
		,Volume INT

		--,ImpliedVolatility FLOAT
		--,Delta FLOAT
		--,Gamma FLOAT
		--,Theta FLOAT
		--,Vega FLOAT
		--,Rho FLOAT
	)

	CREATE CLUSTERED INDEX IDX_Market_All ON #OH_Market(Ticker, Maturity, CreatedOn)


	/*
	SET IDENTITY_INSERT #OH_Market ON

	INSERT INTO #OH_Market(Id, Ticker, CreatedOn) 
	SELECT Id, Ticker, CreatedOn 
	FROM HistoricalOptionQuoteXML hoq
	WHERE Ticker = @Ticker
	AND CreatedOn >= DATEADD(dd,-30, GETUTCDATE())

	SET IDENTITY_INSERT #OH_Market OFF

	UPDATE #OH_Market 
		SET Maturity = v.Maturity
		, Strike = v.Strike
		, CallPut = v.CallPut
		, StockPrice = v.StockPrice
		
		, ot = v.ot
		, LastPrice = v.LastPrice
		, Change = v.Change
		, Bid = v.Bid
		, Ask = v.Ask
		, Volume = v.Volume
		, OpenInterest = v.OpenInterest

		,ImpliedVolatility = v.ImpliedVolatility
		,Delta = v.Delta
		,Gamma = v.Gamma
		,Theta = v.Theta
		,Vega = v.Vega
		,Rho = v.Rho

	FROM #OH_Market m
	INNER JOIN vwHistory v ON m.Id=v.Id

	SET IDENTITY_INSERT #OH_Market OFF
	*/

	INSERT INTO #OH_Market
	(
		Ticker
		,CreatedOn

		,Maturity
		,Strike
		,CallPut
		,StockPrice
		
		,ot
		,LastPrice
		,Change
		,Bid
		,Ask
		,Volume
		,OpenInterest

		--,ImpliedVolatility
		--,Delta
		--,Gamma
		--,Theta
		--,Vega
		--,Rho
	)
	SELECT
		v.Ticker
		,v.CreatedOn

		,v.Maturity
		,v.Strike
		,v.CallPut
		,v.StockPrice
		
		,v.ot
		,v.LastPrice
		,v.Change
		,v.Bid
		,v.Ask
		,v.Volume
		,v.OpenInterest

		--,v.ImpliedVolatility
		--,v.Delta
		--,v.Gamma
		--,v.Theta
		--,v.Vega
		--,v.Rho

	FROM vwHistory v
	INNER JOIN #OH_Maturities m ON v.Maturity=m.Maturity
	WHERE v.Ticker = @Ticker
	AND v.CreatedOn >= DATEADD(dd,-30, GETUTCDATE())
	AND v.CreatedOn > DATEADD(dd,-30, v.Maturity)	


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
				m.ot
				,m.LastPrice as lp
				,m.Change as c
				,m.bid as b
				,m.Ask as a
				,m.Volume as v
				,m.OpenInterest as oi
				--,m.ImpliedVolatility as iv
				--,m.Delta as de
				--,m.Gamma as ga
				--,m.Theta as th
				--,m.Vega as ve
				--,m.Rho as rh
				,CONVERT(VARCHAR(10), m.CreatedOn, 101) AS d
			FROM #OH_Market m
			ORDER BY m.Maturity, m.Strike, m.CallPut, m.CreatedOn ASC
			FOR JSON AUTO
		) AS Content	

	--SELECT DISTINCT Maturity FROM #OH_Market ORDER BY Maturity

	DROP TABLE #OH_Market
	DROP TABLE #OH_Maturities
	
