<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">
    <xsl:apply-templates select="Root" />
  </xsl:template>

  <xsl:template match="Root">
    <style type="text/css">
      body { margin:0; padding:0; background:#f4f6f8; font-family:"Helvetica Neue",Helvetica,Arial,sans-serif; font-size:14px; color:#222; }
      .wrapper { max-width:660px; margin:20px auto; background:#fff; border-radius:8px; overflow:hidden; border:1px solid #dde3ec; }
      .header { background:#1a1f2e; padding:20px 24px; color:#fff; font-size:20px; font-weight:bold; }
      .header a { color:#7eb3ff; text-decoration:none; }
      .body { padding:20px 24px; }
      h3 { margin:0 0 4px; font-size:15px; font-weight:600; color:#1a1f2e; display:inline; }
      .see-more { font-size:12px; color:#1a73e8; text-decoration:none; margin-left:8px; }
      table { width:100%; border-collapse:collapse; margin-bottom:20px; }
      th { background:#1a73e8; color:#fff; padding:7px 10px; font-weight:500; font-size:13px; text-align:left; }
      td { padding:6px 10px; font-size:13px; border-bottom:1px solid #eef0f3; }
      tr:last-child td { border-bottom:none; }
      a { color:#1a73e8; }
      .section { margin:0 0 20px 0; }
      .footer { padding:16px 24px; border-top:1px solid #dde3ec; font-size:11px; color:#888; }
      .HeadBorder { border-right:1px solid #ccc; }
      .CellBorder { border-right:1px solid #ccc; border-bottom:1px solid #ccc; }
    </style>

    <div class="wrapper">
      <div class="header">
        <a href="https://maximum-pain.com">maximum-pain.com</a> daily option screener
      </div>
      <div class="body">

        <xsl:if test="string-length(@ChartLink) != 0">
          <div class="section">
            <h3>Open Interest chart for <xsl:value-of select="@ChartTicker"/>.</h3>
            <br/>
            <a href="{@ChartLink}"><img width="100%" src="{@ChartImage}" /></a>
          </div>
        </xsl:if>

        <div class="section">
          <p>This is the <a href="https://maximum-pain.com">maximum-pain.com</a> <strong>daily stock option screener</strong>.</p>
          <p>You are receiving this email because you opted in to our mailing list. It demonstrates which options have the largest changes in open interest, volume and price from the previous day.</p>
          <p>The screener is run daily <strong>after the market close</strong>, and compares against the previous day. The screener checks the symbols from the SP500.</p>
        </div>

        <div class="section">
          <xsl:apply-templates select="ArrayOfMostActive" mode="first" />
        </div>

        <div class="section">
          <xsl:apply-templates select="ArrayOfMostActive" mode="second" />
        </div>

        <div class="section">
          <h3>Outside Open Interest Walls</h3>
          <a class="see-more" href="https://maximum-pain.com/outside-oi-walls">see more</a>
          <a class="see-more" href="https://maximum-pain.com/blog/archive/open-interest-walls/">what does this mean?</a>
          <xsl:apply-templates select="ArrayOfOutsideOIWalls" />
        </div>
		
      </div>
      <div class="footer">
        <p>Thank you,<br/>Dan</p>
        <p>Click here if you wish to <a href="{@UnsubscribeUrl}">unsubscribe</a> from the mailing list.</p>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="ArrayOfMostActive" mode="first">
    <h3>Largest change in Price</h3>
    <a class="see-more" href="https://maximum-pain.com/screener/ChangePrice">see more options</a>
    <table border="0" cellpadding="0" cellspacing="0">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='ChangePrice']" />
    </table>

    <h3>Highest Open Interest</h3>
    <a class="see-more" href="https://maximum-pain.com/screener/OpenInterest">see more options</a>
    <table border="0" cellpadding="0" cellspacing="0">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='OpenInterest']" />
    </table>
  </xsl:template>

  <xsl:template match="ArrayOfMostActive" mode="second">
    <h3>Largest change in Open Interest</h3>
    <a class="see-more" href="https://maximum-pain.com/screener/ChangeOpenInterest">see more options</a>
    <table border="0" cellpadding="0" cellspacing="0">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='ChangeOpenInterest']" />
    </table>

    <h3>Highest volume</h3>
    <a class="see-more" href="https://maximum-pain.com/screener/Volume">see more options</a>
    <table border="0" cellpadding="0" cellspacing="0">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='Volume']" />
    </table>

    <h3>Largest change in volume</h3>
    <a class="see-more" href="https://maximum-pain.com/screener/ChangeVolume">see more options</a>
    <table border="0" cellpadding="0" cellspacing="0">
      <xsl:apply-templates select="MostActive[NextMaturity='true' and QueryType='ChangeVolume']" />
    </table>
  </xsl:template>

  <xsl:template match="MostActive">
    <xsl:if test="position() &lt; 4">
      <xsl:if test="position() = 1">
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

      <xsl:variable name="varType">
        <xsl:choose>
          <xsl:when test="CallPut='C'">call</xsl:when>
          <xsl:when test="CallPut='P'">put</xsl:when>
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
        <td class="CellBorder"><xsl:value-of select="$varMaturity"/></td>
        <td class="CellBorder"><xsl:value-of select="$varCreatedOn"/></td>
        <td class="CellBorder"><xsl:value-of select="$varType"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="Strike"/></td>
        <xsl:choose>
          <xsl:when test="QueryType='ChangePrice'">
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(Price, '$###,##0.00')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(PrevPrice, '$###,##0.00')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(ChangePrice, '###,##0%')"/></td>
          </xsl:when>
          <xsl:when test="QueryType='OpenInterest' or QueryType='ChangeOpenInterest'">
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(OpenInterest, '###,###,###,###,##0')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(PrevOpenInterest, '###,###,###,###,##0')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(ChangeOpenInterest, '###,##0%')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(Price, '$###,##0.00')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(PrevPrice, '$###,##0.00')"/></td>
          </xsl:when>
          <xsl:when test="QueryType='Volume' or QueryType='ChangeVolume'">
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(Volume, '###,###,###,###,##0')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(PrevVolume, '###,###,###,###,##0')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(ChangeVolume, '###,##0%')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(Price, '$###,##0.00')"/></td>
            <td class="CellBorder" align="right"><xsl:value-of select="format-number(PrevPrice, '$###,##0.00')"/></td>
          </xsl:when>
        </xsl:choose>
      </tr>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ArrayOfOutsideOIWalls">
    <table border="0" cellpadding="0" cellspacing="0">
      <xsl:apply-templates select="OutsideOIWalls" />
    </table>
  </xsl:template>

  <xsl:template match="OutsideOIWalls">
    <xsl:if test="position() = 1">
      <tr>
        <th class="HeadBorder">Stock</th>
        <th class="HeadBorder">Maturity</th>
        <th class="HeadBorder">Total Open Interest</th>
        <th class="HeadBorder">High Put Strike</th>
        <th class="HeadBorder">High Call Strike</th>
        <th class="HeadBorder">Stock Price</th>
      </tr>
    </xsl:if>
    <xsl:if test="position() &lt; 4">
      <tr>
        <td class="CellBorder">
          <a href="https://maximum-pain.com/options/{Ticker}?m={Maturity}">
            <xsl:value-of select="Ticker"/>
          </a>
        </td>
        <td class="CellBorder"><xsl:value-of select="Maturity"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(SumOI, '#,##0')"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(PutStrike, '$#,##0.00')"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(CallStrike, '$#,##0.00')"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(StockPrice, '$#,##0.00')"/></td>
      </tr>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ArrayOfImportMaxPain">
    <table border="0" cellpadding="0" cellspacing="0">
      <xsl:apply-templates select="ImportMaxPain" />
    </table>
  </xsl:template>

  <xsl:template match="ImportMaxPain">
    <xsl:if test="position() &lt; 10">
      <xsl:variable name="varTotalOI" select="@TotalCallOI+@TotalPutOI"/>
      <xsl:if test="position() = 1">
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
        <td class="CellBorder"><xsl:value-of select="@Maturity"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(@StockPrice, '$#,##0.00')"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(@MaxPain, '$#,##0.00')"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number($varTotalOI, '#,##0')"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(@HighCallStrike, '$#,##0.00')"/></td>
        <td class="CellBorder" align="right"><xsl:value-of select="format-number(@HighPutStrike, '$#,##0.00')"/></td>
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
