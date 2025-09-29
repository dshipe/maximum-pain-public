USE MaxPainAPI
GO

if exists (select * from dbo.sysobjects where name='StockTicker')
	drop table [dbo].[StockTicker]	
GO

CREATE TABLE StockTicker (
	StockTickerID INT IDENTITY
	, Ticker VARCHAR(10)
	, IsActive BIT
	, CreatedOn SMALLDATETIME
	, ModifiedOn SMALLDATETIME
)