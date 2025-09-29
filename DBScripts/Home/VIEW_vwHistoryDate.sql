USE Fin
GO

IF EXISTS(SELECT * FROM SysObjects WHERE Name='vwHistoryDate')
BEGIN
	DROP VIEW vwHistoryDate
END
GO

CREATE VIEW vwHistoryDate
AS
	SELECT
		Id 
		,Ticker
		,CreatedOn
		,dateadd(dd,0, datediff(dd,0,q.CreatedOn)) AS [Date]
		,Content
	FROM HistoricalOptionQuoteXML q WITH(NOLOCK)
GO

SELECT *
FROM vwHistoryDate WITH(NOLOCK)
WHERE Ticker='AAPL' 
ORDER BY CreatedOn
