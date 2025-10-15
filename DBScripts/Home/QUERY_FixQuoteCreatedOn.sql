USE Fin
GO

SELECT Id,Ticker,CreatedOn, Content
FROM HistoricalOptionQuoteXml WITH(NOLOCK)
WHERE CreatedOn>'1/21/21'
AND TIcker='GE'
ORDER BY CreatedOn

SELECT CreatedOn, COUNT(*) AS Records 
FROM HistoricalOptionQuoteXML WITH(NOLOCK) 
WHERE CreatedOn >DATEADD(dd, -30, GETUTCDATE())
GROUP BY CreatedOn
ORDER BY CreatedOn DESC

/*
-- all dates
SELECT * FROM HistoricalOptionQuoteXML WHERE Ticker='AAPL' AND CreatedOn > '4/15/2020'

-- duplicates
SELECT a.*
FROM HistoricalOptionQuoteXML a
INNER JOIN HistoricalOptionQuoteXML b 
	ON a.Ticker=b.Ticker 
	AND dateadd(dd,0, datediff(dd,0, a.CreatedOn)) = dateadd(dd,0, datediff(dd,0, b.CreatedOn))
	AND a.ID<>b.ID
WHERE a.Ticker='AAPL'
*/

BEGIN TRANSACTION

--DELETE FROM HistoricalOptionQuoteXML WHERE CreatedOn < '3/1/2020'

-- remove duplicates
DELETE FROM HistoricalOptionQuoteXML WHERE Id IN (
	SELECT b.Id
	FROM HistoricalOptionQuoteXML a WITH(NOLOCK)
	INNER JOIN HistoricalOptionQuoteXML b WITH(NOLOCK) 
		ON a.Ticker=b.Ticker 
		AND dateadd(dd,0, datediff(dd,0, a.CreatedOn)) = dateadd(dd,0, datediff(dd,0, b.CreatedOn))
		AND a.ID<b.ID
)

-- fix CreatedOn
--UPDATE HistoricalOptionQuoteXML SET CreatedOn = '2020-07-24 23:50:00' WHERE CreatedOn BETWEEN '7/24/2020' AND '7/26/2020 06:00:00'

SELECT CreatedOn, COUNT(*) AS Records 
FROM HistoricalOptionQuoteXML WITH(NOLOCK) 
WHERE CreatedOn >DATEADD(dd, -30, GETUTCDATE())
GROUP BY CreatedOn
ORDER BY CreatedOn DESC

SELECT a.*
FROM HistoricalOptionQuoteXML a WITH(NOLOCK)
INNER JOIN HistoricalOptionQuoteXML b WITH(NOLOCK) 
	ON a.Ticker=b.Ticker 
	AND dateadd(dd,0, datediff(dd,0, a.CreatedOn)) = dateadd(dd,0, datediff(dd,0, b.CreatedOn))
	AND a.ID<>b.ID
WHERE a.Ticker='AAPL'

ROLLBACK
--COMMIT
