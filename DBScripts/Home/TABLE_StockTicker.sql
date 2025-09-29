/****** Object:  Table [dbo].[StockTicker]    Script Date: 3/17/2020 10:43:44 AM ******/
DROP TABLE [dbo].[StockTicker]
GO

/****** Object:  Table [dbo].[StockTicker]    Script Date: 3/17/2020 10:43:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StockTicker](
	[StockTickerID] [int] IDENTITY(1,1) NOT NULL,
	[Ticker] [varchar](10) NOT NULL,
	[IsActive] [bit] NULL,
	[CreatedOn] [smalldatetime] NULL,
	[ModifiedOn] [smalldatetime] NULL,
 CONSTRAINT [PK_StockTicker] PRIMARY KEY CLUSTERED 
(
	[StockTickerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


