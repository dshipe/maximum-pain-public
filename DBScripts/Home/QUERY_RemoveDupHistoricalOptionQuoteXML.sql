USE FIN
GO

/*
SELECT ID, Ticker, CreatedOn
FROM HistoricalOptionQuoteXML WITH(NOLOCK)
WHERE Ticker='SPX'
AND CreatedOn BETWEEN '5/26/20' AND '5/27/20'
*/

DECLARE @Start DATETIME = '1/4/21'

BEGIN TRANSACTION

DELETE FROM HistoricalOptionQuoteXML
WHERE ID IN (
	SELECT a.ID
	FROM HistoricalOptionQuoteXML a WITH(NOLOCK)
	INNER JOIN HistoricalOptionQuoteXML b WITH(NOLOCK)
		ON a.Ticker = b.Ticker
		AND dateadd(dd,0, datediff(dd,0, a.CreatedOn)) = dateadd(dd,0, datediff(dd,0, b.CreatedOn))
		AND a.Id < b.Id
		AND a.CreatedOn BETWEEN @Start AND DATEADD(dd, 7, @Start)
)


SELECT a.ID, a.Ticker, a.CreatedOn, b.ID, b.Ticker, b.CreatedOn 
FROM HistoricalOptionQuoteXML a WITH(NOLOCK)
INNER JOIN HistoricalOptionQuoteXML b WITH(NOLOCK)
	ON a.Ticker = b.Ticker
	AND dateadd(dd,0, datediff(dd,0, a.CreatedOn)) = dateadd(dd,0, datediff(dd,0, b.CreatedOn))
	AND a.Id < b.Id
	AND a.CreatedOn BETWEEN @Start AND DATEADD(dd, 7, @Start)
ORDER BY a.CreatedOn


SELECT CreatedOn, COUNT(*) AS Records 
FROM HistoricalOptionQuoteXML WITH(NOLOCK)
WHERE CreatedOn >DATEADD(dd, -30, GETUTCDATE())
GROUP BY CreatedOn
ORDER BY CreatedOn DESC

ROLLBACK
--COMMIT