<?xml version="1.0" encoding="utf-8"?>
 
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">
    <xsl:apply-templates select="HealthChecks" />
  </xsl:template>

  <xsl:template match="HealthChecks">
    <xsl:variable name="padName">
      <xsl:call-template name="PadString">
        <xsl:with-param name="content">Name</xsl:with-param>
        <xsl:with-param name="size">30</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="padStatus">
      <xsl:call-template name="PadString">
        <xsl:with-param name="content">Status</xsl:with-param>
        <xsl:with-param name="size">10</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="padDescription">
      <xsl:call-template name="PadString">
        <xsl:with-param name="content">Description</xsl:with-param>
        <xsl:with-param name="size">100</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
HealthCheck
<xsl:value-of select="concat($padName, $padStatus, $padDescription)"/>
<xsl:apply-templates select="HealthCheck" />
  </xsl:template>

  <xsl:template match="HealthCheck">
    <xsl:variable name="padName">
      <xsl:call-template name="PadString">
        <xsl:with-param name="content">
          <xsl:value-of select="@Name"/>
        </xsl:with-param>
        <xsl:with-param name="size">30</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="padStatus">
      <xsl:call-template name="PadString">
        <xsl:with-param name="content">
          <xsl:choose>
            <xsl:when test="@HasError='True'">ERROR</xsl:when>
            <xsl:otherwise>&#32;</xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
        <xsl:with-param name="size">10</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="padDescription">
      <xsl:call-template name="PadString">
        <xsl:with-param name="content">
          <xsl:value-of select="@Description"/>
        </xsl:with-param>
        <xsl:with-param name="size">100</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

<xsl:value-of select="concat($padName, $padStatus, $padDescription)"/>
  </xsl:template>

  <xsl:template name="PadString">
    <xsl:param name="content"/>
    <xsl:param name="size"/>

    <xsl:variable name="spaces">                                                                                          </xsl:variable>
    <xsl:value-of select="substring(concat($content, $spaces), 1, $size)"/>
  </xsl:template>
</xsl:stylesheet> 