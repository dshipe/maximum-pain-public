USE Fin
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[spPythonOIWalls]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[spPythonOIWalls]	
GO

CREATE PROCEDURE dbo.spPythonOIWalls
	@Ticker VARCHAR(10) =NULL
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	rebuild ML Data Set

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.


	Revision History:

	Date		Name	Description
	----		----	-----------
	2020.11.08	DES		Initial Code
********* ********* ********* ********* *********
*/
BEGIN	
	SET NOCOUNT ON

	IF OBJECT_ID('tempdb..#POIData') IS NOT NULL DROP TABLE #POIData
	CREATE TABLE #POIData 
	(
		Id INT IDENTITY
		,Ticker VARCHAR(10)
		,TickerID INT
		,Maturity SMALLDATETIME
		,[Date] SMALLDATETIME
		,DaysToExp INT
		,LastPrice MONEY
		,ClosePrice MONEY
		,TargetPrice MONEY
		,MaxPain MONEY
		,HighCallOI MONEY
		,HighPutOI MONEY
		,TotalCallOI INT
		,TotalPutOI INT
		,IsOutside BIT
	)

	INSERT INTO #POIData 
	(
		Ticker
		,Maturity
		,[Date]
		,DaysToExp
		,LastPrice
		,ClosePrice
		,TargetPrice
		,MaxPain
		,HighCallOI
		,HighPutOI
		,TotalCallOI
		,TotalPutOI
		,IsOutside
	)
	SELECT
		ds.Ticker 
		,ds.Maturity 
		,ds.[Date] 
		,ds.DaysToExp 
		,ds.LastPrice 
		,ds.ClosePrice 
		,ds.TargetPrice 
		,ds.MaxPain 
		,ds.HighCallOI 
		,ds.HighPutOI 
		,ds.TotalCallOI 
		,ds.TotalPutOI 
		,ds.IsOutside 
	FROM MLDataSet ds
	LEFT JOIN (
		SELECT Ticker, Maturity
		FROM MLDataSet
		WHERE IsOutside=1
		GROUP BY Ticker, Maturity
	) AS x
		ON ds.Ticker=x.Ticker
		AND ds.Maturity=x.Maturity
	WHERE (ds.Ticker=@Ticker OR @Ticker IS NULL)
	AND ds.Maturity<'12/24/2020'
	AND TargetPrice IS NOT NULL
	ORDER BY ds.Ticker, ds.[Maturity], ds.[Date]

	IF OBJECT_ID('tempdb..#POIOutput') IS NOT NULL DROP TABLE #POIOutput
	CREATE TABLE #POIOutput 
	(
		Id INT IDENTITY
		,Ticker VARCHAR(10)
		,Maturity SMALLDATETIME
		,TargetPrice MONEY

		,LastPrice_7 MONEY
		,ClosePrice_7 MONEY
		,MaxPain_7 MONEY
		,HighCallOI_7 MONEY
		,HighPutOI_7 MONEY
		,TotalCallOI_7 INT
		,TotalPutOI_7 INT

		,LastPrice_4 MONEY
		,ClosePrice_4 MONEY
		,MaxPain_4 MONEY
		,HighCallOI_4 MONEY
		,HighPutOI_4 MONEY
		,TotalCallOI_4 INT
		,TotalPutOI_4 INT

		,LastPrice_3 MONEY
		,ClosePrice_3 MONEY
		,MaxPain_3 MONEY
		,HighCallOI_3 MONEY
		,HighPutOI_3 MONEY
		,TotalCallOI_3 INT
		,TotalPutOI_3 INT

		,LastPrice_2 MONEY
		,ClosePrice_2 MONEY
		,MaxPain_2 MONEY
		,HighCallOI_2 MONEY
		,HighPutOI_2 MONEY
		,TotalCallOI_2 INT
		,TotalPutOI_2 INT

		,MinValue MONEY
		,MaxValue MONEY
		,WasOutside BIT
		,Result BIT
	)

	INSERT INTO #POIOutput
	(
		Ticker
		,Maturity
		,TargetPrice
		
		,LastPrice_7 
		,ClosePrice_7 
		,MaxPain_7 
		,HighCallOI_7 
		,HighPutOI_7 
		,TotalCallOI_7 
		,TotalPutOI_7 
	)
	SELECT
		Ticker
		,Maturity
		,TargetPrice

		,LastPrice
		,ClosePrice
		,MaxPain
		,HighCallOI
		,HighPutOI
		,TotalCallOI
		,TotalPutOI
	FROM #POIData 
	WHERE DaysToExp=7
	ORDER BY Ticker,Maturity

	UPDATE #POIOutput
		SET LastPrice_4 = d.LastPrice 
		,ClosePrice_4 = d.ClosePrice 
		,MaxPain_4 = d.MaxPain
		,HighCallOI_4 = d.HighCallOI
		,HighPutOI_4 = d.HighPutOI
		,TotalCallOI_4 = d.TotalCallOI
		,TotalPutOI_4 = d.TotalPutOI
	FROM #POIOutput o
	INNER JOIN #POIData d
		ON o.Ticker=d.Ticker
		AND o.Maturity=d.Maturity
		AND d.DaysToExp=4

	UPDATE #POIOutput
		SET LastPrice_3 = d.LastPrice 
		,ClosePrice_3 = d.ClosePrice 
		,MaxPain_3 = d.MaxPain
		,HighCallOI_3 = d.HighCallOI
		,HighPutOI_3 = d.HighPutOI
		,TotalCallOI_3 = d.TotalCallOI
		,TotalPutOI_3 = d.TotalPutOI
	FROM #POIOutput o
	INNER JOIN #POIData d
		ON o.Ticker=d.Ticker
		AND o.Maturity=d.Maturity
		AND d.DaysToExp=3

	UPDATE #POIOutput
		SET LastPrice_2 = d.LastPrice 
		,ClosePrice_2 = d.ClosePrice 
		,MaxPain_2 = d.MaxPain
		,HighCallOI_2 = d.HighCallOI
		,HighPutOI_2 = d.HighPutOI
		,TotalCallOI_2 = d.TotalCallOI
		,TotalPutOI_2 = d.TotalPutOI
	FROM #POIOutput o
	INNER JOIN #POIData d
		ON o.Ticker=d.Ticker
		AND o.Maturity=d.Maturity
		AND d.DaysToExp=2



	-- find the highest PUT and CALL
	UPDATE #POIOutput
		SET MinValue=x.MinValue
		,MaxValue=x.MaxValue
		,WasOutside=x.WasOutside
	FROM #POIOutput o
	INNER JOIN (
		SELECT
			Ticker
			,Maturity
			,MIN(HighPutOI) AS MinValue
			,MAX(HighCallOI) AS MaxValue
			,MAX(CONVERT(INT,IsOutside)) AS WasOutside
		FROM #POIData d
		WHERE d.DaysToExp>=2
		GROUP BY Ticker, Maturity
	) AS x
		ON o.Ticker=x.Ticker
		AND o.Maturity=x.Maturity

	-- add the result
	UPDATE #POIOutput
		SET Result = CASE
			WHEN WasOutside=1 AND TargetPrice BETWEEN MinValue AND MaxValue THEN 1
			ELSE 0
		END

	-- remove NULLs
	DELETE FROM #POIOutput WHERE LastPrice_4 IS NULL
	DELETE FROM #POIOutput WHERE LastPrice_3 IS NULL
	DELETE FROM #POIOutput WHERE LastPrice_2 IS NULL
	

	SELECT 
		Ticker 
		,Maturity 

		,LastPrice_7 
		,ClosePrice_7 
		,MaxPain_7 
		,HighCallOI_7 
		,HighPutOI_7 
		,TotalCallOI_7 
		,TotalPutOI_7 

		,LastPrice_4 
		,ClosePrice_4 
		,MaxPain_4 
		,HighCallOI_4 
		,HighPutOI_4 
		,TotalCallOI_4 
		,TotalPutOI_4 

		,LastPrice_3 
		,ClosePrice_3 
		,MaxPain_3 
		,HighCallOI_3 
		,HighPutOI_3 
		,TotalCallOI_3 
		,TotalPutOI_3 

		,LastPrice_2 
		,ClosePrice_2 
		,MaxPain_2 
		,HighCallOI_2 
		,HighPutOI_2 
		,TotalCallOI_2 
		,TotalPutOI_2 

		,Result 
	FROM #POIOutput
	ORDER BY Ticker, Maturity

	RETURN 1
END
GO


EXEC spPythonOIWalls --@Ticker='AAPL'


