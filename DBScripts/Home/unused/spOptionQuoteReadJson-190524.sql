USE MaxPainAPI
GO

/*
SELECT compatibility_level FROM sys.databases WHERE name = 'MaxPainAPI';
--ALTER DATABASE MaxPain SET COMPATIBILITY_LEVEL = 100;  
ALTER DATABASE MaxPainAPI SET COMPATIBILITY_LEVEL = 130;  
GO
*/  

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spOptionQuoteReadJson]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spOptionQuoteReadJson]	
GO


CREATE PROCEDURE spOptionQuoteReadJson
	@ticker varchar(5)
	,@maturity datetime = NULL
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

	SELECT *
	FROM vwOptionQuote
	WHERE Ticker = @ticker
	AND (@maturity IS NULL OR Maturity=@maturity)
	FOR JSON AUTO

	RETURN 1
END
GO

	
EXEC spOptionQuoteReadJson @ticker='SPX'
