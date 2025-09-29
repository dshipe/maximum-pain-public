USE Fin
GO

--BEGIN TRANSACTION

--EXEC [spHistoricalOptionQuoteXMLPostFromStaging] @ImportDate='6/4/2021'

SELECT CreatedOn, COUNT(*) AS Records
FROM ImportStaging WITH(NOLOCK)
GROUP BY CreatedOn
ORDER BY CreatedOn DESC

SELECT TOP 10 ImportDate, CreatedOn, COUNT(*) AS Records
FROM ImportCache WITH(NOLOCK)
--WHERE [Hour]>16
GROUP BY ImportDate, CreatedOn
ORDER BY ImportDate DESC, CreatedOn DESC

SELECT TOP 100 CreatedOn, COUNT(*) AS Records
FROM HistoricalOptionQuoteXML WITH(NOLOCK)
GROUP BY CreatedOn
ORDER BY CreatedOn DESC

--COMMIT

--UPDATE ImportCache SET ImportDate='3/5/21' WHERE ImportDate='3/7/21'

/*
DECLARE @importDate DATETIME = '1/15/2021'
SELECT
	hv.Id
	,hv.Ticker
	,hv.CreatedOn
	,hv.ot
	,hv.Volume
	,ic.Volume
FROM vwHistoryVolume hv WITH(NOLOCK)
INNER JOIN vwImportCacheVolume ic WITH(NOLOCK)
	ON hv.Ticker=ic.Ticker
	AND hv.CreatedOn=ic.ImportDate
	AND hv.ot = ic.ot
	AND ic.[Hour] > 16
WHERE hv.CreatedOn=@ImportDate
AND hv.Volume<>ic.Volume

-- DELETE FROM ImportCache WHERE ImportDate='1/27/21'
-- UPDATE HistoricalOptionQuoteXML SET CreatedOn='12/23/21' WHERE CreatedOn='12/27/21'

SELECT * FROM HistoricalOptionQuoteXML WITH(NOLOCK) WHERE CreatedOn='2021-12-26 00:00:00'
-- DELETE FROM HistoricalOptionQuoteXML WHERE CreatedOn='2021-12-26 00:00:00'

https://localhost:5001/api/finimport/patchvolume?importdate=1/21/2021
https://localhost:5001/api/finimport/patchvolume?importdate=1/21/2021&ticker=FE

http://maximum-pain.com/api/finimport/patchvolume?importdate=1/21/2021

EXEC spHistoricalOptionQuoteXMLPostFromStaging
EXEC spMPImportPostProcessing
*/
