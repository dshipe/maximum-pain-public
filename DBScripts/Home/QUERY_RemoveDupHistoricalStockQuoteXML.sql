USE FIN
GO

/*
SELECT ID, Ticker, CreatedOn
FROM HistoricalOptionQuoteXML WITH(NOLOCK)
WHERE Ticker='SPX'
AND CreatedOn BETWEEN '5/26/20' AND '5/27/20'
*/

BEGIN TRANSACTION

DELETE FROM HistoricalOptionQuoteXML
WHERE CreatedOn BETWEEN '12/31/20' AND '1/2/21'
AND ID IN (
	SELECT a.ID
	FROM HistoricalOptionQuoteXML a WITH(NOLOCK)
	INNER JOIN HistoricalOptionQuoteXML b WITH(NOLOCK)
		ON a.Ticker = b.Ticker
		AND a.CreatedOn BETWEEN '12/31/20' AND '1/2/21'
		AND dateadd(dd,0, datediff(dd,0, a.CreatedOn)) = dateadd(dd,0, datediff(dd,0, b.CreatedOn))
		AND a.Id < b.Id
)

SELECT a.CreatedOn
FROM HistoricalStockQuoteXML a WITH(NOLOCK)
INNER JOIN HistoricalStockQuoteXML b WITH(NOLOCK)
	ON a.Id<>b.Id
	AND a.CreatedOn < b.CreatedOn
	AND dateadd(dd,0, datediff(dd,0, a.CreatedOn)) = dateadd(dd,0, datediff(dd,0, b.CreatedOn))
ORDER BY a.CreatedOn

SELECT CreatedOn, COUNT(*) AS Records 
FROM HistoricalOptionQuoteXML WITH(NOLOCK)
WHERE CreatedOn >DATEADD(dd, -30, GETUTCDATE())
GROUP BY CreatedOn
ORDER BY CreatedOn DESC

ROLLBACK
--COMMIT