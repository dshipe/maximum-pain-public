USE MaxPainAPI
GO

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
		,@datasrc='xx-server-xx'
		,@provider=N'SQLNCLI'
END
	