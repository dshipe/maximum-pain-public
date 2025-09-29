USE [fin]
GO

/****** Object:  Table [dbo].[MarketCalendar]    Script Date: 4/21/2020 11:01:26 AM ******/
DROP TABLE [dbo].[MarketCalendar]
GO

/****** Object:  Table [dbo].[MarketCalendar]    Script Date: 4/21/2020 11:01:26 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MarketCalendar](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Date] [smalldatetime] NULL
) ON [PRIMARY]
GO

INSERT INTO MarketCalendar ([Date]) VALUES ('12/31/2020')

--SELECT * FROM MarketCalendar