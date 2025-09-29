USE FIN
GO

-- DELETE FROM HistoricalOptionQuoteXml WHERE CreatedOn > '11/2/2020'

-- drop linked server
IF EXISTS( SELECT * FROM SysServers WHERE SrvName='AWS')
BEGIN
	EXEC sp_dropserver 'AWS', 'droplogins';
END

-- add linked server
IF NOT EXISTS( SELECT * FROM SysServers WHERE SrvName='AWS')
BEGIN
	EXEC sp_addlinkedserver
		@server='AWS'
		,@srvproduct=N''
		,@datasrc='35.172.202.150,1433'
		,@provider=N'SQLNCLI'
END
	
INSERT INTO HistoricalOptionQuoteXml (Ticker, CreatedOn, Content)
SELECT Ticker, CreatedOn, Content -- , CONVERT(XML, Content) as ContentXML 
FROM [AWS].MaxPainAPI.dbo.vwHistoricalOptionQuoteVarchar 
WHERE CreatedOn>'11/26/2020'
