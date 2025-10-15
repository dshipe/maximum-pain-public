USE [MaxPainAPI]
GO

/****** Object:  Table [dbo].[OptionChainJson]    Script Date: 6/20/2019 10:06:14 PM ******/
DROP TABLE [dbo].[OptionChainJson]
GO

/****** Object:  Table [dbo].[OptionChainJson]    Script Date: 6/20/2019 10:06:14 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OptionChainJson](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Ticker] [varchar](5) NULL,
	[Content] [text] NULL,
	[ModifiedOn] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


