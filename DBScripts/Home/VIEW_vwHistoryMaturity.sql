USE Fin
GO

IF EXISTS(SELECT * FROM SysObjects WHERE Name='vwHistoryMaturity')
BEGIN
	DROP VIEW vwHistoryMaturity
END
GO

CREATE VIEW vwHistoryMaturity
AS
	SELECT
		Id
		,Ticker 
		--,SUBSTRING(n.x.value('@ot','VARCHAR(30)'), 1, LEN(n.x.value('@ot','VARCHAR(30)'))-15) AS Ticker2
		,CONVERT(SMALLDATETIME, SUBSTRING(n.x.value('@ot','VARCHAR(30)'), LEN(n.x.value('@ot','VARCHAR(30)'))-14, 6), 103) AS [Maturity]
		,dateadd(dd,0, datediff(dd,0,q.CreatedOn)) AS [Date]
		,q.CreatedOn
	FROM dbo.HistoricalOptionQuoteXML q WITH(NOLOCK)
	CROSS APPLY Content.nodes('/OptChn/Options/Opt') as n(x)
GO

/*
BEGIN TRANSACTION

UPDATE HistoricalOptionQuoteXML SET Ticker = vw.Ticker2
FROM HistoricalOptionQuoteXML hoq WITH(NOLOCK)
INNER JOIN vwHistoryMaturity vw WITH(NOLOCK) ON hoq.Ticker=vw.Ticker AND hoq.CreatedOn=vw.CreatedOn
WHERE vw.Ticker<>vw.Ticker2

SELECT TOP 10 vw.* 
FROM HistoricalOptionQuoteXML hoq WITH(NOLOCK)
INNER JOIN vwHistoryMaturity vw WITH(NOLOCK) ON hoq.Ticker=vw.Ticker AND hoq.CreatedOn=vw.CreatedOn
WHERE vw.Ticker<>vw.Ticker2

SELECT TOP 10 *
FROM vwHistory WITH(NOLOCK)
WHERE CreatedOn IN ('5/20/21', '5/21/21')
AND Ticker = 'SLG'
AND Maturity = '5/21/21'
AND CallPut = 'P'
AND Strike = 70
ORDER BY CreatedOn

ROLLBACK
*/