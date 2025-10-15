USE [MaxPainAPI]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='SettingsXml')
BEGIN
	DROP TABLE [SettingsXml]
END

CREATE TABLE [SettingsXml]
(
	Id bigint identity
	,Content text
	,ModifiedOn DateTime
)

INSERT INTO SettingsXml
(ModifiedOn, Content)
VALUES
(GetUTCDATE(),
'<Settings>
  <Setting Name="ScreenerLastRun" Value="2/2/2014 7:23:17 AM" />
</Settings>'
)

SELECT Convert(xml,Content) FROM SettingsXml