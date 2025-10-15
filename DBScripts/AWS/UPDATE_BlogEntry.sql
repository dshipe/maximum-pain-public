USE MaxPainAPI
GO

/*
SELECT
	'INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn, Content) VALUES ('
	+ ' ''' + Title + ''''
	+ ',''' + ImageUrl + ''''
	+ ',''' + CONVERT(VARCHAR(10), Ordinal) + ''''
	+ ',''' + CONVERT(VARCHAR(10), IsActive) + ''''
	+ ',''' + CONVERT(VARCHAR(10), ShowOnHome) + ''''
	--+ ',''' + CONVERT(VARCHAR(100), CreatedOn) + ''''
	--+ ',''' + CONVERT(VARCHAR(100), ModifiedOn) + ''''
	+ ',''' + CONVERT(VARCHAR(100), GETUTCDATE()) + ''''
	+ ',''' + CONVERT(VARCHAR(100), GETUTCDATE()) + ''''
	+ ',''' + REPLACE(CONVERT(VARCHAR(MAX), Content), '''', '''''') + ''''
	+ ')'
FROM BlogEntry WITH(NOLOCK)
ORDER BY Ordinal

SELECT
	'INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ('
	+ ' ''' + Title + ''''
	+ ',''' + ImageUrl + ''''
	+ ',''' + CONVERT(VARCHAR(10), Ordinal) + ''''
	+ ',''' + CONVERT(VARCHAR(10), IsActive) + ''''
	+ ',''' + CONVERT(VARCHAR(10), ShowOnHome) + ''''
	--+ ',''' + CONVERT(VARCHAR(100), CreatedOn) + ''''
	--+ ',''' + CONVERT(VARCHAR(100), ModifiedOn) + ''''
	+ ',''' + CONVERT(VARCHAR(100), GETUTCDATE()) + ''''
	+ ',''' + CONVERT(VARCHAR(100), GETUTCDATE()) + ''''
	--+ ',''' + REPLACE(CONVERT(VARCHAR(MAX), Content), '''', '''''') + ''''
	+ ')'
FROM BlogEntry WITH(NOLOCK)
ORDER BY Ordinal

*/

BEGIN TRANSACTION

DECLARE @Keep TABLE (Title VARCHAR(500), Content TEXT)
INSERT INTO @Keep (Title, Content) SELECT Title, Content FROM BlogEntry

TRUNCATE TABLE BlogEntry

INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Where Do I Start','/assets/question_mark_small.png','10','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Max Pain Video','/assets/video.png','20','1','1','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'How to Calculate Max Pain in Excel','/assets/howToCalculateMaxPainInExcel.jpg','30','1','1','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Google Sheets Option Chain','/assets/googleSheetsOptionChain.jpg','40','1','1','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Is Max Pain Accurate','/assets/OIWalls2.png','1000','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'How to Calculate Max Pain','assets/maxpain.png','1010','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Manually Calculating Max Pain','/assets/SpyPutCash-14-12-12.png','1020','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Demystifying Max Pain','/assets/maxpain.png','1030','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'How Max Pain Works','/assets/maxpain.png','1040','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Open Interest Walls','/assets/OIWalls2.png','1050','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')
INSERT INTO BlogEntry (Title, ImageUrl, Ordinal, IsActive, ShowOnHome, CreatedOn, ModifiedOn) VALUES ( 'Option Data Sources','/assets/google.jpg','1060','1','0','May  5 2020  8:43PM','May  5 2020  8:43PM')

UPDATE BlogEntry SET Content=K.Content
FROM BlogEntry BE WITH(NOLOCK)
INNER JOIN @Keep K ON BE.Title=K.Title

SELECT * FROM BlogEntry WITH(NOLOCK)

ROLLBACK
--COMMIT

