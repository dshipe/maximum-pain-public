USE [MaxPainAPI]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT [name] FROM SysObjects WHERE [name]='BlogXml')
BEGIN
	DROP TABLE [BlogXml]
END

CREATE TABLE [BlogXml]
(
	Id bigint identity
	,Content text
	,ModifiedOn DateTime
)

INSERT INTO BlogXml
(ModifiedOn, Content)
VALUES
(GetUTCDATE(),
'
<Blog>
<Item>
	<Title>How to Calculate Max Pain in Excel</Title>
	<Summary>This is a video explanation of max pain. It describes how max pain can be used to predict the stock price upon option expiration. In brief, max pain functions because of the hedge re-balancing by the market maker who wrote/ sold the option contracts. For a more detailed explanation, please watch the video below or see the various blog posts on this topic.</Summary>
	<Content>
&lt;p&gt;This is a video explanation of max pain. It describes how max pain can be used to predict the stock price upon option expiration. In brief, max pain functions because of the hedge re-balancing by the market maker who wrote&#x2F; sold the option contracts. For a more detailed explanation, please watch the video below or see the various blog posts on this topic.&lt;&#x2F;p&gt;

&lt;div style&#x3D;&quot;text-align:center;&quot;&gt;
&lt;a href&#x3D;&quot;https:&#x2F;&#x2F;www.youtube.com&#x2F;watch?v&#x3D;_pDeijxXMK0&quot;&gt;
&lt;img align&#x3D;&quot;center&quot; src&#x3D;&quot;&#x2F;assets&#x2F;video.png&quot;&#x2F;&gt;
&lt;&#x2F;a&gt;
&lt;&#x2F;div&gt;	
	</Content>
	</ImageLink>/assets/video.png</ImageLink>
</Item>
<Item>
	<Title>How to Calculate Max Pain in Excel</Title>
	<Summary>This video describes how to calculate stock option max pain from option chain data using Excel.  You can also use Google sheets.</Summary>
	<Content>
&lt;p&gt;This video describes how to calculate stock option max pain from option chain data using Excel.  You can also use Google sheets.&lt;&#x2F;p&gt;

&lt;div style&#x3D;&quot;text-align:center;&quot;&gt;
&lt;a href&#x3D;&quot;https:&#x2F;&#x2F;youtu.be&#x2F;udJFZIBdQh4&quot;&gt;
&lt;img align&#x3D;&quot;center&quot; src&#x3D;&quot;&#x2F;assets&#x2F;howToCalculateMaxPainInExcel.jpg&quot;&#x2F;&gt;
&lt;&#x2F;a&gt;
&lt;&#x2F;div&gt;	
	</Content>
	</ImageLink>/assets/howToCalculateMaxPainInExcel.jpg</ImageLink>
</Item>
</Blog>
'
)

SELECT Convert(xml,Content) FROM BlogXml