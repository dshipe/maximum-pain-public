if exists (select * from dbo.sysobjects where name='[HistoricalOptionQuoteXML]')
	drop table [dbo].[HistoricalOptionQuoteXML]	
GO

CREATE TABLE HistoricalOptionQuoteXML (
	ID BIGINT IDENTITY
	, Ticker VARCHAR(10)
	, Content XML
	, CreatedOn SMALLDATETIME
) 

if exists (select * from dbo.sysobjects where name='[ImportStaging]')
	drop table [dbo].[ImportStaging	]
GO

CREATE TABLE ImportStaging (
	ID BIGINT IDENTITY
	, Ticker VARCHAR(10)
	, Content XML
	, CreatedOn SMALLDATETIME
) 
