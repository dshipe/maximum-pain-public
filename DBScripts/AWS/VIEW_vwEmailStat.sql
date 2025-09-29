USE [MaxPainAPI]
GO

/****** Object:  View [dbo].[vwEmailStat]    Script Date: 4/21/2020 10:13:19 AM ******/
DROP VIEW [dbo].[vwEmailStat]
GO

/****** Object:  View [dbo].[vwEmailStat]    Script Date: 4/21/2020 10:13:19 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwEmailStat]
AS

SELECT TOP 999999
	--CreatedOnDay
	DATEADD(m, DATEDIFF(m, 0, CreatedOnDay), 0) as [Date] 
	--DATEADD(WEEK, DATEDIFF(WEEK, CreatedOnDay, CURRENT_TIMESTAMP), CreatedOnDay) as [Date] 
    , SUM([active]) as Active
    , SUM([confirmed]) as Confirmed
    , SUM([unsubscribed]) as Unsubscribed
    , SUM([honeypot]) as Honeypot
FROM 
(
	SELECT *
	FROM
	(
		SELECT
			T.CreatedOnDay
			,ES.Description
			,COUNT(*) AS Records
		FROM
		(
			SELECT 
				CONVERT(VARCHAR(10), CreatedOn, 120) AS CreatedOnDay
				,*
			FROM EmailAccount WITH(NOLOCK)
		) AS T
		INNER JOIN EmailStatus ES ON T.EmailStatusID=ES.ID
		WHERE T.CreatedOn > '1/1/2018'
		GROUP BY 
			T.CreatedOnDay
			,ES.Description 
	) AS X
	pivot
	(
		sum(records)
		for Description in ([active], [confirmed], [unsubscribed], [honeypot])
	) PIV
) as RESULT
GROUP BY DATEADD(m, DATEDIFF(m, 0, CreatedOnDay), 0) 
WITH ROLLUP
ORDER BY [Date] DESC
GO


