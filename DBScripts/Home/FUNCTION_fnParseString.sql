
/****** Object:  UserDefinedFunction [dbo].[fnParseString]    Script Date: 3/14/2020 6:11:53 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[fnParseString] (@strValue varchar(8000), @Deliminator char(1))
RETURNS @tbReturnValue TABLE (ReturnValue varchar(200)) AS  
BEGIN 
	DECLARE  @Start int, @Found int, @Len int, @ReturnValue varchar(200)
	
	SELECT @Start = 1
	SELECT @Len = Len(@strValue)
	
	SELECT @Found = CHARINDEX(@Deliminator,@strValue,@Start)
	WHILE @Found > 0
	BEGIN
		SELECT @ReturnValue = SUBSTRING(@strValue, @Start, @Found - @Start)
		INSERT INTO @tbReturnValue (ReturnValue)  Values (@ReturnValue)
		
		SELECT @Start = @Found + 1
		SELECT @Found = CHARINDEX(@Deliminator,@strValue,@Start)
	END

	--Get Last Value
	SELECT @ReturnValue =  SUBSTRING(@strValue, @Start, @Len - @Start + 1)
	INSERT INTO @tbReturnValue 	(ReturnValue) 
	Values (@ReturnValue)
	
	RETURN
END

GO


