USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spMPImportPostProcessing]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spMPImportPostProcessing]	
GO


CREATE PROCEDURE dbo.[spMPImportPostProcessing]
	@ProcessDate SMALLDATETIME = NULL
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	After the import has executed, run this SP to do any post-processing
	for example: collect the "most active" data

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.

	Revision History:

	Date		Name	Description
	----		----	-----------
	2014.07.10	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN	
	SET NOCOUNT ON

	EXEC spMPMostActiveXML @QueryType='ChangePrice', @TickerCount=10, @NextMaturity=1, @TruncateMarket=1
	EXEC spMPMostActiveXML @QueryType='OpenInterest', @TickerCount=10, @NextMaturity=1
	EXEC spMPMostActiveXML @QueryType='ChangeOpenInterest', @TickerCount=10, @NextMaturity=1
	EXEC spMPMostActiveXML @QueryType='Volume', @TickerCount=10, @NextMaturity=1
	EXEC spMPMostActiveXML @QueryType='ChangeVolume', @TickerCount=10, @NextMaturity=1
	EXEC spMPMostActiveXML @QueryType='IV', @TickerCount=10, @NextMaturity=1
	EXEC spMPMostActiveXML @QueryType='ChangeIV', @TickerCount=10, @NextMaturity=1
	
	EXEC spMPMostActiveXML @QueryType='ChangePrice', @TickerCount=10, @TruncateResult=1
	EXEC spMPMostActiveXML @QueryType='OpenInterest', @TickerCount=10
	EXEC spMPMostActiveXML @QueryType='ChangeOpenInterest', @TickerCount=10
	EXEC spMPMostActiveXML @QueryType='Volume', @TickerCount=10
	EXEC spMPMostActiveXML @QueryType='ChangeVolume', @TickerCount=10
	EXEC spMPMostActiveXML @QueryType='IV', @TickerCount=10
	EXEC spMPMostActiveXML @QueryType='ChangeIV', @TickerCount=10
	
	EXEC spMPOutsideOIWallsXML

	TRUNCATE TABLE MostActiveResult
	TRUNCATE TABLE MostActiveMarket

	RETURN 1
END
GO

--EXEC spMPImportPostProcessing
--GO


SELECT NextMaturity, * FROM MostActive WITH(NOLOCK)
GO
