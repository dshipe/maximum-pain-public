USE [fin]
GO

/****** Object:  StoredProcedure [dbo].[spMPImportPatchVolume]    Script Date: 1/30/2021 10:03:52 AM ******/
DROP PROCEDURE [dbo].[spMPImportPatchVolume]
GO

/****** Object:  StoredProcedure [dbo].[spMPImportPatchVolume]    Script Date: 1/30/2021 10:03:52 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE PROCEDURE [dbo].[spMPImportPatchVolume]
	@ImportDate DATETIME
	,@Ticker VARCHAR(10) = NULL
	,@IncludeReset BIT = 0
	,@IncludeSanityCheck BIT = 0
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	This SP updates the morning volume from the previous night

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.

	Revision History:

	Date		Name	Description
	----		----	-----------
	2012.10.29	DES	Initial Code
********* ********* ********* ********* *********
*/
BEGIN

	SET NOCOUNT ON

	DECLARE @src_Content XML
	DECLARE @dst_Content XML
	DECLARE @src_V INT
	DECLARE @prev_Id INT
	
	DECLARE @cur_Id INT
	DECLARE @cur_Ticker VARCHAR(10)
	DECLARE @cur_CreatedOn DATETIME
	DECLARE @cur_Content XML
	DECLARE @cur_OT VARCHAR(25)
	DECLARE @cur_V INT

	IF OBJECT_ID('tempdb..#PVSource') IS NOT NULL DROP TABLE #PVSource
	CREATE TABLE #PVSource (Ticker VARCHAR(10), ot VARCHAR(25), v INT)
	CREATE CLUSTERED INDEX ix_PVSource ON #PVSource (Ticker, ot)
	INSERT INTO #PVSource (Ticker, ot, v)
	SELECT Ticker, ot, Volume
	FROM vwImportCacheVolume WITH(NOLOCK)
	WHERE ImportDate=@ImportDate
	AND [Hour]>16
	AND (@Ticker IS NULL OR @Ticker=Ticker)

	-- loop through all HistoricalOptionQuoteXML records for the date
	-- this is the 500+ tickers
	
	/*
	DECLARE cur1 CURSOR FOR 
	SELECT Id, Ticker, CreatedOn, NULL, x.value('(@ot)[1]', 'varchar(25)')
	FROM HistoricalOptionQuoteXML hoq WITH(NOLOCK)
	CROSS APPLY hoq.Content.nodes('/OptChn/Options/Opt') AS Content1(x)
	WHERE (@ImportDate IS NULL OR CreatedOn=@ImportDate)
	--AND Ticker='AAPL'
	--AND x.value('(@ot)[1]', 'varchar(25)') IN (
	--	'AAPL210129C00065000'
	--	,'AAPL210129C00070000'
	--	,'AAPL210129C00075000'
	--)
	ORDER BY Ticker
	*/

	DECLARE cur1 CURSOR FOR 
	SELECT hv.Id, hv.Ticker, hv.CreatedOn, NULL, hv.ot, hv.v, src.v
	FROM #PVSource src 
	INNER JOIN [vwHistoryVolume] hv WITH(NOLOCK) ON hv.ot = src.ot
	WHERE hv.CreatedOn = @ImportDate
	AND hv.Ticker = src.Ticker
	AND hv.Volume <> src.v
	ORDER BY hv.Ticker

	OPEN cur1  
	FETCH NEXT FROM cur1 INTO @cur_Id, @cur_Ticker, @cur_CreatedOn, @cur_Content, @cur_OT, @cur_V, @src_V

	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		-- if the Ticker changes
		IF (@prev_Id IS NULL OR @cur_Id <> @prev_Id)
		BEGIN
			PRINT @cur_Ticker

			-- now that we have modified all the option tickers in the JSON
			-- save that JSON back to the HistoricalOptionQuoteXML
			UPDATE HistoricalOptionQuoteXML
				SET Content = @dst_Content
			FROM HistoricalOptionQuoteXML hoq WITH(NOLOCK)
			WHERE hoq.Id = @prev_Id

			-- find the destination JSON
			-- this is the JSON will the volume data will be saved into
			-- capture this in a local variable, so that we are modifying the JSON in-memory
			-- instead of repeatedly updating an XML field in the database
			SELECT @dst_Content = Content
			FROM HistoricalOptionQuoteXML WITH(NOLOCK)
			WHERE Id = @cur_Id

			-- set everything to zero to start
			--DECLARE @Zero INT = 0
			--SET @dst_Content.modify('replace value of (/OptChn/Options/Opt/@v)[1] with sql:variable("@Zero")')
		END

		--PRINT 'ot=' + @cur_OT + ' cur_V=' + CAST( @cur_V AS VARCHAR(10)) + ' src_V=' + CAST( @src_V AS VARCHAR(10));
		SET @dst_Content.modify('replace value of (/OptChn/Options/Opt[@ot=sql:variable("@cur_OT")]/@v)[1] with sql:variable("@src_V")')
			   
		SET @prev_Id = @cur_Id
		FETCH NEXT FROM cur1 INTO @cur_Id, @cur_Ticker, @cur_CreatedOn, @cur_Content, @cur_OT, @cur_V, @src_V
	END 
	
	CLOSE cur1 
	DEALLOCATE cur1 

	IF OBJECT_ID('tempdb..#PVSource') IS NOT NULL DROP TABLE #PVSource

	IF @IncludeSanityCheck=1
	BEGIN
		SELECT
			hoq.Id
			,hoq.Ticker
			,hoq.CreatedOn

			,hoq.ot
			,hoq.LastPrice
			,hoq.Change
			,hoq.Bid
			,hoq.Ask
			,hoq.OpenInterest
			,hoq.Volume
			,hoq.Delta
			,hoq.Gamma
			,hoq.Theta
			,hoq.Vega
			,hoq.Rho	
			,ic.v
		FROM vwHistory hoq WITH(NOLOCK)
		INNER JOIN (
			SELECT ic.Ticker, ic.CreatedOn, ic.CreatedOnEST, ic.ImportDate, ic.[Hour]
				,x.value('(@ot)[1]', 'varchar(25)') as ot
				,x.value('(@p)[1]', 'varchar(25)') as p
				,x.value('(@c)[1]', 'varchar(25)') as c
				,x.value('(@b)[1]', 'varchar(25)') as b
				,x.value('(@a)[1]', 'varchar(25)') as a
				,x.value('(@oi)[1]', 'varchar(25)') as oi
				,x.value('(@v)[1]', 'varchar(25)') as v
				,x.value('(@iv)[1]', 'varchar(25)') as iv
				,x.value('(@de)[1]', 'varchar(25)') as de
				,x.value('(@th)[1]', 'varchar(25)') as th
				,x.value('(@ve)[1]', 'varchar(25)') as ve
				,x.value('(@rh)[1]', 'varchar(25)') as rh
			FROM ImportCache ic
			CROSS APPLY ic.Content.nodes('/OptChn/Options/Opt') AS Content1(x)
		) AS ic
			ON hoq.Ticker=ic.Ticker
			AND hoq.CreatedOn=ic.ImportDate
			AND hoq.ot = ic.ot
			AND ic.[Hour]>16
		WHERE hoq.CreatedOn=@ImportDate
		AND hoq.Ticker='AAPL'
		AND hoq.Volume<>ic.v
	END

	IF OBJECT_ID('tempdb..#PVSource') IS NOT NULL DROP TABLE #PVSource

	RETURN 1
END
GO


