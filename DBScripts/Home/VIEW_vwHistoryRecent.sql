USE Fin
GO

IF EXISTS(SELECT * FROM SysObjects WHERE Name='vwHistoryRecent')
BEGIN
	DROP VIEW vwHistoryRecent
END
GO

/*
CREATE VIEW vwHistoricalOptionQuoteXMLRecent
AS
	SELECT 
		Ticker AS UnderlyingSymbol
		,CONVERT(SMALLDATETIME, '20' + SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-14, 6), 103) AS [Maturity]
		,SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-8, 1) AS [CallPut]
		,CONVERT(MONEY, SUBSTRING(n.x.value('@s','VARCHAR(30)'), LEN(n.x.value('@s','VARCHAR(30)'))-7, 8))/1000.0 AS Strike
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
	INNER JOIN (
		SELECT MAX([Date]) AS [Date]
		FROM vwHistoricalOptionQuoteXMLDate WITH(NOLOCK)
	) AS maxq ON q.[Date]=maxq.[Date]
	CROSS APPLY Content.nodes('/root/x') as n(x)
GO
*/

CREATE VIEW vwHistoryRecent
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
		,q.[Date]
		,q.CreatedOn
	FROM vwHistoryDate q WITH(NOLOCK)
	INNER JOIN (
		SELECT MAX([Date]) AS [Date]
		FROM vwHistoryDate WITH(NOLOCK)
	) AS maxq ON q.[Date]=maxq.[Date]
	CROSS APPLY Content.nodes('/OptChn/Options/Opt') as n(x)
GO


SELECT *
FROM vwHistoryRecent WITH(NOLOCK)
WHERE Ticker='AAPL' 
ORDER BY CreatedOn
