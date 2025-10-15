USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMPHistoricalMaxPainJson]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMPHistoricalMaxPainJson]	
GO


CREATE PROCEDURE dbo.spMPHistoricalMaxPainJson
	@Ticker VARCHAR(10)
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	Locate the stocks with high number of open call contracts

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.

	spMPHistoricalMaxPain @Ticker='AAPL'

	Revision History:

	Date		Name	Description
	----		----	-----------
	2020.03.20	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN

	SET NOCOUNT ON

	IF OBJECT_ID('tempdb..#HMP_Market') IS NOT NULL DROP TABLE #HMP_Market

	CREATE TABLE #HMP_Ticker
	(
		Ticker VARCHAR(10)
		,Maturity SMALLDATETIME
		,CreatedOn SMALLDATETIME
		,Content XML
	)

	INSERT INTO #HMP_Ticker (ticker, maturity, createdOn, content)
	SELECT Ticker, CONVERT(SMALLDATETIME, '20' + SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-14, 6), 103), CreatedOn, Content
	FROM HistoricalOptionQuoteXML
	CROSS APPLY Content.nodes('/root/x') as n(x)
	WHERE Ticker = @Ticker
	
	DELETE FROM #HMP_Ticker
	WHERE Maturity < DATEADD(d,-45,GETUTCDATE()) 
	OR Maturity > DATEADD(d,14,GETUTCDATE()) 
	


	IF OBJECT_ID('tempdb..#HMP_Market') IS NOT NULL DROP TABLE #HMP_Market
	IF OBJECT_ID('tempdb..#HMP_Stock') IS NOT NULL DROP TABLE #HMP_Stock
	IF OBJECT_ID('tempdb..#HMP_CashValue') IS NOT NULL DROP TABLE #HMP_CashValue
	IF OBJECT_ID('tempdb..#HMP_MaxPain') IS NOT NULL DROP TABLE #HMP_MaxPain
	IF OBJECT_ID('tempdb..#HMP_HighCall') IS NOT NULL DROP TABLE #HMP_HighCall
	IF OBJECT_ID('tempdb..#HMP_HighPut') IS NOT NULL DROP TABLE #HMP_HighPut
	IF OBJECT_ID('tempdb..#HMP_Result') IS NOT NULL DROP TABLE #HMP_Result
	IF OBJECT_ID('tempdb..#HMP_Summary') IS NOT NULL DROP TABLE #HMP_Summary
	IF OBJECT_ID('tempdb..#HMP_Maturities') IS NOT NULL DROP TABLE #HMP_Maturities
	
	-- ********* ********* ********* ********* *********
	-- collect data
	IF NOT EXISTS(SELECT * FROM SysObjects WHERE name='HistoricalMaxPainStaging')
	BEGIN
		CREATE TABLE HistoricalMaxPainStaging 
		(
			Maturity SMALLDATETIME
			,[Date] SMALLDATETIME
			,CallPut CHAR(1)
			,Strike FLOAT
			,OpenInterest INT
		)

		CREATE CLUSTERED INDEX [HistoricalMaxPainStaging]
		ON HistoricalMaxPainStaging ([Maturity])
	END

	TRUNCATE TABLE HistoricalMaxPainStaging

	INSERT INTO HistoricalMaxPainStaging WITH (TABLOCK)
	(
		Maturity
		,[Date]
		,CallPut
		,Strike
		,OpenInterest
	)
	SELECT 
		t.Maturity
		,dateadd(dd,0, datediff(dd,0,t.CreatedOn))
		,SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-8, 1)
		,CONVERT(MONEY, SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-7, 8))/1000.0
		,n.x.value('@oi','INT') AS OpenInterest
	FROM #HMP_Ticker t
	CROSS APPLY Content.nodes('/root/x') as n(x)
	--WHERE CONVERT(SMALLDATETIME, '20' + SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-14, 6), 103) BETWEEN DATEADD(d,-45,GETUTCDATE()) AND DATEADD(d,14,GETUTCDATE()) 
	

	-- ********* ********* ********* ********* *********
	-- maturities
	CREATE TABLE #HMP_Maturities(Maturity SMALLDATETIME)
		
	CREATE CLUSTERED INDEX [Maturities_Maturity]
	ON #HMP_Maturities ([Maturity])

	INSERT INTO #HMP_Maturities (Maturity)
	SELECT DISTINCT TOP 7 Maturity
	FROM HistoricalMaxPainStaging
	--WHERE Maturity <= GETUTCDATE()
	ORDER BY Maturity DESC

	DELETE HistoricalMaxPainStaging
	FROM HistoricalMaxPainStaging s
	LEFT JOIN #HMP_Maturities m ON s.Maturity=m.Maturity 
	WHERE m.Maturity IS NULL

	DELETE FROM HistoricalMaxPainStaging WHERE [Date] < DATEADD(dd,-14,Maturity)

	-- ********* ********* ********* ********* *********
	-- find the closing stock price
	CREATE TABLE #HMP_Stock 
	(
		ID INT IDENTITY
		,Ticker VARCHAR(10)
		,[Date] SMALLDATETIME
		,ClosePrice FLOAT
		,PrevClose FLOAT
	)

	DECLARE @MinDate AS SMALLDATETIME
	DECLARE @MaxDate AS SMALLDATETIME
	SELECT @MinDate = MIN([Date]) FROM HistoricalMaxPainStaging
	SELECT @MaxDate = MAX([Date]) FROM HistoricalMaxPainStaging

	INSERT INTO #HMP_Stock (Ticker, [Date], ClosePrice)
	SELECT DISTINCT HSQ.Ticker, HSQ.[Date], HSQ.LastPrice
	FROM vwHistoricalStockQuoteXMLClose HSQ
	WHERE HSQ.Ticker = @Ticker		
	AND HSQ.[Date] BETWEEN DATEADD(dd,-7,@MinDate) AND @MaxDate
	ORDER BY HSQ.[Date] 

	UPDATE #HMP_Stock
		SET PrevClose=P.ClosePrice
	FROM #HMP_Stock S
	INNER JOIN (
		SELECT ID, ClosePrice
		FROM #HMP_Stock
	) AS P ON S.ID=P.ID+1

	-- ********* ********* ********* ********* *********
	-- temp tables
	CREATE TABLE #HMP_Market 
	(
		[Date] SMALLDATETIME
		,CallPut CHAR(1)
		,Strike FLOAT
		,OpenInterest INT
		,PrevClose FLOAT
	)

	CREATE TABLE #HMP_CashValue
	(
		[Date] SMALLDATETIME
		,Strike FLOAT
		,StockPrice FLOAT
		,CallOI INT
		,PutOI INT
		,CallIntrinsic FLOAT
		,PutIntrinsic FLOAT
		,CallCash FLOAT
		,PutCash FLOAT
	)

	CREATE CLUSTERED INDEX [CashValue_DateStrike]
	ON #HMP_CashValue ([Date],[Strike])

	CREATE NONCLUSTERED INDEX [CashValue_Date_CallOI]
	ON  #HMP_CashValue ([Date],[CallOI])
	INCLUDE ([Strike])

	CREATE NONCLUSTERED INDEX [CashValue_Date_PutOI]
	ON  #HMP_CashValue ([Date],[PutOI])
	INCLUDE ([Strike])
	
	CREATE TABLE #HMP_MaxPain
	(
		[Date] SMALLDATETIME
		,StockPrice FLOAT
		,CallContracts INT
		,PutContracts INT
		,TotalContracts INT
		,CallCash FLOAT
		,PutCash FLOAT
		,TotalCash FLOAT
	)
	
	CREATE TABLE #HMP_HighCall ([Date] SMALLDATETIME, OI INT, Strike FLOAT)
	CREATE TABLE #HMP_HighPut ([Date] SMALLDATETIME, OI INT, Strike FLOAT)

	CREATE TABLE #HMP_Result 
	(
		[Date] SMALLDATETIME
		,Strike FLOAT
		,TotalCash FLOAT	
	)

	CREATE TABLE #HMP_Summary 
	(
		Ticker VARCHAR(10)
		,Maturity VARCHAR(10)
		,[Date] VARCHAR(10)
		,StockPrice FLOAT
		,CallOI FLOAT
		,PutOI FLOAT
		,ClosePrice FLOAT
		,CallContracts INT
		,PutContracts INT
	)

	DECLARE @Maturity SMALLDATETIME
	DECLARE cur CURSOR
    FOR SELECT Maturity FROM #HMP_Maturities
	OPEN cur
	FETCH NEXT FROM cur INTO @Maturity

	WHILE @@FETCH_STATUS = 0  
    BEGIN
		TRUNCATE TABLE #HMP_Market
		TRUNCATE TABLE #HMP_CashValue
		TRUNCATE TABLE #HMP_MaxPain
		TRUNCATE TABLE #HMP_HighCall
		TRUNCATE TABLE #HMP_HighPut
		TRUNCATE TABLE #HMP_Result
		
		-- ********* ********* ********* ********* *********
		-- find the option data
		INSERT INTO #HMP_Market
		(
			[Date]
			,CallPut
			,Strike
			,OpenInterest
		)
		SELECT 
			[Date]
			,CallPut
			,Strike
			,OpenInterest
		FROM HistoricalMaxPainStaging
		WHERE Maturity = @Maturity

		UPDATE #HMP_Market
			SET PrevClose = S.PrevClose
		FROM #HMP_Market M
		INNER JOIN #HMP_Stock S ON M.[Date] = S.[Date]

		-- ********* ********* ********* ********* *********
		-- find the cash value 
		INSERT INTO #HMP_CashValue ([Date], Strike, StockPrice, CallOI, PutOI)
		SELECT DISTINCT m.[Date], m.Strike, x.StockPrice, 0, 0 
		FROM #HMP_Market m
		CROSS JOIN (
			SELECT DISTINCT Strike AS StockPrice
			FROM #HMP_Market
		) AS X
		ORDER BY m.[Date], m.Strike, x.StockPrice 

		UPDATE #HMP_CashValue SET
			CallOI=m.OpenInterest
		FROM #HMP_CashValue cv
		INNER JOIN #HMP_Market m
			ON cv.[Date]=m.[Date]
			AND cv.Strike=m.Strike
			AND m.CallPut='C'

		UPDATE #HMP_CashValue SET
			PutOI=m.OpenInterest
		FROM #HMP_CashValue cv
		INNER JOIN #HMP_Market m
			ON cv.[Date]=m.[Date]
			AND cv.Strike=m.Strike
			AND m.CallPut='P'

		UPDATE #HMP_CashValue SET
			CallIntrinsic = CASE
				WHEN StockPrice - Strike <= 0 THEN 0
				ELSE (StockPrice - Strike) 
			END 
			,PutIntrinsic = CASE
				WHEN Strike - StockPrice <= 0 THEN 0
				ELSE (Strike - StockPrice)
			END

		UPDATE #HMP_CashValue SET
			CallCash=CallOI*100*CallIntrinsic
			,PutCash=PutOI*100*PutIntrinsic
		
		-- ********* ********* ********* ********* *********
		-- sum up the cash value
		INSERT INTO #HMP_MaxPain
		(
			[Date] 
			,StockPrice
			,CallContracts 
			,PutContracts 
			,TotalContracts 
			,CallCash 
			,PutCash 
			,TotalCash 
		)		
		SELECT 
			[Date] 
			,StockPrice
			,SUM(CallOI)
			,SUM(PutOI)
			,SUM(CallOI) + SUM(PutOI)
			,SUM(CallCash)
			,SUM(PutCash) 
			,SUM(CallCash) + SUM(PutCash)
		FROM #HMP_CashValue
		GROUP BY 
			[Date] 
			,StockPrice

		--SELECT * FROM #HMP_MaxPain WHERE [Date]='7/13/2015' ORDER BY [Date], StockPrice
		--spMPHistoricalMaxPain
	
		-- ********* ********* ********* ********* *********
		-- results
		INSERT INTO #HMP_Result
		(
			[Date]
			,TotalCash	
		)
		SELECT
			[Date]
			,MIN(TotalCash) as TotalCash
		FROM #HMP_MaxPain
		WHERE TotalCash>0
		GROUP BY [Date]
	
		INSERT INTO #HMP_HighCall([Date],OI,Strike)
		SELECT  
			[Date]
			,MAX(CallOI)
			,0
		FROM #HMP_CashValue
		GROUP BY [Date]		

		UPDATE #HMP_HighCall SET
			Strike = cv.Strike
		FROM #HMP_HighCall x
		INNER JOIN #HMP_CashValue cv
			ON x.[Date]=cv.[Date]
			AND x.OI=cv.CallOI
		WHERE x.OI>0
				
		INSERT INTO #HMP_HighPut([Date],OI,Strike)
		SELECT
			[Date]
			,MAX(PutOI)
			,0
		FROM #HMP_CashValue
		GROUP BY [Date]		

		UPDATE #HMP_HighPut SET
			Strike = cv.Strike
		FROM #HMP_HighPut x
		INNER JOIN #HMP_CashValue cv
			ON x.[Date]=cv.[Date]
			AND x.OI=cv.PutOI
		WHERE x.OI>0


		INSERT INTO #HMP_Summary
		(
			Ticker 
			,Maturity 
			,[Date] 
			,StockPrice 
			,CallOI
			,PutOI
			,ClosePrice
			,CallContracts
			,PutContracts
		)
		SELECT DISTINCT
			@Ticker AS Ticker
			,CONVERT(VARCHAR(10),@Maturity,101) AS Maturity
			,CONVERT(VARCHAR(10),MP.[Date],101) AS [Date]
			,MP.StockPrice AS MaxPain
			,C.Strike AS CallOI
			,P.Strike AS PutOI
			,S.ClosePrice
			,MP.CallContracts
			,MP.PutContracts
		FROM #HMP_MaxPain MP
		INNER JOIN #HMP_Result R 
			ON MP.[Date]=R.[Date]
			AND MP.TotalCash=R.TotalCash
		LEFT JOIN #HMP_HighCall C
			ON C.[Date]=MP.[Date]
		LEFT JOIN #HMP_HighPut P
			ON P.[Date]=MP.[Date]
		INNER JOIN #HMP_Stock S
			ON S.[Date]=MP.[Date]
		ORDER BY CONVERT(VARCHAR(10),MP.[Date],101)

		FETCH NEXT FROM cur INTO @Maturity
    END
	CLOSE cur
	DEALLOCATE cur


	SELECT
		1 as ID
		,(
			SELECT 
				Ticker AS TK
				,Maturity AS M
				,[Date] AS D
				,StockPrice AS MP
				,CallOI AS COI
				,PutOI AS POI
				,ClosePrice AS SP
				,CallContracts AS CC
				,PutContracts AS PC
			FROM #HMP_Summary
			ORDER BY Maturity,[Date]
			FOR JSON AUTO
		) AS Content

	DROP TABLE #HMP_Market
	DROP TABLE #HMP_Stock
	DROP TABLE #HMP_CashValue
	DROP TABLE #HMP_MaxPain
	DROP TABLE #HMP_HighCall
	DROP TABLE #HMP_HighPut
	DROP TABLE #HMP_Result
	DROP TABLE #HMP_Summary
	DROP TABLE #HMP_Maturities
	
	
	RETURN 1
END
GO

EXEC spMPHistoricalMaxPainJson @Ticker='aapl'
GO
