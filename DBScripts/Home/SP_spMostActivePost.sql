USE [fin]
GO

/****** Object:  StoredProcedure [dbo].[spMostActivePost]    Script Date: 7/7/2021 9:48:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


IF EXISTS(SELECT * FROM SYS.procedures WHERE [Name]='spMostActivePost')
BEGIN
	DROP PROCEDURE spMostActivePost
END
GO

CREATE PROCEDURE [dbo].[spMostActivePost]
	@xml TEXT
AS
/*
********* ********* ********* ********* *********
	Copyright (c) 2011 - Dan Shipe

	This SP is responsible for parsing the option data and inserting or 
	updating records in the database

	Returns: A resultset of error messages (or empty resultset when no errors)

	Return Code:

	Code	Meaning
	----	-------
	1	Success - Actual status returned via recordset.

	Revision History:

	Date		Name	Description
	----		----	-----------
	2012.10.29	DES	Initial Code
********* ********* ********* ********* *********
*/
BEGIN
	SET NOCOUNT ON

	DECLARE @CurrentDate SMALLDATETIME
	SELECT @CurrentDate = GETUTCDATE()

	/*
	********* ********* ********* ********* *********
	translate XML into a table var
	********* ********* ********* ********* *********
	*/

	DECLARE @tblXml TABLE
	(
		[SortId]				[int] 
		,[Type]					VARCHAR(50)
		,[TypeId]				INT
		,[Ticker]				[varchar](10) 
		,[Maturity]				[datetime] 
		,[CallPut]				[char](1) 
		,[Strike]				VARCHAR(50)  
		,[OpenInterest]			VARCHAR(50)
		,[PrevOpenInterest]		VARCHAR(50)
		,[ChangeOpenInterest]	VARCHAR(50) 
		,[Volume]				VARCHAR(50)
		,[PrevVolume]			VARCHAR(50)
		,[ChangeVolume]			VARCHAR(50)  
		,[Price]				VARCHAR(50) 
		,[PrevPrice]			VARCHAR(50)  
		,[ChangePrice]			VARCHAR(50) 
		,[IV]					VARCHAR(50)  
		,[PrevIV]				VARCHAR(50) 
		,[ChangeIV]				VARCHAR(50) 
		,[CreatedOn]			VARCHAR(50) 
		,[NextMaturity]			VARCHAR(50)
		,[StockPrice]			VARCHAR(50)  
		,[MaxPain]				VARCHAR(50)  
		,[QueryType]			VARCHAR(50)
	)

	DECLARE @idoc INT
	EXEC sp_xml_preparedocument @idoc OUTPUT, @xml

	-- use generic column		
	INSERT INTO @tblXml 
	(
		[SortId]
		,[Type]
		,[Ticker]
		,[Maturity]
		,[CallPut]
		,[Strike]
		,[OpenInterest]
		,[PrevOpenInterest]
		,[ChangeOpenInterest]
		,[Volume]
		,[PrevVolume]
		,[ChangeVolume]
		,[Price]
		,[PrevPrice]
		,[ChangePrice]
		,[IV]
		,[PrevIV]
		,[ChangeIV]
		,[CreatedOn]
		,[NextMaturity]
		,[StockPrice]
		,[MaxPain]
		,[QueryType]
	)
	SELECT
		[SortId]
		,[Type]
		,[Ticker]
		,[Maturity]
		,[CallPut]
		,[Strike]
		,[OpenInterest]
		,[PrevOpenInterest]
		,[ChangeOpenInterest]
		,[Volume]
		,[PrevVolume]
		,[ChangeVolume]
		,[Price]
		,[PrevPrice]
		,[ChangePrice]
		,[IV]
		,[PrevIV]
		,[ChangeIV]
		,[CreatedOn]
		,[NextMaturity]
		,[StockPrice]
		,[MaxPain]
		,[QueryType]
	FROM     
	OPENXML (@idoc, '/ArrayOfMostActive/MostActive', 2)
	WITH
	(
		[SortId]				[int]				'SortID'
		,[Type]					VARCHAR(50)			'Type' 
		,[Ticker]				[varchar](10)		'Ticker'
		,[Maturity]				[smalldatetime]		'Maturity' 
		,[CallPut]				[char](1)			'CallPut' 
		,[Strike]				VARCHAR(50)			'Strike' 
		,[OpenInterest]			VARCHAR(50)			'OpenInterest' 
		,[PrevOpenInterest]		VARCHAR(50)			'PrevOpenInterest' 
		,[ChangeOpenInterest]	VARCHAR(50)			'ChangeOpenInterest' 
		,[Volume]				VARCHAR(50)			'Volume' 
		,[PrevVolume]			VARCHAR(50) 		'PrevVolume'
		,[ChangeVolume]			VARCHAR(50)			'ChangeVolume' 
		,[Price]				VARCHAR(50)			'Price' 
		,[PrevPrice]			VARCHAR(50)		 	'PrevPrice'
		,[ChangePrice]			VARCHAR(50)		 	'ChangePrice'
		,[IV]					VARCHAR(50)			'IV' 
		,[PrevIV]				VARCHAR(50)			'PrevIV' 
		,[ChangeIV]				VARCHAR(50)			'ChangeIV' 
		,[CreatedOn]			VARCHAR(50)			'CreatedOn'
		,[NextMaturity]			VARCHAR(50)			'NextMaturity'
		,[StockPrice]			VARCHAR(50)		 	'StockPrice'
		,[MaxPain]				VARCHAR(50)		 	'MaxPain'
		,[QueryType]			VARCHAR(50) 		'QueryType'
	)

	EXEC sp_xml_removedocument @idoc

    UPDATE @tblXml SET TypeId = CASE
        WHEN [Type]='ChangeOpenInterest' THEN 1
        WHEN [Type]='ChangePrice' THEN 2
        WHEN [Type]='ChangeVolume' THEN 3
        WHEN [Type]='OpenInterest' THEN 4 
        WHEN [Type]='Volume' THEN 5
	END

	--SELECT * FROM @tblXml
		
	/*
	********* ********* ********* ********* *********
	insert 
	if this is the first time option data was fetched today
	********* ********* ********* ********* *********
	*/

	INSERT INTO [MostActive] 
	(
		[SortId]
		,[Type]
		,[Ticker]
		,[Maturity]
		,[CallPut]
		,[Strike]
		,[OpenInterest]
		,[PrevOpenInterest]
		,[ChangeOpenInterest]
		,[Volume]
		,[PrevVolume]
		,[ChangeVolume]
		,[Price]
		,[PrevPrice]
		,[ChangePrice]
		,[IV]
		,[PrevIV]
		,[ChangeIV]
		,[CreatedOn]
		,[NextMaturity]
		,[StockPrice]
		,[MaxPain]
		,[QueryType]
	)	
	SELECT
		[SortId]
		,[TypeId]
		,[Ticker]
		,[Maturity]
		,[CallPut]
		,[Strike]
		,[OpenInterest]
		,[PrevOpenInterest]
		,[ChangeOpenInterest]
		,[Volume]
		,[PrevVolume]
		,[ChangeVolume]
		,[Price]
		,[PrevPrice]
		,[ChangePrice]
		,[IV]
		,[PrevIV]
		,[ChangeIV]
		,@CurrentDate --,CONVERT(SMALLDATETIME,[CreatedOn])
		,[NextMaturity]
		,[StockPrice]
		,[MaxPain]
		,[QueryType]
	FROM @tblXml x
	ORDER BY SortId

	--select @@ROWCOUNT as inserted

	RETURN 1
END
GO

BEGIN TRANSACTION
EXEC spMostActivePost @xml='
<ArrayOfMostActive><MostActive><Id>0</Id><SortID>1</SortID><Type>ChangeOpenInterest</Type><Ticker>NCLH</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>25</Strike><CallPut>P</CallPut><OpenInterest>927</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>322</Volume><PrevVolume>854</PrevVolume><Price>0.03</Price><PrevPrice>0.02</PrevPrice><IV>61.387001037597656</IV><PrevIV>64.41500091552734</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>92600</ChangeOpenInterest><ChangeVolume>-62.3</ChangeVolume><ChangePrice>50</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>2</SortID><Type>ChangeOpenInterest</Type><Ticker>CZR</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>99</Strike><CallPut>C</CallPut><OpenInterest>2574</OpenInterest><PrevOpenInterest>3</PrevOpenInterest><Volume>64</Volume><PrevVolume>2646</PrevVolume><Price>0.36</Price><PrevPrice>1.4</PrevPrice><IV>43.650001525878906</IV><PrevIV>44.15800094604492</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>85700</ChangeOpenInterest><ChangeVolume>-97.58</ChangeVolume><ChangePrice>-74.29</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>3</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3620</Strike><CallPut>P</CallPut><OpenInterest>2110</OpenInterest><PrevOpenInterest>3</PrevOpenInterest><Volume>2303</Volume><PrevVolume>5581</PrevVolume><Price>6.0</Price><PrevPrice>21.2</PrevPrice><IV>24.054000854492188</IV><PrevIV>28.816999435424805</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>70233.33</ChangeOpenInterest><ChangeVolume>-58.73</ChangeVolume><ChangePrice>-71.7</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>4</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3930</Strike><CallPut>C</CallPut><OpenInterest>301</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>597</Volume><PrevVolume>755</PrevVolume><Price>2.1</Price><PrevPrice>4.2</PrevPrice><IV>40.75299835205078</IV><PrevIV>42.74700164794922</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>30000</ChangeOpenInterest><ChangeVolume>-20.93</ChangeVolume><ChangePrice>-50</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>5</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3680</Strike><CallPut>P</CallPut><OpenInterest>464</OpenInterest><PrevOpenInterest>2</PrevOpenInterest><Volume>3448</Volume><PrevVolume>1532</PrevVolume><Price>18.78</Price><PrevPrice>44.65</PrevPrice><IV>21.371000289916992</IV><PrevIV>29.152000427246094</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>23100</ChangeOpenInterest><ChangeVolume>125.07</ChangeVolume><ChangePrice>-57.94</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>6</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3640</Strike><CallPut>P</CallPut><OpenInterest>433</OpenInterest><PrevOpenInterest>2</PrevOpenInterest><Volume>1472</Volume><PrevVolume>2014</PrevVolume><Price>8.43</Price><PrevPrice>26.0</PrevPrice><IV>22.827999114990234</IV><PrevIV>28.89900016784668</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>21550</ChangeOpenInterest><ChangeVolume>-26.91</ChangeVolume><ChangePrice>-67.58</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>7</SortID><Type>ChangeOpenInterest</Type><Ticker>GOOG</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>2512.5</Strike><CallPut>P</CallPut><OpenInterest>4394</OpenInterest><PrevOpenInterest>21</PrevOpenInterest><Volume>29</Volume><PrevVolume>4593</PrevVolume><Price>0.52</Price><PrevPrice>1.6</PrevPrice><IV>0</IV><PrevIV>21.55900001525879</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>20823.81</ChangeOpenInterest><ChangeVolume>-99.37</ChangeVolume><ChangePrice>-67.5</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>8</SortID><Type>ChangeOpenInterest</Type><Ticker>V</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>257.5</Strike><CallPut>C</CallPut><OpenInterest>209</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>20</Volume><PrevVolume>209</PrevVolume><Price>0.01</Price><PrevPrice>0.02</PrevPrice><IV>32.80799865722656</IV><PrevIV>30.690000534057617</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>20800</ChangeOpenInterest><ChangeVolume>-90.43</ChangeVolume><ChangePrice>-50</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>9</SortID><Type>ChangeOpenInterest</Type><Ticker>GOOG</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>2507.5</Strike><CallPut>P</CallPut><OpenInterest>4339</OpenInterest><PrevOpenInterest>21</PrevOpenInterest><Volume>27</Volume><PrevVolume>4394</PrevVolume><Price>0.46</Price><PrevPrice>1.75</PrevPrice><IV>0</IV><PrevIV>22.22599983215332</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>20561.9</ChangeOpenInterest><ChangeVolume>-99.39</ChangeVolume><ChangePrice>-73.71</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>10</SortID><Type>ChangeOpenInterest</Type><Ticker>VLO</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>65</Strike><CallPut>P</CallPut><OpenInterest>203</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>49</Volume><PrevVolume>208</PrevVolume><Price>0.03</Price><PrevPrice>0.03</PrevPrice><IV>58.27299880981445</IV><PrevIV>64.44999694824219</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>20200</ChangeOpenInterest><ChangeVolume>-76.44</ChangeVolume><ChangePrice>0</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>11</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3575</Strike><CallPut>P</CallPut><OpenInterest>384</OpenInterest><PrevOpenInterest>2</PrevOpenInterest><Volume>939</Volume><PrevVolume>1213</PrevVolume><Price>3.6</Price><PrevPrice>12.1</PrevPrice><IV>28.55900001525879</IV><PrevIV>30.850000381469727</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>19100</ChangeOpenInterest><ChangeVolume>-22.59</ChangeVolume><ChangePrice>-70.25</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>12</SortID><Type>ChangeOpenInterest</Type><Ticker>HAS</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>98.5</Strike><CallPut>C</CallPut><OpenInterest>192</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>0</Volume><PrevVolume>192</PrevVolume><Price>0.05</Price><PrevPrice>0.05</PrevPrice><IV>29.868999481201172</IV><PrevIV>20.58099937438965</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>19100</ChangeOpenInterest><ChangeVolume>-100</ChangeVolume><ChangePrice>0</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>13</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3610</Strike><CallPut>P</CallPut><OpenInterest>535</OpenInterest><PrevOpenInterest>3</PrevOpenInterest><Volume>1756</Volume><PrevVolume>3028</PrevVolume><Price>5.19</Price><PrevPrice>18.43</PrevPrice><IV>24.88800048828125</IV><PrevIV>29.45199966430664</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>17733.33</ChangeOpenInterest><ChangeVolume>-42.01</ChangeVolume><ChangePrice>-71.84</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>14</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3910</Strike><CallPut>C</CallPut><OpenInterest>356</OpenInterest><PrevOpenInterest>2</PrevOpenInterest><Volume>584</Volume><PrevVolume>890</PrevVolume><Price>2.35</Price><PrevPrice>4.7</PrevPrice><IV>38.54800033569336</IV><PrevIV>41.194000244140625</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>17700</ChangeOpenInterest><ChangeVolume>-34.38</ChangeVolume><ChangePrice>-50</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>15</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3630</Strike><CallPut>P</CallPut><OpenInterest>1474</OpenInterest><PrevOpenInterest>10</PrevOpenInterest><Volume>1633</Volume><PrevVolume>4339</PrevVolume><Price>7.3</Price><PrevPrice>23.35</PrevPrice><IV>23.194000244140625</IV><PrevIV>28.989999771118164</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>14640</ChangeOpenInterest><ChangeVolume>-62.36</ChangeVolume><ChangePrice>-68.74</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>16</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3660</Strike><CallPut>P</CallPut><OpenInterest>386</OpenInterest><PrevOpenInterest>3</PrevOpenInterest><Volume>1257</Volume><PrevVolume>1416</PrevVolume><Price>12.65</Price><PrevPrice>34.25</PrevPrice><IV>21.760000228881836</IV><PrevIV>28.683000564575195</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>12766.67</ChangeOpenInterest><ChangeVolume>-11.23</ChangeVolume><ChangePrice>-63.07</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>17</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3690</Strike><CallPut>P</CallPut><OpenInterest>252</OpenInterest><PrevOpenInterest>2</PrevOpenInterest><Volume>4087</Volume><PrevVolume>519</PrevVolume><Price>22.65</Price><PrevPrice>49.69</PrevPrice><IV>21.06599998474121</IV><PrevIV>29.2810001373291</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>12500</ChangeOpenInterest><ChangeVolume>687.48</ChangeVolume><ChangePrice>-54.42</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>18</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3650</Strike><CallPut>P</CallPut><OpenInterest>972</OpenInterest><PrevOpenInterest>8</PrevOpenInterest><Volume>7592</Volume><PrevVolume>3684</PrevVolume><Price>10.27</Price><PrevPrice>29.9</PrevPrice><IV>22.323999404907227</IV><PrevIV>28.625</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>12050</ChangeOpenInterest><ChangeVolume>106.08</ChangeVolume><ChangePrice>-65.65</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>19</SortID><Type>ChangeOpenInterest</Type><Ticker>MET</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>56</Strike><CallPut>P</CallPut><OpenInterest>545</OpenInterest><PrevOpenInterest>5</PrevOpenInterest><Volume>0</Volume><PrevVolume>544</PrevVolume><Price>0.06</Price><PrevPrice>0.06</PrevPrice><IV>35.29800033569336</IV><PrevIV>31.974000930786133</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>10800</ChangeOpenInterest><ChangeVolume>-100</ChangeVolume><ChangePrice>0</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>20</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3670</Strike><CallPut>P</CallPut><OpenInterest>327</OpenInterest><PrevOpenInterest>3</PrevOpenInterest><Volume>1939</Volume><PrevVolume>1753</PrevVolume><Price>15.4</Price><PrevPrice>39.0</PrevPrice><IV>21.22800064086914</IV><PrevIV>28.768999099731445</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>10800</ChangeOpenInterest><ChangeVolume>10.61</ChangeVolume><ChangePrice>-60.51</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>21</SortID><Type>ChangeOpenInterest</Type><Ticker>STZ</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>212.5</Strike><CallPut>P</CallPut><OpenInterest>100</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>0</Volume><PrevVolume>100</PrevVolume><Price>0.05</Price><PrevPrice>0.05</PrevPrice><IV>0</IV><PrevIV>28.46500015258789</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>9900</ChangeOpenInterest><ChangeVolume>-100</ChangeVolume><ChangePrice>0</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>22</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>4650</Strike><CallPut>C</CallPut><OpenInterest>94</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>107</Volume><PrevVolume>102</PrevVolume><Price>0.13</Price><PrevPrice>0.23</PrevPrice><IV>87.81300354003906</IV><PrevIV>79.4729995727539</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>9300</ChangeOpenInterest><ChangeVolume>4.9</ChangeVolume><ChangePrice>-43.48</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>23</SortID><Type>ChangeOpenInterest</Type><Ticker>HBI</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>17.5</Strike><CallPut>C</CallPut><OpenInterest>93</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>25</Volume><PrevVolume>230</PrevVolume><Price>0.17</Price><PrevPrice>0.53</PrevPrice><IV>32.678001403808594</IV><PrevIV>32.678001403808594</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>9200</ChangeOpenInterest><ChangeVolume>-89.13</ChangeVolume><ChangePrice>-67.92</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>24</SortID><Type>ChangeOpenInterest</Type><Ticker>SCHW</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>70.5</Strike><CallPut>C</CallPut><OpenInterest>87</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>92</Volume><PrevVolume>91</PrevVolume><Price>0.41</Price><PrevPrice>0.84</PrevPrice><IV>24.43000030517578</IV><PrevIV>23.155000686645508</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>8600</ChangeOpenInterest><ChangeVolume>1.1</ChangeVolume><ChangePrice>-51.19</ChangePrice></MostActive><MostActive><Id>0</Id><SortID>25</SortID><Type>ChangeOpenInterest</Type><Ticker>AMZN</Ticker><Maturity>2021-07-09T00:00:00</Maturity><Strike>3710</Strike><CallPut>P</CallPut><OpenInterest>75</OpenInterest><PrevOpenInterest>1</PrevOpenInterest><Volume>3046</Volume><PrevVolume>146</PrevVolume><Price>34.08</Price><PrevPrice>66.0</PrevPrice><IV>21.31399917602539</IV><PrevIV>29.415000915527344</PrevIV><CreatedOn>2021-07-08T00:09:30.0105789Z</CreatedOn><NextMaturity>true</NextMaturity><QueryType>0</QueryType><ChangeOpenInterest>7400</ChangeOpenInterest><ChangeVolume>1986.3</ChangeVolume><ChangePrice>-48.36</ChangePrice></MostActive></ArrayOfMostActive>
'

SELECT * FROM MostActive
ROLLBACK