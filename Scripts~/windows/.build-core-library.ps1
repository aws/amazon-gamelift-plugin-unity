$ROOT_DIR="."
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$CORE_LIBRARY_PATH="$RUNTIME_PATH\Core"
$CORE_LIBRARY_PLUGINS_PATH="$CORE_LIBRARY_PATH\Plugins"

echo "Building core library dependencies..."

& "$PSScriptRoot\.verify-working-directory.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ((Get-Command "dotnet" -ErrorAction SilentlyContinue) -eq $null) 
{ 
	Write-Host "Unable to find 'dotnet' executable in your PATH. See README on how to install .NET in order to build the plugin" -ForegroundColor Red
	exit 1
}

dotnet build "$CORE_LIBRARY_PATH\AmazonGameLiftPlugin.Core.csproj"

Write-Host "Core library dependencies built and saved in $CORE_LIBRARY_PLUGINS_PATH" -ForegroundColor Yellow

exit 0
