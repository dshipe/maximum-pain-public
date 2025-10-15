USE [MaxPainAPI]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='Message')
BEGIN
	DROP TABLE [Message]
END

CREATE TABLE [Message]
(
	Id bigint identity
	,[Subject] varchar(200)
	,Body varchar(max)
	,CreatedOn DateTime
)

INSERT INTO [Message] ([Subject], Body, CreatedOn) VALUES ('subject #1', 'body #1', GETUTCDATE())
INSERT INTO [Message] ([Subject], Body, CreatedOn) VALUES ('subject #2', 'body #2', GETUTCDATE())
INSERT INTO [Message] ([Subject], Body, CreatedOn) VALUES ('subject #3', 'body #3', GETUTCDATE())

SELECT * FROM [Message]