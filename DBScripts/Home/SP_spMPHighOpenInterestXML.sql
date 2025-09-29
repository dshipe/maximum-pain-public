USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMPHighOpenInterestJson]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMPHighOpenInterestJson]	
GO



CREATE PROCEDURE dbo.spMPHighOpenInterestJson
	@Maturity DATETIME = NULL --'7/18/2014'
	,@MinContracts INT = 50000
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

	spMPHighOpenInterest2 @Maturity='7/10/2015'
	spMPHighOpenInterest2 @maturity='7/31/2015'

	Revision History:

	Date		Name	Description
	----		----	-----------
	2014.07.10	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN

	/*
	DROP INDEX [idxOptionQuote_OptionSymbolID] ON [OptionQuote]
	DROP INDEX [idxOptionQuote_OptionSymbolID2] ON [OptionQuote]
	DROP INDEX [idxOptionQuote_OptionSymbolID_OpenInterest_ModifiedOn] ON [OptionQuote]
	
	CREATE INDEX [idxOptionQuote_OptionSymbolID] ON [OptionQuote] ([OptionSymbolID]) INCLUDE ([ModifiedOn])
	CREATE INDEX [idxOptionQuote_OptionSymbolID2] ON [OptionQuote] ([OptionSymbolID])
	CREATE INDEX [idxOptionQuote_OptionSymbolID_OpenInterest_ModifiedOn] ON [OptionQuote] ([OptionSymbolID],[OpenInterest], [ModifiedOn])
	*/
	
	SET NOCOUNT ON
	
	IF @Maturity IS NULL 
		SELECT @Maturity = GetUTCDate()
	-- convert to Friday
	SELECT @Maturity = DATEADD(d,6-DATEPART(dw,@Maturity),@Maturity)
	-- remove timestamp
	SELECT @Maturity = dateadd(dd,0, datediff(dd,0, @Maturity))
	--SELECT @Maturity

	/*
	http://weblogs.sqlteam.com/peterl/archive/2009/06/17/How-to-get-the-Nth-weekday-of-a-month.aspx
	*/
	DECLARE @MonthlyExpDate SMALLDATETIME
	SELECT @MonthlyExpDate = dbo.fnGetNthWeekdayOfMonth(@Maturity, 5, 3) -- 5=friday, 3=3rd week of month
	--SELECT @MonthlyExpDate

	DECLARE @Market TABLE
	(
		ID BIGINT IDENTITY
		,Ticker VARCHAR(10)
		,Maturity DateTime
		,CallPut CHAR(1)
		,CreatedOn DateTime
		,OpenInterest INT
		,IsValid BIT
	)

	INSERT INTO @Market
	(
		Ticker
		,Maturity
		,CallPut
		,Createdon
		,OpenInterest
	)
	SELECT 
		UnderLyingSymbol as Ticker
		, CONVERT(VARCHAR,Maturity,101) AS Maturity
		, CallPut
		, CONVERT(VARCHAR,[Date],101) AS CreatedOn
		, SUM(OpenInterest) as OpenInterest
	FROM vwHistoricalOptionQuote WITH(NOLOCK)
	WHERE 1=1
	AND CallPut='C'
	AND Maturity = @Maturity
	AND [Date] BETWEEN DateAdd(dd, -14, Maturity) AND DateAdd(dd, 7, Maturity)
	GROUP BY UnderLyingSymbol, Maturity, [Date], CallPut
	HAVING SUM(OpenInterest) >= @MinContracts
	ORDER BY UnderLyingSymbol, Maturity, [Date], CallPut

	UPDATE @Market 
		SET IsValid = CASE
			WHEN @Maturity=@MonthlyExpDate AND OpenInterest >= (@MinContracts * 4) THEN 1
			WHEN @Maturity<>@MonthlyExpDate AND OpenInterest >= (@MinContracts * 1) THEN 1
			ELSE 0
		END
	
	DECLARE @Sort TABLE
	(
		SortID INT IDENTITY
		,Ticker VARCHAR(10)
	)

	INSERT INTO @Sort (Ticker)
	SELECT Ticker 
	FROM @Market M
	WHERE IsValid=1
	ORDER BY OpenInterest DESC

	DELETE @Sort 
	FROM @Sort S
	LEFT JOIN (
		SELECT Ticker, MIN(SortID) AS SortID
		FROM @Sort
		GROUP BY Ticker
	) AS X ON S.SortID=X.SortID
	WHERE X.SortID IS NULL
	
	
	
	DECLARE @tblResult TABLE
	(
		ID INT
		,SortID INT
		,Ticker VARCHAR(10)
		,Maturity DATETIME
		,Weekdy VARCHAR(10)
		,OptionType VARCHAR(100)
		,CreatedOn DATETIME
		,OpenInterest INT
	)

	INSERT INTO @tblResult
	(
		SortID
		,Ticker
		,Maturity
		,Weekdy
		,OptionType
		,CreatedOn
		,OpenInterest
	)	
	SELECT
		S.SortID
		,M.Ticker
		,CONVERT(VARCHAR,M.Maturity,101) AS Maturity
		,DATEPART(dw, M.Maturity) AS weekdy
		,CASE 
			WHEN @Maturity=@MonthlyExpDate THEN 'monthly'
			ELSE 'weekly'
		END as OptionType
		,CONVERT(VARCHAR,M.CreatedOn,101) AS CreatedOn
		,M.OpenInterest
	FROM @Market M
	INNER JOIN @Sort S ON M.Ticker = S.Ticker
	WHERE IsValid=1
	ORDER BY SortID, CreatedOn
	
	/*	
	SELECT
		SortID
		,Ticker
		,Maturity
		,Weekdy
		,OptionType
		,CreatedOn
		,OpenInterest
	FROM @tblResult
	FOR JSON AUTO
	*/

	SELECT 
		1 AS ID
		,(
			SELECT
				SortID
				,Ticker
				,Maturity
				,Weekdy
				,OptionType
				,CreatedOn
				,OpenInterest
			FROM @tblResult
			FOR JSON AUTO
		) AS Content


	RETURN 1
END
GO

EXEC spMPHighOpenInterestJson @Maturity='6/7/2019'
GO
