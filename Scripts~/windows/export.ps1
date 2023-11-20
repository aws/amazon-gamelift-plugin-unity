$ROOT_DIR='.'
$DESTINATION_PATH="$ROOT_DIR\.build"

& "$PSScriptRoot\.verify-working-directory.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null) 
{ 
	Write-Host "Unable to find 'npm' executable in your PATH. Please install nodejs first: https://nodejs.org/en/download/"
	exit 1
}

npm pack

exit 0
