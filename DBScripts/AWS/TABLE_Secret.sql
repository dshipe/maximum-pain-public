USE [MaxPainAPI]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='Secret')
BEGIN
	DROP TABLE [Secret]
END

CREATE TABLE [Secret]
(
	Id bigint identity
	,Content text
	,ModifiedOn DateTime
)

INSERT INTO [Secret]
(ModifiedOn, Content)
VALUES
(GetUTCDATE(),
'
{	
	"TestKey":"TestValue",
}
'
)

SELECT * FROM [Secret]