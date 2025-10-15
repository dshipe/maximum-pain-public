USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMPSync]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMPSync]	
GO

CREATE PROCEDURE dbo.spMPSync
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	Copies records from AWS HistoricalOptionQuote to HOME

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.
	
	Revision History:

	Date		Name	Description
	----		----	-----------
	2020.04.18	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN	
	SET NOCOUNT ON

	/*
	-- drop linked server
	IF EXISTS( SELECT * FROM SysServers WHERE SrvName='AWS')
	BEGIN
		EXEC sp_dropserver 'AWS', 'droplogins';
	END
	*/

	-- add linked server
	IF NOT EXISTS( SELECT * FROM SysServers WHERE SrvName='AWS')
	BEGIN
		EXEC sp_addlinkedserver
			@server='AWS'
			,@srvproduct=N''
			,@datasrc='xx-server-xx'
			,@provider=N'SQLNCLI'
	END

	/*
	INSERT INTO HistoricalOptionQuoteXML (
		Ticker
		,CreatedOn
		,Content
	)
	SELECT TOP 1
		src.Ticker
		,src.CreatedOn
		,src.Content
	FROM AWS.MaxPainAPI.dbo.vwHistoricalOptionQuoteNVARCHAR src
	LEFT JOIN HistoricalOptionQuoteXML dst
		ON src.Ticker = dst.Ticker
		AND src.DateOnly = dateadd(dd,0, datediff(dd,0, dst.CreatedOn))
	WHERE dst.Ticker IS NULL
	ORDER BY src.CreatedOn, src.Ticker
	*/

	
	/*
	-- sanity check
	SELECT TOP 1
		src.Ticker
		,src.CreatedOn
		--,src.Content
	FROM AWS.MaxPainAPI.dbo.vwHistoricalOptionQuoteNVARCHAR src
	LEFT JOIN HistoricalOptionQuoteXML dst
		ON src.Ticker = dst.Ticker
		AND src.DateOnly = dateadd(dd,0, datediff(dd,0, dst.CreatedOn))
	WHERE dst.Ticker IS NULL
	ORDER BY src.CreatedOn, src.Ticker
	*/

	SELECT TOP 1
		'INSERT INTO HistoricalOptionQuoteXML (Ticker, CreatedOn, Content) VALUES ('
		+ ' ''' + src.Ticker + ''''
		+ ',''' + CONVERT(VARCHAR(200), src.CreatedOn) + ''''
		+ ',''' + src.Content + ''''
		+ ')'
	FROM AWS.MaxPainAPI.dbo.vwHistoricalOptionQuoteNVARCHAR src
	LEFT JOIN HistoricalOptionQuoteXML dst
		ON src.Ticker = dst.Ticker
		AND src.DateOnly = dateadd(dd,0, datediff(dd,0, dst.CreatedOn))
	WHERE dst.Ticker IS NULL
	ORDER BY src.CreatedOn, src.Ticker



	RETURN 1
END
GO

EXEC spMPSync
GO