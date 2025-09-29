USE Fin
GO

SELECT 'INSERT INTO StockTicker (Ticker, IsActive, CreatedOn, ModifiedOn) VALUES ('
	--+ ',''' + CONVERT(VARCHAR, StockTickerId) + ''''
	+ ' ''' + Ticker + ''''
	+ ',''' + CONVERT(VARCHAR, IsActive) + ''''
	+ ',''' + CONVERT(VARCHAR, CreatedOn) + ''''
	+ ',''' + CONVERT(VARCHAR, ModifiedOn) + ''''
	+ ')'
FROM StockTicker WITH(NOLOCK)
WHERE IsActive = 1
ORDER BY Ticker