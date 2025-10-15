USE Fin
GO

SELECT CreatedOn, COUNT(*) AS Records 
FROM HistoricalOptionQuoteXML WITH(NOLOCK) 
WHERE CreatedOn >DATEADD(dd, -30, GETUTCDATE())
GROUP BY CreatedOn
ORDER BY CreatedOn DESC

SELECT * FROM MarketCalendar WITH(NOLOCK)
--INSERT INTO MarketCalendar ([Date]) VALUES ('1/13/2021')

/*
--SELECT * FROM StockTicker WITH(NOLOCK)
SELECT ID, Ticker, CreatedOn FROM HistoricalOptionQuoteXML WHERE CreatedOn>'12/31/2020' AND Ticker='AAPL'
DELETE FROM HistoricalOptionQuoteXML WHERE CreatedOn>='1/13/2021'

UPDATE HistoricalOptionQuoteXML SET CreatedOn='1/13/21' WHERE CreatedOn>='1/12/2021'

DELETE FROM MaxPainAPI..Message WHERE Subject LIKE 'ERROR ScrapeHelper.cs%'
DELETE FROM MaxPainAPI..Message WHERE Subject LIKE 'FinImportEngine partial%'
*/


DECLARE @CurrentDate DATETIME = GETUTCDATE()
DECLARE @CurrentDateEST DATETIME = CONVERT(DATETIME, SWITCHOFFSET(@CurrentDate, DATEPART(TZOFFSET, @CurrentDate AT TIME ZONE 'Eastern Standard Time')))
DECLARE @CurrentDatePST DATETIME = CONVERT(DATETIME, SWITCHOFFSET(@CurrentDate, DATEPART(TZOFFSET, @CurrentDate AT TIME ZONE 'Pacific Standard Time')))

SELECT @CurrentDate, @CurrentDateEST, @CurrentDatePST, MAX([Date]) AS [Date] 
FROM MarketCalendar WITH(NOLOCK) 
WHERE [Date]<dateadd(dd,0, datediff(dd,0, @CurrentDateEST))
