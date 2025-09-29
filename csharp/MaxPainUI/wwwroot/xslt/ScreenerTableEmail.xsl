<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">


  <xsl:template match="/">
    <xsl:apply-templates select="Root" />
  </xsl:template>

  <xsl:template match="Root">
    <style type="text/css">
      body {font-family: "Helvetica Neue",Helvetica,Arial,sans-serif;}
      h3 {margin: 10px; padding: 0px; font-size: large; display: inline;}
      table {margin: 10px; padding: 0px;}
      th {padding: 3px; background-color: #cce0ff; font-weight: normal;}
      td {padding: 3px;}
      .TableBorder {border-left: 1px solid #ccc; border-top: 1px solid #ccc}
      .HeadBorder {border-right: 1px solid #ccc;}
      .CellBorder {border-right: 1px solid #ccc; border-bottom: 1px solid #ccc;}
      .FLeft { float: left; }
    </style>

    <div class="FLeft">
      <a rel="nofollow" href="https://maximum-pain.com/api/hop/cb?v=optionspop">
        <img src="http://maximum-pain.com/images/OptionsPop.png"/>
      </a>
    </div>

    <table border="0" cellspacing="0" cellpadding="0" style="border-left: 1px solid #fff; border-top: 1px solid #fff">
      <tr><td>
        This is the <a href="https://maximum-pain.com">maximum-pain.com</a><b> daily stock option screener</b>.

        <br/><br/>
        You are receiving this email because you opted in to our mailing list.  It will demonstrate which options have the largest changes in open interest, volume and price from the previous day.

        <br/><br/>
        The screener is run daily <b>after the market close</b>, and compares against the previous day.  The screener checks the symbols from the SP500.
      </td></tr>

      <xsl:apply-templates select="ArrayOfMostActive" mode="first" />
    </table>

    <table border="0" cellspacing="0" cellpadding="0" style="border-left: 1px solid #fff; border-top: 1px solid #fff">
      <xsl:apply-templates select="ArrayOfMostActive" mode="second" />

      <xsl:if test="string-length('@ChartLink')!=0">
        <tr><td><h3>Open Interest chart for <xsl:value-of select="@ChartTicker"/>.</h3></td></tr>
        <tr><td><a href="{@ChartLink}"><img width="100%" src="{@ChartImage}" /></a></td></tr>
      </xsl:if>

      <tr><td><h3>Outside Open Interest Walls</h3>  (<a href="https://maximum-pain.com/outside-oi-walls">see more</a>) (<a href="https://maximum-pain.com/blog/archive/open-interest-walls/">what does this mean?</a>)</td></tr>
      <tr><td><xsl:apply-templates select="ArrayOfOutsideOIWalls" /></td></tr>
	  
      <tr><td><xsl:text disable-output-escaping="yes"><![CDATA[&nbsp;]]></xsl:text></td></tr>
      <tr><td>Thank you,</td></tr>
      <tr><td>Dan</td></tr>
      <tr><td><xsl:text disable-output-escaping="yes"><![CDATA[&nbsp;]]></xsl:text></td></tr>
      <tr><td>Click here if you wish to <a href="{@UnsubscribeUrl}">unsubscribe</a> from the mailing list.</td></tr>

      <tr><td><div style="height:2000px; color: #fefefe;">text to keep link at bottom</div></td></tr>
    </table>
  </xsl:template>

  <xsl:template match="ArrayOfMostActive" mode="first">
    <tr><td><h3>Largest change in Price</h3> (<a href="https://maximum-pain.com/screener/ChangePrice">see more options</a>)</td></tr>
    <tr><td>
      <table border="0" cellpadding="0" cellspacing="0" class="AddBorder">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='ChangePrice']"></xsl:apply-templates>
      <xsl:text> </xsl:text></table>
    </td></tr>

    <tr><td><h3>Highest Open Interest</h3> (<a href="https://maximum-pain.com/screener/OpenInterest">see more options</a>)</td></tr>
    <tr><td>
      <table border="0" cellpadding="0" cellspacing="0" class="AddBorder">
      <xsl:choose>
        <xsl:when test="count(MostActive[NextMaturity='true' and QueryType='OpenInterest'])=0">
          <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='OpenInterest']"></xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='OpenInterest']"></xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:text> </xsl:text></table>
    </td></tr>
  </xsl:template>

  <xsl:template match="ArrayOfMostActive" mode="second">
    <tr><td><h3>Largest change in Open Interest</h3> (<a href="https://maximum-pain.com/screener/ChangeOpenInterest">see more options</a>)</td></tr>
    <tr><td>
      <table border="0" cellpadding="0" cellspacing="0" class="AddBorder">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='ChangeOpenInterest']"></xsl:apply-templates>
      <xsl:text> </xsl:text></table>
    </td></tr>

    <tr><td><h3>Highest volume</h3> (<a href="https://maximum-pain.com/screener/Volume">see more options</a>)</td></tr>
    <tr><td>
      <table border="0" cellpadding="0" cellspacing="0" class="AddBorder">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='Volume']"></xsl:apply-templates>
      <xsl:text> </xsl:text></table>
    </td></tr>

    <tr><td><h3>Largest change in volume</h3> (<a href="https://maximum-pain.com/screener/ChangeVolume">see more options</a>)</td></tr>
    <tr><td>
      <table border="0" cellpadding="0" cellspacing="0" class="AddBorder">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='ChangeVolume']"></xsl:apply-templates>
      <xsl:text> </xsl:text></table>
    </td></tr>
  </xsl:template>


  <xsl:template match="MostActive">

    <xsl:if test="position()&lt;4">
      <xsl:if test="position()=1">
        <tr>
          <th class="HeadBorder">Stock</th>
          <th class="HeadBorder">Maturity</th>
          <th class="HeadBorder">Date</th>
          <th class="HeadBorder">Type</th>
          <th class="HeadBorder">Strike</th>

          <xsl:choose>
            <xsl:when test="QueryType='ChangePrice'">
              <th class="HeadBorder">Price</th>
              <th class="HeadBorder">Prev. Price</th>
              <th class="HeadBorder">Change</th>
            </xsl:when>
            <xsl:when test="QueryType='OpenInterest' or QueryType='ChangeOpenInterest'">
              <th class="HeadBorder">OI</th>
              <th class="HeadBorder">Prev. OI</th>
              <th class="HeadBorder">Change</th>
              <th class="HeadBorder">Price</th>
              <th class="HeadBorder">Prev. Price</th>
            </xsl:when>
            <xsl:when test="QueryType='Volume' or QueryType='ChangeVolume'">
              <th class="HeadBorder">Volume</th>
              <th class="HeadBorder">Prev. Volume</th>
              <th class="HeadBorder">Change</th>
              <th class="HeadBorder">Price</th>
              <th class="HeadBorder">Prev. Price</th>
            </xsl:when>
          </xsl:choose>
        </tr>

      </xsl:if>


      <xsl:variable name="varPage">
        <xsl:choose>
          <xsl:when test="QueryType='OpenInterest' or QueryType='ChangeOpenInterest'">open-interest</xsl:when>
          <xsl:when test="QueryType='Volume' or QueryType='ChangeVolume'">volume</xsl:when>
          <xsl:otherwise>price</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="varType">
        <xsl:choose>
          <xsl:when test="CallPut='C'">
            <td class="CellBorder">call</td>
          </xsl:when>
          <xsl:when test="CallPut='P'">
            <td class="CellBorder">put</td>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>      

      <xsl:variable name="varMaturity">
          <xsl:call-template name="formatdate">
              <xsl:with-param name="dte" select="Maturity"/>
          </xsl:call-template>          
      </xsl:variable>      

      <xsl:variable name="varCreatedOn">
          <xsl:call-template name="formatdate">
              <xsl:with-param name="dte" select="CreatedOn"/>
          </xsl:call-template>          
      </xsl:variable>      

      <tr>
        <td class="CellBorder">
          <a href="https://maximum-pain.com/history/{Ticker}?m={$varMaturity}&amp;s={Strike}">
            <xsl:value-of select="Ticker"/>
          </a>
        </td>
        <td class="CellBorder">
			<xsl:value-of select="$varMaturity"/>
        </td>
        <td class="CellBorder">
			<xsl:value-of select="$varCreatedOn"/>
        </td>
        <td class="CellBorder">
          <xsl:value-of select="$varType"/>
        </td>
        <td class="CellBorder" align="right">
          <xsl:value-of select="Strike"/>
        </td>

        <xsl:choose>
          <xsl:when test="QueryType='ChangePrice'">
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(Price, '$###,##0.00')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(PrevPrice, '$###,##0.00')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(ChangePrice, '###,##0%')"/>
            </td>
          </xsl:when>
          <xsl:when test="QueryType='OpenInterest' or QueryType='ChangeOpenInterest'">
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(OpenInterest, '###,###,###,###,##0')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(PrevOpenInterest, '###,###,###,###,##0')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(ChangeOpenInterest, '###,##0%')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(Price, '$###,##0.00')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(PrevPrice, '$###,##0.00')"/>
            </td>
          </xsl:when>
          <xsl:when test="QueryType='Volume' or QueryType='ChangeVolume'">
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(Volume, '###,###,###,###,##0')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(PrevVolume, '###,###,###,###,##0')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(ChangeVolume, '###,##0%')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(Price, '$###,##0.00')"/>
            </td>
            <td class="CellBorder" align="right">
              <xsl:value-of select="format-number(PrevPrice, '$###,##0.00')"/>
            </td>
          </xsl:when>
        </xsl:choose>
      </tr>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ArrayOfOutsideOIWalls">
    <table border="0" cellpadding="0" cellspacing="0" class="AddBorder">
      <xsl:apply-templates select="OutsideOIWalls"></xsl:apply-templates>
    </table>
  </xsl:template>

  <xsl:template match="OutsideOIWalls">
    <xsl:if test="position()=1">
      <tr>
        <th class="HeadBorder">Stock</th>
        <th class="HeadBorder">Maturity</th>
        <th class="HeadBorder">Total Open Interest</th>
        <th class="HeadBorder">High Put Strike</th>
        <th class="HeadBorder">High Call Strike</th>
        <th class="HeadBorder">Stock Price</th>
      </tr>
    </xsl:if>

    <xsl:if test="position()&lt;4">
    <tr>
      <td class="CellBorder">
        <a href="https://maximum-pain.com/options/{Ticker}?m={Maturity}">
          <xsl:value-of select="Ticker"/>
        </a>
      </td>
      <td class="CellBorder">
        <xsl:value-of select="Maturity"/>
      </td>
      <td class="CellBorder" align="right">
        <xsl:value-of select="format-number(SumOI, '#,##0')"/>
      </td>
      <td class="CellBorder" align="right">
        <xsl:value-of select="format-number(PutStrike, '$#,##0.00')"/>
      </td>
      <td class="CellBorder" align="right">
        <xsl:value-of select="format-number(CallStrike, '$#,##0.00')"/>
      </td>
      <td class="CellBorder" align="right">
        <xsl:value-of select="format-number(StockPrice, '$#,##0.00')"/>
      </td>
    </tr>
    </xsl:if>
  </xsl:template>  

  <xsl:template match="ArrayOfImportMaxPain">
    <table border="0" cellpadding="0" cellspacing="0" class="AddBorder">
      <xsl:apply-templates select="ImportMaxPain"></xsl:apply-templates>
    </table>
  </xsl:template>

  <xsl:template match="ImportMaxPain">
    <xsl:if test="position()&lt;10">

	  <xsl:variable name="varTotalOI" select="@TotalCallOI+@TotalPutOI"/>
	  
      <xsl:if test="position()=1">
        <tr>
          <th class="HeadBorder">Ticker</th>
          <th class="HeadBorder">Maturity</th>		  
          <th class="HeadBorder">Stock Price</th>
          <th class="HeadBorder">Max Pain</th>
          <th class="HeadBorder">Total OI</th>
          <th class="HeadBorder">High Call</th>
          <th class="HeadBorder">High Put</th>
        </tr>
      </xsl:if>

      <tr>
        <td class="CellBorder"> 
          <a href="https://maximum-pain.com/options/{Ticker}?m={@Maturity}">
            <xsl:value-of select="@Ticker"/>
          </a>
        </td>
        <td class="CellBorder">
          <xsl:value-of select="@Maturity"/>
        </td>
        <td class="CellBorder" align="right">
          <xsl:value-of select="format-number(@StockPrice, '$#,##0.00')"/>
        </td>
        <td class="CellBorder" align="right">
          <xsl:value-of select="format-number(@MaxPain, '$#,##0.00')"/>
        </td>
        <td class="CellBorder" align="right">
          <xsl:value-of select="format-number($varTotalOI, '#,##0')"/>
        </td>
        <td class="CellBorder" align="right">
          <xsl:value-of select="format-number(@HighCallStrike, '$#,##0.00')"/>
        </td>
        <td class="CellBorder" align="right">
          <xsl:value-of select="format-number(@HighPutStrike, '$#,##0.00')"/>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>  
  
 <xsl:template name="formatdate">
    <xsl:param name="dte" />
   
    <xsl:variable name="yy"><xsl:value-of select="substring($dte,3,2)" /></xsl:variable>
    <xsl:variable name="mm"><xsl:value-of select="substring($dte,6,2)" /></xsl:variable>
    <xsl:variable name="dd"><xsl:value-of select="substring($dte,9,2)" /></xsl:variable>

    <xsl:value-of select="concat($mm,'/',$dd,'/',$yy)" />
  </xsl:template>
</xsl:stylesheet>
