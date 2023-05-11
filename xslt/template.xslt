<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" omit-xml-declaration="yes" />

  <xsl:param name="ID" />
  <xsl:param name="Group" />
  <xsl:param name="YearCreate" />

  <xsl:template match="/">
    <xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>

    <html>
    <head>
        <title>Документація - <xsl:value-of select="root/group[@name = $Group]/page[@id = $ID]/name"/></title>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link href="/doc/style.css" rel="stylesheet" />
    </head>
    <body>

    <div class="container-fluid p-5 bg-light text-center">
    <h1>Документація</h1>
    <ul class="nav nav-pills justify-content-center">

        <xsl:for-each select="root/group">
            <li class="nav-item">
                <a href="{concat('/doc/', @name, '/')}">
                    <xsl:choose>
                        <xsl:when test="@name = $Group">
                            <xsl:attribute name="class">
                                <xsl:text>nav-link active</xsl:text>
                            </xsl:attribute>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:attribute name="class">
                                <xsl:text>nav-link</xsl:text>
                            </xsl:attribute>
                        </xsl:otherwise>
                    </xsl:choose>
                    <xsl:value-of select="@alias"/>
                </a>
            </li>
        </xsl:for-each>

    </ul>
    </div>
    
    <div class="container mt-3">
    <div class="row">
        <div class="col-sm-3">

            <xsl:for-each select="root/group[@name = $Group]/page">
                <p>
                    <xsl:choose>
                        <xsl:when test="@id = $ID">
                            <xsl:value-of select="name"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <a href="{concat('/doc/', $Group, '/', @id, '.html')}"> 
                                <xsl:value-of select="name"/>
                            </a>
                        </xsl:otherwise>
                    </xsl:choose> 
                </p>
            </xsl:for-each>
            
        </div>
        <div class="col-sm-9">
           <h2>
                <xsl:value-of select="root/group[@name = $Group]/page[@id = $ID]/name"/>
           </h2>
           <hr />

           <xsl:value-of disable-output-escaping="yes" select="root/group[@name = $Group]/page[@id = $ID]/value"/>

        </div>
    </div>
    </div>

    <div class="jumbotron text-center" style="margin-bottom:0;margin-top:20px;">
        <hr />
        <p><a href="https://accounting.org.ua">accounting.org.ua</a> © <xsl:value-of select="$YearCreate"/> рік</p>
    </div>

    </body>
    </html>

  </xsl:template>

</xsl:stylesheet>