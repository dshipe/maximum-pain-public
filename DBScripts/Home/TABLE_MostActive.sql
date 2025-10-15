USE [fin]
GO

/****** Object:  Table [dbo].[MostActive]    Script Date: 4/21/2020 11:01:26 AM ******/
DROP TABLE [dbo].[MostActive]
GO

/****** Object:  Table [dbo].[MostActive]    Script Date: 4/21/2020 11:01:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MostActive](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SortID] [int] NULL,
	[Type] [int] NULL,
	QueryType VARCHAR(100) NULL,
	[Ticker] [varchar](10) NULL,
	[Maturity] [smalldatetime] NULL,
	[CallPut] [char](1) NULL,
	[Strike] [decimal](10, 2) NULL,
	[OpenInterest] [int] NULL,
	[PrevOpenInterest] [int] NULL,
	[ChangeOpenInterest] [decimal](10, 2) NULL,
	[Volume] [int] NULL,
	[PrevVolume] [int] NULL,
	[ChangeVolume] [decimal](10, 2) NULL,
	[Price] [decimal](10, 2) NULL,
	[PrevPrice] [decimal](10, 2) NULL,
	[ChangePrice] [decimal](10, 2) NULL,
	[IV] [float] NULL,
	[PrevIV] [float] NULL,
	[ChangeIV] [decimal](10, 2) NULL,
	[CreatedOn] [smalldatetime] NULL,
	[NextMaturity] [bit] NULL,
	[StockPrice] decimal(10,2) NULL,
	[MaxPain] decimal(10,2) NULL
) ON [PRIMARY]
GO


