USE [MaxPainAPI]
GO

/****** Object:  StoredProcedure [dbo].[spOptionQuoteReadJson]    Script Date: 4/21/2020 10:28:05 AM ******/
DROP PROCEDURE [dbo].[spOptionQuoteReadJson]
GO

/****** Object:  StoredProcedure [dbo].[spOptionQuoteReadJson]    Script Date: 4/21/2020 10:28:05 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[spOptionQuoteReadJson]
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


