USE [fin]
GO

/****** Object:  View [dbo].[vwHistoricalStockQuoteXML]    Script Date: 8/13/2020 8:54:49 PM ******/
DROP VIEW [dbo].[vwHistoricalStockQuoteXML]
GO

/****** Object:  View [dbo].[vwHistoricalStockQuoteXML]    Script Date: 8/13/2020 8:54:50 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vwHistoricalStockQuoteXML]
AS
	SELECT 
		CreatedOn 
		,dateadd(dd,0, datediff(dd,0, CreatedOn)) AS [Date]
		,n.x.value('symbol[1]','VARCHAR(10)') AS Ticker

		,n.x.value('assetType[1]','VARCHAR(100)') AS AssetType
		,n.x.value('assetMainType[1]','VARCHAR(100)') AS assetMainType
		,n.x.value('cusip[1]','VARCHAR(100)') AS cusip
		,n.x.value('symbol[1]','VARCHAR(10)') AS symbol
		,n.x.value('bidPrice[1]','DECIMAL(6,2)') AS bidPrice
		,n.x.value('bidSize[1]','INT') AS bidSize
		--,n.x.value('bidId[1]','CHAR(1)') AS bidId
		,n.x.value('askPrice[1]','DECIMAL(6,2)') AS askPrice
		,n.x.value('askSize[1]','INT') AS askSize
		--,n.x.value('askId[1]','CHAR(1)') AS askId
		,n.x.value('lastPrice[1]','DECIMAL(6,2)') AS lastPrice
		,n.x.value('lastSize[1]','INT') AS lastSize
		--,n.x.value('lastId[1]','CHAR(1)') AS lastId
		,n.x.value('openPrice[1]','DECIMAL(6,2)') AS openPrice
		,n.x.value('highPrice[1]','DECIMAL(6,2)') AS highPrice
		,n.x.value('lowPrice[1]','DECIMAL(6,2)') AS lowPrice
		--,n.x.value('bidTick[1]','VARCHAR(100)') AS bidTick
		,n.x.value('closePrice[1]','DECIMAL(6,2)') AS closePrice
		,n.x.value('netChange[1]','DECIMAL(6,2)') AS netChange
		,n.x.value('totalVolume[1]','INT') AS totalVolume
		,n.x.value('quoteTimeInLong[1]','VARCHAR(20)') AS quoteTimeInLong
		,n.x.value('tradeTimeInLong[1]','VARCHAR(20)') AS tradeTimeInLong
		,n.x.value('mark[1]','DECIMAL(6,2)') AS mark
		,n.x.value('exchange[1]','VARCHAR(100)') AS exchange
		,n.x.value('marginable[1]','bit') AS marginable
		,n.x.value('shortable[1]','bit') AS shortable
		,n.x.value('volatility[1]','float') AS volatility

		,n.x.value('digits[1]','VARCHAR(100)') AS digits
		,n.x.value('_52WkHigh[1]','DECIMAL(6,2)') AS _52WkHigh
		,n.x.value('_52WkLow[1]','DECIMAL(6,2)') AS _52WkLow
		,n.x.value('nAV[1]','FLOAT') AS nAV
		,n.x.value('peRatio[1]','FLOAT') AS peRatio
		,n.x.value('divAmount[1]','VARCHAR(100)') AS divAmount
		,n.x.value('divYield[1]','VARCHAR(100)') AS divYield
		,n.x.value('divDate[1]','SMALLDATETIME') AS divDate
		,n.x.value('securityStatus[1]','VARCHAR(100)') AS securityStatus
		--,n.x.value('regularMarketLastPrice[1]','VARCHAR(100)') AS regularMarketLastPrice
		--,n.x.value('regularMarketLastSize[1]','VARCHAR(100)') AS regularMarketLastSize
		--,n.x.value('regularMarketNetChange[1]','VARCHAR(100)') AS regularMarketNetChange
		--,n.x.value('regularMarketTradeTimeInLong[1]','VARCHAR(100)') AS regularMarketTradeTimeInLong
		--,n.x.value('netPercentChangeInDouble[1]','VARCHAR(100)') AS netPercentChangeInDouble
		--,n.x.value('markPercentChangeInDouble[1]','VARCHAR(100)') AS markPercentChangeInDouble
		--,n.x.value('regularMarketPercentChangeInDouble[1]','VARCHAR(100)') AS regularMarketPercentChangeInDouble

		,n.x.value('delayed[1]','bit') AS delayed

	FROM HistoricalStockQuoteXML q WITH(NOLOCK)
	CROSS APPLY Content.nodes('/ArrayOfStock/Stock') as n(x)
GO


