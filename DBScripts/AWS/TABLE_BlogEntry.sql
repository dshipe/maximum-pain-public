USE [MaxPainAPI]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='BlogEntry')
BEGIN
	DROP TABLE [Blog]
END

CREATE TABLE [BlogEntry]
(
	Id bigint identity
	,Title varchar(100)
	,ImageUrl varchar(100)
	,Ordinal int
	,IsActive bit
	,IsStockPick bit
	,ShowOnHome bit
	,CreatedOn DateTime
	,ModifiedOn DateTime
	,Content text
)

INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn, Content)
VALUES (
'Max Pain Video'
,'/assets/video.png'
,10
,1
,1
,GETUTCDATE()
,GETUTCDATE()
,'<p>This is a video explanation of max pain. It describes how max pain can be used to predict the stock price upon option expiration. In brief, max pain functions because of the hedge re-balancing by the market maker who wrote/ sold the option contracts. For a more detailed explanation, please watch the video below or see the various blog posts on this topic.</p>
<div style="text-align:center;">
<a href="https://www.youtube.com/watch?v=_pDeijxXMK0">
<img align="center" src="/assets/video.png"/>
</a>
</div>'
)

INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn, Content)
VALUES (
'How to Calculate Max Pain in Excel'
,'/assets/howToCalculateMaxPainInExcel.jpg'
,20
,1
,1
,GETUTCDATE()
,GETUTCDATE()
,'<p>This video describes how to calculate stock option max pain from option chain data using Excel.  You can also use Google sheets.</p>
<div style="text-align:center;">
<a href="https://youtu.be/udJFZIBdQh4">
<img align="center" src="/assets/howToCalculateMaxPainInExcel.jpg"/>
</a>
</div>'
)


SELECT * FROM BlogEntry