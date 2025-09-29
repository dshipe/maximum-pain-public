USE Fin
GO

IF EXISTS(SELECT * FROM SysObjects WHERE Name='vwImportMaxPain')
BEGIN
	DROP VIEW vwImportMaxPain
END
GO

CREATE VIEW vwImportMaxPain
--WITH SCHEMABINDING
AS
	SELECT
		imp.ID
		,imp.CreatedOn
		,n.x.value('@t[1]','VARCHAR(10)') AS Ticker
		,n.x.value('@m[1]','VARCHAR(10)') AS Maturity
		,n.x.value('@sp[1]','MONEY') AS StockPrice
		,n.x.value('@mp[1]','MONEY') AS MaxPain
		,n.x.value('@coi[1]','INT') AS TotalCallOI
		,n.x.value('@poi[1]','INT') AS TotalPutOI
		,n.x.value('@hc[1]','MONEY') AS HighCallOI
		,n.x.value('@hp[1]','MONEY') AS HighPutOI
	FROM ImportMaxPainXML imp WITH(NOLOCK)
	CROSS APPLY Content.nodes('/ArrayOfMX/Mx') as n(x)
GO

--CREATE UNIQUE CLUSTERED INDEX IDX_vwImportMaxPain
--ON vwHistory(Ticker, Maturity)


IF EXISTS(SELECT * FROM SysObjects WHERE Name='vwImportMaxPainRecent')
BEGIN
	DROP VIEW vwImportMaxPainRecent
END
GO

CREATE VIEW vwImportMaxPainRecent
--WITH SCHEMABINDING
AS
	SELECT
		imp.ID
		,imp.CreatedOn
		,n.x.value('@t[1]','VARCHAR(10)') AS Ticker
		,n.x.value('@m[1]','VARCHAR(10)') AS Maturity
		,n.x.value('@sp[1]','MONEY') AS StockPrice
		,n.x.value('@mp[1]','MONEY') AS MaxPain
		,n.x.value('@coi[1]','INT') AS TotalCallOI
		,n.x.value('@poi[1]','INT') AS TotalPutOI
		,n.x.value('@hc[1]','MONEY') AS HighCallOI
		,n.x.value('@hp[1]','MONEY') AS HighPutOI
	FROM ImportMaxPainXML imp WITH(NOLOCK)
	INNER JOIN (
		SELECT MAX([CreatedOn]) AS [CreatedOn]
		FROM ImportMaxPainXML WITH(NOLOCK)
	) AS maximp ON imp.[CreatedOn]=maximp.[CreatedOn]
	CROSS APPLY Content.nodes('/ArrayOfMX/Mx') as n(x)
GO


SELECT *
FROM vwImportMaxPain WITH(NOLOCK)
WHERE CreatedOn = '3/3/22'
AND Ticker = 'AAPL' 
AND Maturity = '03/04/2022'
ORDER BY CreatedOn

SELECT *
FROM vwImportMaxPainRecent WITH(NOLOCK)
WHERE Ticker='AAPL' 
ORDER BY CreatedOn
