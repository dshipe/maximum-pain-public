USE [fin]
GO

/****** Object:  View [dbo].[vwHistoricalStockQuoteXMLRecentClose]    Script Date: 8/13/2020 8:56:31 PM ******/
DROP VIEW [dbo].[vwHistoricalStockQuoteXMLRecentClose]
GO

/****** Object:  View [dbo].[vwHistoricalStockQuoteXMLRecentClose]    Script Date: 8/13/2020 8:56:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwHistoricalStockQuoteXMLRecentClose]
AS
	SELECT 
		CreatedOn 
		,dateadd(dd,0, datediff(dd,0, CreatedOn)) AS [Date]
		,n.x.value('symbol[1]','VARCHAR(10)') AS Ticker
		,n.x.value('lastPrice[1]','DECIMAL(6,2)') AS lastPrice
	FROM HistoricalStockQuoteXML q WITH(NOLOCK)
	INNER JOIN (
		SELECT MAX(dateadd(dd,0, datediff(dd,0, CreatedOn))) AS [Date]
		FROM HistoricalStockQuoteXML WITH(NOLOCK)
	) AS maxq ON dateadd(dd,0, datediff(dd,0,q.CreatedOn))= maxq.[Date]	
	CROSS APPLY Content.nodes('/ArrayOfStock/Stock') as n(x)
GO


