USE Fin
GO


DROP PROCEDURE [dbo].[spImportStagingPost]
GO

CREATE PROCEDURE [dbo].[spImportStagingPost]
    @importDate DATETIME,
    @json NVARCHAR(MAX)
AS
BEGIN

    INSERT INTO ImportStaging (Content, ImportDate, CreatedOn)
    SELECT [value], @importDate, GETUTCDATE()
    FROM OPENJSON(@json)

    UPDATE ImportStaging SET Ticker = x.Stock
    FROM ImportStaging s
    CROSS APPLY OPENJSON(s.Content)
    WITH (
        Stock  VARCHAR(100) '$.Stock'
    ) x

END
GO

BEGIN TRANSACTION

TRUNCATE TABLE ImportStaging

EXEC [spImportStagingPost] @importDate='2026-03-20', @json = '[
{"Stock":"$SPX.X","StockPrice":3195.33,"InterestRate":0.1,"Volatility":29.0,"DaysToExpiration":0.0,"NumberOfContracts":16120,"CreatedOn":"2020-06-10T14:36:20.036397Z","Options":[
 {"ot":"SPX200619C00100000","p":2732.37,"c":-394.32,"b":3091.6,"a":3094.8,"oi":10,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":0.0,"ve":0.012,"rh":0.027}
,{"ot":"SPX200619C00200000","p":0.0,"c":0.0,"b":2991.8,"a":2995.0,"oi":0,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":-0.001,"ve":0.016,"rh":0.055}
,{"ot":"SPX200619C00300000","p":0.0,"c":0.0,"b":2891.8,"a":2895.0,"oi":0,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":-0.001,"ve":0.002,"rh":0.082}
,{"ot":"SPX200619C00400000","p":2432.52,"c":-394.18,"b":2791.8,"a":2795.0,"oi":3,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":-0.001,"ve":0.004,"rh":0.11}
]}
,{"Stock":"AAPL","StockPrice":3195.33,"InterestRate":0.1,"Volatility":29.0,"DaysToExpiration":0.0,"NumberOfContracts":16120,"CreatedOn":"2020-06-10T14:36:20.036397Z","Options":[
 {"ot":"AAPL200619C00100000","p":2732.37,"c":-394.32,"b":3091.6,"a":3094.8,"oi":10,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":0.0,"ve":0.012,"rh":0.027}
,{"ot":"AAPL200619C00200000","p":0.0,"c":0.0,"b":2991.8,"a":2995.0,"oi":0,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":-0.001,"ve":0.016,"rh":0.055}
,{"ot":"AAPL200619C00300000","p":0.0,"c":0.0,"b":2891.8,"a":2895.0,"oi":0,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":-0.001,"ve":0.002,"rh":0.082}
,{"ot":"AAPL200619C00400000","p":2432.52,"c":-394.18,"b":2791.8,"a":2795.0,"oi":3,"v":0,"iv":0.1,"de":1.0,"ga":0.0,"th":-0.001,"ve":0.004,"rh":0.11}
]}
]'



SELECT * FROM ImportStaging

ROLLBACK