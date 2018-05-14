# This script uses Autorest to generate service's client library

# == Prerequisites ==

# Nodejs version >= 6.11.2 - https://nodejs.org/en/download/
# NPM version >= 3.10.10 - https://www.npmjs.com/get-npm
# Autorest version >= 1.2.2 - https://www.npmjs.com/package/autorest

# Run this file if you use PowerShell directly

$url = "http://localhost:5000/swagger/v1/swagger.json"
$json = "$PSScriptRoot\hft-swagger.json"
$outdir = "$PSScriptRoot\..\lib"

Write-Output "Downloading swagger json"
Invoke-WebRequest -Uri $url -OutFile $json

autorest --typescript --output-folder=$outdir --input-file=$json --license-header=MICROSOFT_MIT_NO_VERSION --package-name='lykke-hft-client' --package-version=1.0.1