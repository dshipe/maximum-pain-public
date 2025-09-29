USE MaxPainAPI
GO

/****** Object:  Table [dbo].[MostActive]    Script Date: 4/21/2020 11:01:26 AM ******/
DROP TABLE [dbo].[Hop]
GO

/****** Object:  Table [dbo].[MostActive]    Script Date: 4/21/2020 11:01:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Hop](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Destination] varchar(255) NULL,
	[Referrer] varchar(255) NULL,
	[UserAgent] varchar(255) NULL,
	[CreatedOn] [smalldatetime] NULL
) ON [PRIMARY]
GO


