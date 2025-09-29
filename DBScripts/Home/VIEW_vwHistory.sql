USE Fin
GO

IF EXISTS(SELECT * FROM SysObjects WHERE Name='vwHistory')
BEGIN
	DROP VIEW vwHistory
END
GO

/*
CREATE VIEW vwHistoricalOptionQuoteXML
AS
	SELECT 
		Ticker AS UnderlyingSymbol
		,CONVERT(SMALLDATETIME, '20' + SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-14, 6), 103) AS [Maturity]
		,SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-8, 1) AS [CallPut]
		,CONVERT(MONEY, SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-7, 8))/1000.0 AS Strike
		,n.x.value('@ot','VARCHAR(30)') AS ot
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
		,q.[Date]
		,q.CreatedOn
		--,n.x.value('@s','VARCHAR(30)')
	FROM vwHistoricalOptionQuoteXMLDate q WITH(NOLOCK)
	CROSS APPLY Content.nodes('/root/x') as n(x)
GO
*/

CREATE VIEW vwHistory
--WITH SCHEMABINDING
AS
	SELECT
		Id 
		,Ticker
		--,SUBSTRING(n.x.value('@ot','VARCHAR(30)'), 1, LEN(n.x.value('@ot','VARCHAR(30)'))-15) AS Ticker
		,CONVERT(SMALLDATETIME, SUBSTRING(n.x.value('@ot','VARCHAR(30)'), LEN(n.x.value('@ot','VARCHAR(30)'))-14, 6), 103) AS [Maturity]
		,SUBSTRING(n.x.value('@ot','VARCHAR(30)'), LEN(n.x.value('@ot','VARCHAR(30)'))-8, 1) AS [CallPut]
		,CONVERT(MONEY, SUBSTRING(n.x.value('@ot','VARCHAR(30)'), LEN(n.x.value('@ot','VARCHAR(30)'))-7, 8))/1000.0 AS Strike
		,n.x.value('../../@StockPrice','MONEY') AS StockPrice
		,n.x.value('@ot','VARCHAR(30)') AS ot
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
		,dateadd(dd,0, datediff(dd,0,q.CreatedOn)) AS [Date]
		,q.CreatedOn
	FROM dbo.HistoricalOptionQuoteXML q WITH(NOLOCK)
	CROSS APPLY Content.nodes('/OptChn/Options/Opt') as n(x)
GO

--CREATE UNIQUE CLUSTERED INDEX IDX_vwHistory
--ON vwHistory(Ticker, Maturity)

/*
SELECT *
FROM vwHistory WITH(NOLOCK)
WHERE Ticker='AAPL' 
AND Maturity = '6/26/20'
AND Strike = 300
AND CallPut='C'
ORDER BY CreatedOn
*/