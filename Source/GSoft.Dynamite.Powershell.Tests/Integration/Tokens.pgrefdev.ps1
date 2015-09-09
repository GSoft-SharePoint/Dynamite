﻿# ******************************************
# PowerShell Files Tokens
# ******************************************

# --------------- Web.ps1  -----------------

# ******************************************
# Export-DSPWeb Tokens 
# ******************************************
$DSP_WebApplicationUrl = "http://pgrefdev"
$DSP_XmlSchema = ".\TestSchema.xsd"
$DSP_OutputFileName = ".\Output.xml"
$DSP_TempSiteCollection = "sites/exporttest"
$DSP_CurrentAccount = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
$DSP_VariationsConfigFile = "./TestVariationsSettings.xml"
