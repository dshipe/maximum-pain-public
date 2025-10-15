USE Fin
GO

if exists (select * from dbo.sysobjects where name='[ImportMaxPainXml]')
	drop table [dbo].ImportMaxPainXml	
GO

CREATE TABLE ImportMaxPainXml (
	ID BIGINT IDENTITY
	, Content XML
	, CreatedOn SMALLDATETIME
) 

