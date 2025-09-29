<?xml version="1.0" encoding="utf-8"?>
 
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">      
    <xsl:apply-templates select="HealthChecks" />
  </xsl:template>
  
	<xsl:template name="style">
		<style type="text/css">
		  table {margin: 10px auto; padding: 0px; width: 80%;}
		  th {padding: 3px; border-bottom: 1px solid #ccc; border-top: 1px solid #ccc; font-weight: bold;}
		  td {padding: 3px; border-bottom: 1px solid #ccc;}
		  .Red {background-color: #ff9999;}
		  .White {background-color: #ffffff;}
		</style>
	</xsl:template>

  <xsl:template match="HealthChecks">
    <table border="1" cellpadding="2" cellspacing="0">
      <tr>
        <th>Name</th>
        <th>Status</th>
        <th>Description</th>
      </tr>
      <xsl:apply-templates select="HealthCheck" />
    </table>
  </xsl:template>

  <xsl:template match="HealthCheck">
    <xsl:variable name="varClass">
      <xsl:choose>
        <xsl:when test="@HasError='True'">Red</xsl:when>
        <xsl:otherwise>White</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <tr>
      <td class="{$varClass}">
        <xsl:value-of select="@Name"/>
      </td>
      <td class="{$varClass}">
        <xsl:choose>
          <xsl:when test="@HasError='True'">ERROR</xsl:when>
          <xsl:otherwise>&#32;</xsl:otherwise>
        </xsl:choose>
      </td>
      <td class="{$varClass}">
        <xsl:value-of select="@Description"/>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet> 