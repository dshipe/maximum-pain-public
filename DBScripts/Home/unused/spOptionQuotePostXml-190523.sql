USE MaxPainAPI
GO

/*
SELECT compatibility_level FROM sys.databases WHERE name = 'MaxPainAPI';
--ALTER DATABASE MaxPain SET COMPATIBILITY_LEVEL = 100;  
ALTER DATABASE MaxPainAPI SET COMPATIBILITY_LEVEL = 130;  
GO
*/  

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spOptionQuotePostXml]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spOptionQuotePostXml]	
GO


CREATE PROCEDURE spOptionQuotePostXml
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
		,ImpliedVolatility	VARCHAR(20)
	)

	-- extract (shred) values from XML column nodes
	INSERT INTO #tbl 
	(
		OptionTicker
		,LastPrice		
		,Bid			
		,Ask			
		,Volume			
		,OpenInterest
		,ImpliedVolatility	
	)
	SELECT
		OptionTicker
		,LastPrice		
		,Bid			
		,Ask			
		,Volume			
		,OpenInterest	
		,ImpliedVolatility	
	SELECT
		n.value('@OT[1]','varchar(25)') AS OptionSymbol
		,n.value('@LP[1]','varchar(10)') AS LastPrice
		,n.value('@C[1]','varchar(10)') AS Change
		,n.value('@B[1]','varchar(10)') AS Bid
		,n.value('@A[1]','varchar(10)') AS Ask
		,n.value('@V[1]','varchar(10)') AS Volume
		,n.value('@OI[1]','varchar(10)') AS OpenInterest
		,n.value('@IV[1]','varchar(10)') AS ImpliedVolatility
	FROM @xml.nodes('/root/x') x(n)

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

	/*
	********* ********* ********* ********* *********
	find the stock symbol
	********* ********* ********* ********* *********
	*/
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

	/*
	********* ********* ********* ********* *********
	delete older records
	********* ********* ********* ********* *********
	*/

	DELETE OptionQuote
	FROM OptionQuote
	--WHERE ModifiedOn <  dateadd(hh, -3, @CurrentDate)

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
		,ImpliedVolatility = x.ImpliedVolatility
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
		,x.ImpliedVolatility
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

DECLARE @json VARCHAR(MAX)
SELECT @json='
[{"OT":"AAPL190524C00150000","LP":33.29,"B":32.6,"A":33.05,"OI":296,"V":0,"IV":1.140629296875,"Id":0},
{"OT":"AAPL190524C00160000","LP":23.1,"B":22.7,"A":23.05,"OI":124,"V":0,"IV":0.89648541015624983,"Id":0},
{"OT":"AAPL190524C00167500","LP":17.35,"B":15.25,"A":15.7,"OI":23,"V":0,"IV":0.717776259765625,"Id":0},
{"OT":"AAPL190524C00172500","LP":10.63,"B":10.35,"A":10.75,"OI":284,"V":0,"IV":0.55859816406250007,"Id":0},
{"OT":"AAPL190524C00175000","LP":8.1,"B":8.0,"A":8.25,"OI":524,"V":0,"IV":0.52881330566406259,"Id":0},
{"OT":"AAPL190524C00177500","LP":6.1,"B":5.8,"A":6.0,"OI":586,"V":0,"IV":0.47559118164062497,"Id":0},
{"OT":"AAPL190524C00180000","LP":3.9,"B":3.8,"A":3.95,"OI":2355,"V":0,"IV":0.428716650390625,"Id":0},
{"OT":"AAPL190524C00182500","LP":2.2,"B":2.15,"A":2.22,"OI":2449,"V":0,"IV":0.38526005371093741,"Id":0},
{"OT":"AAPL190524C00185000","LP":0.98,"B":0.98,"A":1.03,"OI":5964,"V":0,"IV":0.35742830078125,"Id":0},
{"OT":"AAPL190524C00187500","LP":0.38,"B":0.37,"A":0.39,"OI":6852,"V":0,"IV":0.34326828613281246,"Id":0},
{"OT":"AAPL190524C00190000","LP":0.15,"B":0.13,"A":0.15,"OI":13094,"V":0,"IV":0.35352208984374994,"Id":0},
{"OT":"AAPL190524C00192500","LP":0.06,"B":0.06,"A":0.07,"OI":10196,"V":0,"IV":0.38281867187499996,"Id":0},
{"OT":"AAPL190524C00195000","LP":0.03,"B":0.02,"A":0.03,"OI":7613,"V":0,"IV":0.4023497265625,"Id":0},
{"OT":"AAPL190524C00197500","LP":0.02,"B":0.01,"A":0.02,"OI":6633,"V":0,"IV":0.445318046875,"Id":0},
{"OT":"AAPL190524C00200000","LP":0.01,"B":0.01,"A":0.02,"OI":14304,"V":0,"IV":0.507817421875,"Id":0},
{"OT":"AAPL190524C00202500","LP":0.01,"B":0.0,"A":0.01,"OI":3832,"V":0,"IV":0.5312546875,"Id":0},
{"OT":"AAPL190524C00205000","LP":0.01,"B":0.0,"A":0.01,"OI":5441,"V":0,"IV":0.54687953125000011,"Id":0},
{"OT":"AAPL190524C00207500","LP":0.01,"B":0.0,"A":0.01,"OI":15921,"V":0,"IV":0.59375406250000007,"Id":0},
{"OT":"AAPL190524C00210000","LP":0.01,"B":0.0,"A":0.01,"OI":5451,"V":0,"IV":0.65625343750000009,"Id":0},
{"OT":"AAPL190524C00212500","LP":0.01,"B":0.0,"A":0.01,"OI":2577,"V":0,"IV":0.6875031250000001,"Id":0},
{"OT":"AAPL190524C00215000","LP":0.01,"B":0.0,"A":0.01,"OI":4171,"V":0,"IV":0.7500025,"Id":0},
{"OT":"AAPL190524C00217500","LP":0.01,"B":0.0,"A":0.01,"OI":2282,"V":0,"IV":0.7812521875,"Id":0},
{"OT":"AAPL190524C00220000","LP":0.01,"B":0.0,"A":0.01,"OI":25303,"V":0,"IV":0.84375156249999994,"Id":0},
{"OT":"AAPL190524C00222500","LP":0.01,"B":0.0,"A":0.01,"OI":3118,"V":0,"IV":0.87500125,"Id":0},
{"OT":"AAPL190524C00225000","LP":0.01,"B":0.0,"A":0.01,"OI":2801,"V":0,"IV":0.937500625,"Id":0},
{"OT":"AAPL190524C00230000","LP":0.01,"B":0.0,"A":0.01,"OI":1278,"V":0,"IV":1.0312548437500002,"Id":0},
{"OT":"AAPL190524C00232500","LP":0.01,"B":0.0,"A":0.01,"OI":167,"V":0,"IV":1.0625046875000002,"Id":0},
{"OT":"AAPL190524C00235000","LP":0.01,"B":0.01,"A":0.01,"OI":355,"V":0,"IV":1.171879140625,"Id":0},
{"OT":"AAPL190524C00237500","LP":0.02,"B":0.0,"A":0.03,"OI":22,"V":0,"IV":1.2656286718749998,"Id":0},
{"OT":"AAPL190524C00242500","LP":0.04,"B":0.0,"A":0.01,"OI":59,"V":0,"IV":1.21875390625,"Id":0},
{"OT":"AAPL190524P00150000","LP":0.01,"B":0.0,"A":0.01,"OI":2343,"V":0,"IV":0.9062509375,"Id":0},
{"OT":"AAPL190524P00165000","LP":0.05,"B":0.04,"A":0.06,"OI":3104,"V":0,"IV":0.648441015625,"Id":0},
{"OT":"AAPL190524P00172500","LP":0.21,"B":0.17,"A":0.21,"OI":7445,"V":0,"IV":0.514653291015625,"Id":0},
{"OT":"AAPL190524P00175000","LP":0.35,"B":0.34,"A":0.35,"OI":6050,"V":0,"IV":0.48438015625,"Id":0},
{"OT":"AAPL190524P00177500","LP":0.6,"B":0.59,"A":0.62,"OI":5785,"V":0,"IV":0.44873598144531246,"Id":0},
{"OT":"AAPL190524P00180000","LP":1.1,"B":1.02,"A":1.1,"OI":10969,"V":0,"IV":0.414068359375,"Id":0},
{"OT":"AAPL190524P00182500","LP":1.94,"B":1.85,"A":1.91,"OI":7281,"V":0,"IV":0.37940073730468749,"Id":0},
{"OT":"AAPL190524P00185000","LP":3.27,"B":3.1,"A":3.25,"OI":7275,"V":0,"IV":0.35742830078125,"Id":0},
{"OT":"AAPL190524P00187500","LP":5.15,"B":4.95,"A":5.1,"OI":8332,"V":0,"IV":0.3403386279296875,"Id":0},
{"OT":"AAPL190524P00190000","LP":7.41,"B":7.2,"A":7.4,"OI":6656,"V":0,"IV":0.36914693359375,"Id":0},
{"OT":"AAPL190524P00192500","LP":9.78,"B":9.6,"A":9.95,"OI":1873,"V":0,"IV":0.48828636718750007,"Id":0},
{"OT":"AAPL190524P00195000","LP":12.3,"B":12.1,"A":12.4,"OI":7076,"V":0,"IV":0.54785608398437513,"Id":0},
{"OT":"AAPL190524P00197500","LP":14.69,"B":14.6,"A":14.9,"OI":1912,"V":0,"IV":0.63086306640625,"Id":0},
{"OT":"AAPL190524P00200000","LP":17.0,"B":17.15,"A":17.4,"OI":2042,"V":0,"IV":0.5820354296875,"Id":0},
{"OT":"AAPL190524P00202500","LP":19.5,"B":19.65,"A":19.85,"OI":1071,"V":0,"IV":0.59766027343750006,"Id":0},
{"OT":"AAPL190524P00227500","LP":27.6,"B":44.55,"A":44.95,"OI":0,"V":0,"IV":1.15625421875,"Id":0},
{"OT":"AAPL190524P00230000","LP":32.1,"B":46.95,"A":47.45,"OI":0,"V":0,"IV":1.5878926855468747,"Id":0},
{"OT":"AAPL190524P00250000","LP":58.8,"B":67.1,"A":67.45,"OI":0,"V":0,"IV":1.6953140234375002,"Id":0},
{"OT":"AAPL190607C00172500","LP":12.55,"B":12.15,"A":12.35,"OI":63,"V":0,"IV":0.39258419921875,"Id":0},
{"OT":"AAPL190607P00150000","LP":0.19,"B":0.18,"A":0.2,"OI":232,"V":0,"IV":0.50098155273437506,"Id":0},
{"OT":"AAPL190607P00155000","LP":0.28,"B":0.3,"A":0.34,"OI":193,"V":0,"IV":0.47559118164062497,"Id":0},
{"OT":"AAPL190607P00160000","LP":0.48,"B":0.48,"A":0.55,"OI":236,"V":0,"IV":0.445318046875,"Id":0},
{"OT":"AAPL190607P00165000","LP":0.76,"B":0.79,"A":0.88,"OI":192,"V":0,"IV":0.41431249755859373,"Id":0},
{"OT":"AAPL190607P00170000","LP":1.37,"B":1.33,"A":1.4,"OI":934,"V":0,"IV":0.38208625732421869,"Id":0},
{"OT":"AAPL190607P00172500","LP":1.78,"B":1.73,"A":1.83,"OI":421,"V":0,"IV":0.37158831542968751,"Id":0},
{"OT":"AAPL190607P00175000","LP":2.28,"B":2.22,"A":2.34,"OI":2375,"V":0,"IV":0.35852692260742192,"Id":0},
{"OT":"AAPL190607P00177500","LP":2.93,"B":2.89,"A":2.99,"OI":349,"V":0,"IV":0.34644208251953118,"Id":0},
{"OT":"AAPL190607P00180000","LP":3.75,"B":3.65,"A":3.8,"OI":2070,"V":0,"IV":0.33472344970703122,"Id":0},
{"OT":"AAPL190607P00182500","LP":4.75,"B":4.65,"A":4.8,"OI":1788,"V":0,"IV":0.32385930053710932,"Id":0},
{"OT":"AAPL190607P00185000","LP":5.96,"B":5.85,"A":6.0,"OI":2833,"V":0,"IV":0.31311722045898438,"Id":0},
{"OT":"AAPL190607P00187500","LP":7.1,"B":7.25,"A":7.4,"OI":6233,"V":0,"IV":0.301764794921875,"Id":0},
{"OT":"AAPL190607P00190000","LP":8.7,"B":8.9,"A":9.05,"OI":1281,"V":0,"IV":0.29297582031249991,"Id":0},
{"OT":"AAPL190607P00192500","LP":10.5,"B":10.7,"A":10.95,"OI":443,"V":0,"IV":0.28870340209960932,"Id":0},
{"OT":"AAPL190607P00195000","LP":12.69,"B":12.85,"A":12.95,"OI":628,"V":0,"IV":0.27735097656249996,"Id":0},
{"OT":"AAPL190607P00197500","LP":15.02,"B":14.95,"A":15.2,"OI":320,"V":0,"IV":0.27930408203124996,"Id":0},
{"OT":"AAPL190607P00200000","LP":17.34,"B":17.25,"A":17.65,"OI":933,"V":0,"IV":0.30371790039062496,"Id":0},
{"OT":"AAPL210618P00300000","LP":113.53,"B":114.55,"A":119.5,"OI":22,"V":0,"IV":0.23139196350097657,"Id":0}]
'
SELECT LEN(@json)
BEGIN TRANSACTION
EXEC spOptionQuotePostXml @json
GO
SELECT * FROM vwOptionQuote
ROLLBACK