if exists (select * from dbo.sysobjects where name='[HistoricalStockQuoteXML]')
	drop table [dbo].[HistoricalStockQuoteXML]	
GO

CREATE TABLE HistoricalStockQuoteXML (
	ID BIGINT IDENTITY
	, Content XML
	, CreatedOn SMALLDATETIME
) 

