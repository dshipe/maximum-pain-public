USE [Fin]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS(SELECT * FROM Sys.objects WHERE [Name]='fnParseOptionTicker')
BEGIN
	DROP FUNCTION [dbo].[fnParseOptionTicker]
END
GO

IF EXISTS(SELECT * FROM Sys.types WHERE [Name]='ParseOptionTickerType')
BEGIN
	DROP TYPE ParseOptionTickerType
END 
GO

CREATE TYPE dbo.ParseOptionTickerType AS TABLE  
(  
   Id BIGINT
   ,Ticker VARCHAR(10)
   ,Content XML
   ,CreatedOn SMALLDATETIME
)  
GO






CREATE FUNCTION [dbo].[fnParseOptionTicker] (@Param dbo.ParseOptionTickerType READONLY)
RETURNS @tbReturnValue TABLE (
	Id BIGINT
	,ot VARCHAR(30)
	,Maturity SMALLDATETIME
	,CallPut CHAR(1)
	,Strike MONEY
	,StockPrice MONEY
	,LastPrice MONEY
	,Change FLOAT
	,Bid MONEY
	,Ask MONEY
	,Volume INT
	,OpenInterest INT
	,ImpliedVolatility FLOAT
	,Delta FLOAT
	,Gamma FLOAT
	,Theta FLOAT
	,Vega FLOAT
	,Rho FLOAT
	,[Date] SMALLDATETIME
)
BEGIN 

	INSERT INTO @tbReturnValue
	SELECT
		p.Id
		,n.x.value('@ot','VARCHAR(30)') AS ot
		,CONVERT(SMALLDATETIME, '20' + SUBSTRING(n.x.value('@ot','VARCHAR(30)'), LEN(n.x.value('@ot','VARCHAR(30)'))-14, 6), 103) AS [Maturity]
		,SUBSTRING(n.x.value('@ot','VARCHAR(30)'), LEN(n.x.value('@ot','VARCHAR(30)'))-8, 1) AS [CallPut]
		,CONVERT(MONEY, SUBSTRING(n.x.value('@ot','VARCHAR(30)'), LEN(n.x.value('@ot','VARCHAR(30)'))-7, 8))/1000.0 AS Strike
		,n.x.value('../../@StockPrice','MONEY') AS StockPrice
		,n.x.value('@p','MONEY') AS LastPrice
		,n.x.value('@c','FLOAT') AS Change
		,n.x.value('@b','MONEY') AS Bid
		,n.x.value('@a','MONEY') AS Ask
		,n.x.value('@v','INT') AS Volume
		,n.x.value('@oi','INT') AS OpenInterest
		,n.x.value('@iv','FLOAT') AS ImpliedVolatility
		,n.x.value('@de','FLOAT') AS Delta
		,n.x.value('@ga','FLOAT') AS Gamma
		,n.x.value('@th','FLOAT') AS Theta
		,n.x.value('@ve','FLOAT') AS Vega
		,n.x.value('@rh','FLOAT') AS Rho
		,dateadd(dd,0, datediff(dd,0,p.CreatedOn)) AS [Date]

	FROM @param p
	CROSS APPLY Content.nodes('/OptChn/Options/Opt') as n(x)
	
	RETURN
END

GO



DECLARE @param ParseOptionTickerType
INSERT INTO @param (Id, Ticker, CreatedOn, Content)
VALUES (1, 'GE', GETUTCDATE(), '
<OptChn Source="TDA" Stock="GE" StockPrice="7.135" InterestRate="0.1" Volatility="29" CreatedOn="2020-06-19T23:09:54.1922672Z">
  <Options>
    <Opt ot="GE200619C00001000" p="6.25" c="0.01" b="6.1" a="6.2" oi="27" v="0" iv="1000" de="1" ga="0" th="0" ve="0.001" rh="0" />
    <Opt ot="GE200619C00001500" p="5.95" c="0.21" b="5.6" a="5.7" oi="2" v="0" iv="1000" de="1" ga="0" th="0" ve="0" rh="0" />
    <Opt ot="GE200619C00002000" p="5.25" c="-0.03" b="5.1" a="5.2" oi="118" v="5" iv="1000" de="1" ga="0" th="0" ve="0.001" rh="0" />
    <Opt ot="GE200619C00002500" p="0.0" c="0.0" b="4.6" a="4.7" oi="0" v="0" iv="1000" de="1" ga="0" th="0" ve="0.006" rh="0" />
    <Opt ot="GE200619C00003000" p="4.15" c="-0.13" b="4.1" a="4.2" oi="603" v="3" iv="1000" de="1" ga="0" th="0" ve="0.015" rh="0" />
  </Options>
</OptChn>	
')
INSERT INTO @param (Id, Ticker, CreatedOn, Content)
VALUES (2, 'GE', GETUTCDATE(), '
<OptChn Source="TDA" Stock="GE" StockPrice="7.135" InterestRate="0.1" Volatility="29" CreatedOn="2020-06-19T23:09:54.1922672Z">
  <Options>
    <Opt ot="GE200619C00003500" p="4.1" c="0.36" b="3.6" a="3.7" oi="3" v="0" iv="1000" de="0.883" ga="0" th="-0.12" ve="0.031" rh="0" />
    <Opt ot="GE200619C00004000" p="3.2" c="-0.08" b="3.1" a="3.2" oi="340" v="11" iv="762.86" de="0.896" ga="0.002" th="-0.116" ve="0" rh="0" />
    <Opt ot="GE200619C00004500" p="3.0" c="0.26" b="2.58" a="2.69" oi="56" v="0" iv="5" de="1" ga="0" th="0" ve="0" rh="0" />
    <Opt ot="GE200619C00005000" p="2.17" c="-0.11" b="2.08" a="2.19" oi="2358" v="31" iv="5" de="1" ga="0" th="0" ve="0" rh="0" />
    <Opt ot="GE200619C00005500" p="1.67" c="-0.11" b="1.62" a="1.68" oi="159" v="11" iv="298.055" de="0.928" ga="0.015" th="-0.064" ve="0" rh="0" />
  </Options>
</OptChn>	
')

--SELECT * FROM @param

SELECT * FROM fnParseOptionTicker(@param)   