USE Fin
GO

/****** Object:  StoredProcedure [dbo].[spHistoricalOptionQuoteXMLPostFromStaging]    Script Date: 3/12/2020 10:09:13 PM ******/
DROP PROCEDURE [dbo].[spHistoricalOptionQuoteXMLPostFromStaging]
GO

/****** Object:  StoredProcedure [dbo].[spHistoricalOptionQuoteXMLPostFromStaging]    Script Date: 3/12/2020 10:09:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spHistoricalOptionQuoteXMLPostFromStaging]
	@AllowUpdate BIT = 1
	,@ImportDate DATETIME = NULL
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	This SP is responsible for parsing the option data and inserting or 
	updating records in the database

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

	DECLARE @Updated INT = 0
	DECLARE @Inserted INT = 0

	/*
	********* ********* ********* ********* *********
	cache 
	********* ********* ********* ********* *********
	*/

	DECLARE @CurrentDate DATETIME = GETUTCDATE()
	DECLARE @CurrentDateEST DATETIME = CONVERT(DATETIME, SWITCHOFFSET(@CurrentDate, DATEPART(TZOFFSET, @CurrentDate AT TIME ZONE 'Eastern Standard Time')))
	DECLARE @MidnightEST DATETIME = dateadd(dd,0, datediff(dd,0, @CurrentDateEST))

	DELETE FROM ImportCache WHERE CreatedOn<DATEADD(dd,-14,@CurrentDate)
	
	INSERT INTO [ImportCache] 
	(
		Ticker	
		,CreatedOn
		,ImportDate
		,Content
	)	
	SELECT 
		x.Ticker	
		,x.CreatedOn
		,x.ImportDate
		,x.Content
	FROM ImportStaging x WITH(NOLOCK)

	UPDATE ImportCache 
	SET CreatedOnEST = CONVERT(DATETIME, SWITCHOFFSET(CreatedOn, DATEPART(TZOFFSET, CreatedOn AT TIME ZONE 'Eastern Standard Time')))
	WHERE CreatedOnEST IS NULL
	
	UPDATE ImportCache 
	SET [Hour] = DATEPART(HOUR, CreatedOnEST)
	WHERE [Hour] IS NULL

	-- SELECT Id, Ticker, CreatedOn, CreatedOnEST, Hour, ImportDate FROM ImportCache

	/*
	********* ********* ********* ********* *********
	update 
	if option quote for today already exists
	********* ********* ********* ********* *********
	*/

	DECLARE @MaxCreatedOn SMALLDATETIME = (SELECT MAX(CreatedOn) FROM ImportCache)
	
	IF @AllowUpdate = 1
	BEGIN
		UPDATE [HistoricalOptionQuoteXML] SET
			Content = x.Content
			,CreatedOn = x.ImportDate
		FROM HistoricalOptionQuoteXML hoq WITH(NOLOCK) 
		INNER JOIN ImportCache x WITH(NOLOCK)
			ON x.Ticker = hoq.Ticker
			AND x.ImportDate = hoq.CreatedOn
		WHERE x.CreatedOn = @MaxCreatedOn

		SELECT @Updated = @@ROWCOUNT
	END
		
	/*
	********* ********* ********* ********* *********
	insert 
	if this is the first time option data was fetched today
	********* ********* ********* ********* *********
	*/

	INSERT INTO [HistoricalOptionQuoteXML] 
	(
		Ticker
		,CreatedOn
		,Content
	)	
	SELECT 
		x.Ticker	
		,x.ImportDate
		,x.Content
	FROM ImportCache x WITH(NOLOCK)
	LEFT JOIN HistoricalOptionQuoteXML hoq WITH(NOLOCK)
		ON x.Ticker = hoq.Ticker
		AND x.ImportDate = hoq.CreatedOn
	WHERE x.CreatedOn = @MaxCreatedOn
	AND hoq.Ticker IS NULL

	SELECT @Inserted = @@ROWCOUNT
	
	-- TRUNCATE TABLE ImportStaging
	
	SELECT @Updated AS Updated, @Inserted AS Inserted, @Updated+@Inserted AS Total

	RETURN 1
END
GO

/*
********* ********* ********* ********* *********
testing
********* ********* ********* ********* *********
INSERT INTO [ImportCache] 
(
	Ticker	
	,CreatedOn
	,Content
)	
SELECT 
	x.Ticker	
	,x.CreatedOn
	,x.Content
FROM HistoricalOptionQuoteXML x
WHERE x.CreatedOn>='2021-01-13'

INSERT INTO [ImportCache] 
(
	Ticker	
	,CreatedOn
	,Content
)	
SELECT 
	x.Ticker	
	,DATEADD(hh, 1, CreatedOn)
	,x.Content
FROM HistoricalOptionQuoteXML x
WHERE x.CreatedOn>='2021-01-13'
*/

/*
BEGIN TRANSACTION
EXEC spHistoricalOptionQuoteXMLPostFromStaging

SELECT x.Id, x.Ticker, x.CreatedOn, x.CreatedOnEST, x.[Hour], x.ImportDate
FROM ImportCache x
INNER JOIN (
	SELECT MAX(CreatedOn) AS CreatedOn
	FROM ImportCache
) AS mx
	ON x.CreatedOn = mx.CreatedOn	

ROLLBACK
*/
