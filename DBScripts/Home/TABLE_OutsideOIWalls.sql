USE [fin]
GO

/****** Object:  Table [dbo].[OutsideOIWalls]    Script Date: 4/21/2020 11:02:29 AM ******/
DROP TABLE [dbo].[OutsideOIWalls]
GO

/****** Object:  Table [dbo].[OutsideOIWalls]    Script Date: 4/21/2020 11:02:29 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OutsideOIWalls](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Ticker] [varchar](10) NULL,
	[Maturity] [char](10) NULL,
	[IsMonthlyExp] [bit] NULL,
	[SumOI] [int] NULL,
	[PutOI] [int] NULL,
	[CallOI] [int] NULL,
	[PutStrike] [money] NULL,
	[StockPrice] [money] NULL,
	[CallStrike] [money] NULL
) ON [PRIMARY]
GO


