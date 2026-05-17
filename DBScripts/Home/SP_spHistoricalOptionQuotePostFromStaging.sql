DROP PROC [spHistoricalOptionQuotePostFromStaging]
GO

-- EXEC spHistoricalOptionQuotePostFromStaging @AllowUpdate=1
CREATE PROCEDURE [dbo].[spHistoricalOptionQuotePostFromStaging]
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
	2012.10.29	DES	    Initial Code
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

	DECLARE @CurrentDate DATETIME = CONVERT(DATETIME, FORMAT(GETUTCDATE(), 'yyyy-MM-dd HH:00:00')) 
	DECLARE @CurrentDateEST DATETIME = CONVERT(DATETIME, SWITCHOFFSET(@CurrentDate, DATEPART(TZOFFSET, @CurrentDate AT TIME ZONE 'Eastern Standard Time')))
	DECLARE @MidnightEST DATETIME = dateadd(dd,0, datediff(dd,0, @CurrentDateEST))
	
	--DELETE FROM ImportCache WHERE CreatedOn<DATEADD(dd,-14,@CurrentDate)

	UPDATE ImportStaging SET CreatedOn = @CurrentDate

	INSERT INTO [ImportCache] 
	(
		Ticker	
		,CreatedOn
		,ImportDate
		,Content
	)	
	SELECT 
		s.Ticker	
		,s.CreatedOn
		,s.ImportDate
		,s.Content
	FROM ImportStaging s WITH(NOLOCK)
	
	--SELECT Id, Ticker, CreatedOn, CreatedOnEST, Hour, ImportDate FROM ImportCache

	/*
	********* ********* ********* ********* *********
	update 
	if option quote for today already exists
	********* ********* ********* ********* *********
	*/

	DECLARE @MaxCreatedOn SMALLDATETIME = (SELECT MAX(CreatedOn) FROM ImportCache)
	
	IF @AllowUpdate = 1
	BEGIN
		UPDATE [HistoricalOptionQuote] SET
			Content = x.Content
			,CreatedOn = x.ImportDate
		FROM HistoricalOptionQuote hoq WITH(NOLOCK) 
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

	INSERT INTO [HistoricalOptionQuote] 
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
	LEFT JOIN HistoricalOptionQuote hoq WITH(NOLOCK)
		ON x.Ticker = hoq.Ticker
		AND x.ImportDate = hoq.CreatedOn
	WHERE x.CreatedOn = @MaxCreatedOn
	AND hoq.Ticker IS NULL

	SELECT @Inserted = @@ROWCOUNT
	
	-- TRUNCATE TABLE ImportStaging
	
	SELECT @Updated AS Updated, @Inserted AS Inserted, @Updated+@Inserted AS Total

	RETURN 1
END