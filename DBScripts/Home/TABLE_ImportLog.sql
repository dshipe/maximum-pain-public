/****** Object:  Table [dbo].[ImportLog]    Script Date: 3/17/2020 10:47:11 AM ******/
DROP TABLE [dbo].[ImportLog]
GO

/****** Object:  Table [dbo].[ImportLog]    Script Date: 3/17/2020 10:47:11 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ImportLog](
	[ID] bigint IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NULL,
	[Content] [text] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


