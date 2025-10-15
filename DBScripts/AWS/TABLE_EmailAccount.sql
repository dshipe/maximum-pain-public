USE [MaxPainAPI]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='EmailStatus')
BEGIN
	DROP TABLE EmailStatus
END

CREATE TABLE EmailStatus
(
	Id int
	,[Description] varchar(100)
)

INSERT INTO EmailStatus(Id, [Description])
SELECT EmailStatusID, [Description]
FROM MaxPain.dbo.EmailStatus


IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='EmailAccount')
BEGIN
	DROP TABLE EmailAccount
END

CREATE TABLE EmailAccount
(
	Id bigint identity
	,Email varchar(100) not null
	,[Name] varchar(100)
	,[EmailStatusId] int not null
	,[CreatedOn] DateTime
	,[ModifiedOn] DateTime
	,[LastEmaiLSent] DateTime
)

--SET IDENTITY_INSERT EmailAccount ON

INSERT INTO EmailAccount(Email, [Name], EmailStatusId, CreatedOn, ModifiedOn, LastEmailSent)
SELECT Email, [Name], EmailStatusId, CreatedOn, ModifiedOn, LastEmailSent
FROM MaxPain.dbo.EmailList
WHERE EmailStatusId IN (2,3,4)

--SET IDENTITY_INSERT EmailAccount OFF


SELECT * 
FROM EmailAccount EA
INNER JOIN EmailStatus ES ON EA.EmailStatusId = ES.Id
ORDER BY EA.Id
