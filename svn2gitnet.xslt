<xsl:transform version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">

  <xsl:output method="xml" version="1.0" encoding="utf-8" indent="yes"/>

  <xsl:template match="wix:Product/@Version">
    <xsl:attribute name="Version">1.0.0.1</xsl:attribute>
  </xsl:template>

  <xsl:template match="wix:Product/@Manufacturer">
    <xsl:attribute name="Manufacturer">Jingyu Ma</xsl:attribute>
  </xsl:template>

  <xsl:template match="wix:Product/@Name">
    <xsl:attribute name="Name">Svn2Git.NET</xsl:attribute>
  </xsl:template>

  <xsl:template match="wix:Feature/@Title">
    <xsl:attribute name="Title">Svn2Git.NET.Main</xsl:attribute>
  </xsl:template>

  <xsl:template match="wix:Product">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
      <UIRef Id="WixUI_Minimal" xmlns="http://schemas.microsoft.com/wix/2006/wi" />
      <WixVariable Id="WixUILicenseRtf" Value="license.rtf" xmlns="http://schemas.microsoft.com/wix/2006/wi" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:Product/wix:Directory/wix:Directory/@Id">
    <xsl:attribute name="Id">APPLICATIONROOTDIRECTORY</xsl:attribute>
  </xsl:template>

  <xsl:template match="wix:Product/wix:Directory/wix:Directory/@Name">
    <xsl:attribute name="Name">Svn2Git.NET</xsl:attribute>
  </xsl:template>

  <xsl:template match="wix:Product/wix:Directory/wix:Directory">
    <Directory Id="ProgramFilesFolder" xmlns="http://schemas.microsoft.com/wix/2006/wi">
      <Component Id ="setEnviroment">
        <xsl:attribute name="Guid">{9AE672fD-224C-44D4-A38C-26012FF0B478}</xsl:attribute>
        <CreateFolder />
        <Environment
        Id="Environment"
        Name="PATH"
        Part="last"
        System="yes"
        Action="set"
        Value="[APPLICATIONROOTDIRECTORY]" />
      </Component>
      <xsl:copy>
        <xsl:apply-templates select="@*|node()" />
      </xsl:copy>
    </Directory>
  </xsl:template>

  <xsl:template match="wix:Fragment/wix:ComponentGroup">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
      <ComponentRef Id="setEnviroment" xmlns="http://schemas.microsoft.com/wix/2006/wi" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="node() | @*">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:transform>
