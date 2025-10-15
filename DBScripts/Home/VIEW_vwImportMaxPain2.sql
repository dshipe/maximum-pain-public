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
		,n.x.value('@Ticker[1]','VARCHAR(10)') AS Ticker
		,n.x.value('@Maturity[1]','VARCHAR(10)') AS Maturity
		,n.x.value('@StockPrice[1]','MONEY') AS StockPrice
		,n.x.value('@MaxPain[1]','MONEY') AS MaxPain
		,n.x.value('@TotalCallOI[1]','INT') AS TotalCallOI
		,n.x.value('@TotalPutOI[1]','INT') AS TotalPutOI
		,n.x.value('@HighCallOI[1]','MONEY') AS HighCallOI
		,n.x.value('@HighPutOI[1]','MONEY') AS HighPutOI
	FROM ImportMaxPainXML imp WITH(NOLOCK)
	CROSS APPLY Content.nodes('/ArrayOfImportMaxPain/ImportMaxPain') as n(x)
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
		,n.x.value('@Ticker[1]','VARCHAR(10)') AS Ticker
		,n.x.value('@Maturity[1]','VARCHAR(10)') AS Maturity
		,n.x.value('@StockPrice[1]','MONEY') AS StockPrice
		,n.x.value('@MaxPain[1]','MONEY') AS MaxPain
		,n.x.value('@TotalCallOI[1]','INT') AS TotalCallOI
		,n.x.value('@TotalPutOI[1]','INT') AS TotalPutOI
		,n.x.value('@HighCallOI[1]','MONEY') AS HighCallOI
		,n.x.value('@HighPutOI[1]','MONEY') AS HighPutOI
	FROM ImportMaxPainXML imp WITH(NOLOCK)
	INNER JOIN (
		SELECT MAX([CreatedOn]) AS [CreatedOn]
		FROM ImportMaxPainXML WITH(NOLOCK)
	) AS maximp ON imp.[CreatedOn]=maximp.[CreatedOn]
	CROSS APPLY Content.nodes('/ArrayOfImportMaxPain/ImportMaxPain') as n(x)
GO


SELECT *
FROM vwImportMaxPain WITH(NOLOCK)
WHERE Ticker='AAPL' 
ORDER BY CreatedOn
