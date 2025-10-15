if exists (select * from dbo.sysobjects where name='[HistoricalOptionQuoteXML]')
	drop table [dbo].[HistoricalOptionQuoteXML]	
GO

CREATE TABLE HistoricalOptionQuoteXML (
	ID BIGINT IDENTITY
	, Ticker VARCHAR(10) NOT NULL
	, CreatedOn SMALLDATETIME NOT NULL
	, Content XML
) 
GO

-- ALTER TABLE HistoricalOptionQuoteXML DROP COLUMN ImportDate SMALLDATETIME

if exists (select * from dbo.sysobjects where name='ImportStaging')
	drop table [dbo].[ImportStaging]
GO

if exists (select * from dbo.sysobjects where name='ImportCache')
	drop table [dbo].[ImportCache]
GO


CREATE TABLE ImportStaging (
	ID BIGINT IDENTITY
	, Ticker VARCHAR(10)
	, ImportDate SMALLDATETIME 
	, CreatedOn SMALLDATETIME
	, Content XML
) 
GO

CREATE TABLE ImportCache (
	ID BIGINT IDENTITY
	, Ticker VARCHAR(10)
	, CreatedOn SMALLDATETIME
	, CreatedOnEST SMALLDATETIME
	, [Hour] INT
	, ImportDate SMALLDATETIME
	, Content XML
) 
GO


ALTER TABLE HistoricalOptionQuoteXML ALTER COLUMN Ticker VARCHAR(10) NOT NULL
ALTER TABLE HistoricalOptionQuoteXML ALTER COLUMN CreatedOn SMALLDATETIME NOT NULL

/*
ALTER TABLE [dbo].[HistoricalOptionQuoteXML] 
	ADD CONSTRAINT [PK_TickerCreatedOn] 
	PRIMARY KEY CLUSTERED 
(
	[Ticker] ASC
	,[CreatedOn] ASC
)
WITH (
	PAD_INDEX = OFF
	, STATISTICS_NORECOMPUTE = OFF
	, SORT_IN_TEMPDB = OFF
	, IGNORE_DUP_KEY = OFF
	, ONLINE = OFF
	, ALLOW_ROW_LOCKS = ON
	, ALLOW_PAGE_LOCKS = ON
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[HistoricalOptionQuoteXML] DROP CONSTRAINT [PK_TickerCreatedOn] WITH ( ONLINE = OFF )
GO
*/