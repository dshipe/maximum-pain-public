/****** Object:  StoredProcedure [dbo].[spStockTickersPost]    Script Date: 3/12/2020 10:09:13 PM ******/
DROP PROCEDURE [dbo].[spStockTickersPost]
GO

/****** Object:  StoredProcedure [dbo].[spStockTickersPost]    Script Date: 3/12/2020 10:09:13 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spStockTickersPost]
	@TickersCSV VARCHAR(MAX)
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
	
	DECLARE @CurrentDate SMALLDATETIME
	SELECT @CurrentDate = GETUTCDATE()

	DECLARE @Tickers TABLE (Ticker VARCHAR(10))

	INSERT INTO @Tickers(Ticker)
	SELECT ReturnValue
	FROM dbo.fnParseString(@TickersCSV, ',')
	

	/*
	********* ********* ********* ********* *********
	make inactive 
	********* ********* ********* ********* *********
	*/

	UPDATE StockTicker SET IsActive = 0

	/*
	********* ********* ********* ********* *********
	update 
	********* ********* ********* ********* *********
	*/
	
	UPDATE StockTicker SET
		IsActive = 1
		,ModifiedOn = @CurrentDate
	FROM @Tickers x
	INNER JOIN StockTicker st WITH(NOLOCK)
		ON x.Ticker = st.Ticker
		
	/*
	********* ********* ********* ********* *********
	insert 
	********* ********* ********* ********* *********
	*/

	INSERT INTO StockTicker 
	(
		Ticker	
		,IsActive
		,CreatedOn
		,ModifiedOn
	)	
	SELECT DISTINCT
		x.Ticker	
		,1
		,@CurrentDate
		,@CurrentDate
	FROM @Tickers x
	LEFT JOIN StockTicker st WITH(NOLOCK)
		ON x.Ticker = st.Ticker
	WHERE st.Ticker IS NULL
	
	RETURN 1
END
GO
