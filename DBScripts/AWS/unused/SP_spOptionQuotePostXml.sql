USE [MaxPainAPI]
GO

/****** Object:  StoredProcedure [dbo].[spOptionQuotePostXml]    Script Date: 4/21/2020 10:29:00 AM ******/
DROP PROCEDURE [dbo].[spOptionQuotePostXml]
GO

/****** Object:  StoredProcedure [dbo].[spOptionQuotePostXml]    Script Date: 4/21/2020 10:29:00 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[spOptionQuotePostXml]
	@xml XML
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	This SP is responsible for parsing the YQL Option Data XML and inserting or 
	updating records in the database

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.

	Revision History:

	Date		Name	Description
	----		----	-----------
	2019.05.22	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN
	SET NOCOUNT ON

	DECLARE @CurrentDate SMALLDATETIME
	SELECT @CurrentDate = GETUTCDATE()


	/*
	********* ********* ********* ********* *********
	translate XML into a table var
	********* ********* ********* ********* *********
	*/
	CREATE TABLE #tbl
	(
		OptionTicker	VARCHAR(20)
		,LastPrice		VARCHAR(10)
		,Change			VARCHAR(10)
		,Bid			VARCHAR(10)
		,Ask			VARCHAR(10)
		,Volume			VARCHAR(10)
		,OpenInterest	VARCHAR(10)
		,ImpliedVolatility	VARCHAR(30)
	)

	CREATE NONCLUSTERED	INDEX ix_tblOptionTicker ON #tbl (OptionTicker);

	-- extract (shred) values from XML column nodes
	INSERT INTO #tbl 
	(
		OptionTicker
		,LastPrice		
		,Change
		,Bid			
		,Ask			
		,Volume			
		,OpenInterest
		,ImpliedVolatility	
	)
	SELECT
		n.value('@s[1]','varchar(25)') AS OptionSymbol
		,n.value('@p[1]','varchar(10)') AS LastPrice
		,n.value('@c[1]','varchar(10)') AS Change
		,n.value('@b[1]','varchar(10)') AS Bid
		,n.value('@a[1]','varchar(10)') AS Ask
		,n.value('@v[1]','varchar(10)') AS Volume
		,n.value('@oi[1]','varchar(10)') AS OpenInterest
		,n.value('@iv[1]','varchar(30)') AS ImpliedVolatility
	FROM @xml.nodes('/root/x') x(n)

	/*
	-- remove NaN and replace with zero
	UPDATE #tbl SET
		LastPrice = CASE WHEN LastPrice='NaN' THEN NULL ELSE LastPrice END
		,Change = CASE WHEN Change='NaN' THEN NULL ELSE Change END
		,Bid = CASE WHEN Bid='NaN' THEN NULL ELSE Bid END
		,Ask = CASE WHEN Ask='NaN' THEN NULL ELSE Ask END
		,Volume = CASE WHEN Volume='NaN' THEN NULL ELSE Volume END
		,OpenInterest = CASE WHEN OpenInterest='NaN' THEN NULL ELSE OpenInterest END
		,ImpliedVolatility = CASE WHEN ImpliedVolatility='NaN' THEN NULL ELSE ImpliedVolatility END

	-- remove dash and replace with zero
	UPDATE #tbl SET
		LastPrice = CASE WHEN LastPrice='-' THEN NULL ELSE LastPrice END
		,Change = CASE WHEN Change='-' THEN NULL ELSE Change END
		,Bid = CASE WHEN Bid='-' THEN NULL ELSE Bid END
		,Ask = CASE WHEN Ask='-' THEN NULL ELSE Ask END
		,Volume = CASE WHEN Volume='-' THEN NULL ELSE Volume END
		,OpenInterest = CASE WHEN OpenInterest='-' THEN NULL ELSE OpenInterest END
		,ImpliedVolatility = CASE WHEN ImpliedVolatility='-' THEN NULL ELSE ImpliedVolatility END
	*/

	/*
	********* ********* ********* ********* *********
	find the stock symbol
	********* ********* ********* ********* *********

	DECLARE @tblTickers TABLE (Ticker VARCHAR(10), Records INT)
	INSERT INTO @tblTickers 
	SELECT SUBSTRING(OptionTicker, 1, LEN(OptionTicker)-15), COUNT(*)
	FROM #tbl
	GROUP BY SUBSTRING(OptionTicker, 1, LEN(OptionTicker)-15)

	DECLARE @Ticker VARCHAR(10)
	SELECT @Ticker = (
		SELECT TOP 1 Ticker 
		FROM @tblTickers
		ORDER BY Records DESC 
	)
	*/

	/*
	********* ********* ********* ********* *********
	delete older records
	********* ********* ********* ********* *********
	*/

	DELETE OptionQuote
	FROM OptionQuote
	WHERE ModifiedOn <  dateadd(hh, -2, @CurrentDate)

	/*
	********* ********* ********* ********* *********
	update 
	if option quote for today already exists
	********* ********* ********* ********* *********
	*/
	
	UPDATE [OptionQuote] SET
		LastPrice = x.LastPrice
		,Bid = x.Bid
		,Ask = x.Ask
		,Volume	= x.Volume
		,OpenInterest = x.OpenInterest
		,ImpliedVolatility = CONVERT(FLOAT, x.ImpliedVolatility)
		,ModifiedOn = @CurrentDate	
	FROM #tbl x
	INNER JOIN OptionQuote oq 
		ON x.OptionTicker = oq.OptionTicker
		
	/*
	********* ********* ********* ********* *********
	insert 
	if this is the first time option data was fetched today
	********* ********* ********* ********* *********
	*/
		
	INSERT INTO OptionQuote
	(
		OptionTicker	
		,LastPrice	
		,Bid	
		,Ask	
		,Volume	
		,OpenInterest	
		,ImpliedVolatility
		,ModifiedOn
	)	
	SELECT DISTINCT
		x.OptionTicker
		,x.LastPrice	
		,x.Bid	
		,x.Ask	
		,x.Volume	
		,x.OpenInterest
		,CONVERT(FLOAT, x.ImpliedVolatility)
		,@CurrentDate
	FROM #tbl x
	LEFT JOIN OptionQuote oq 
		ON x.OptionTicker = oq.OptionTicker
	WHERE oq.OptionTicker IS NULL

	/*
	********* ********* ********* ********* *********
	clean up
	********* ********* ********* ********* *********
	*/
	
	DROP TABLE #tbl

	RETURN 1
END
GO


