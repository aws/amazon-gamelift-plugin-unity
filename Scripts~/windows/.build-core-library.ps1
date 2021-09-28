$ROOT_DIR="."
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$CORE_LIBRARY_PATH="$RUNTIME_PATH\Core"
$CORE_LIBRARY_PLUGINS_PATH="$CORE_LIBRARY_PATH\Plugins"

echo "Building core library dependencies..."

if (-Not (Test-Path -Path $RUNTIME_PATH))
{
	echo "$RUNTIME_PATH is not found in the working directory. Make sure you are executing the script from the project root."
	exit 1
}

if ((Get-Command "dotnet" -ErrorAction SilentlyContinue) -eq $null) 
{ 
	Write-Host "Unable to find 'dotnet' executable in your PATH. See README on how to install the .NET 4.5 Developer Pack in order to build the Server SDK"
	exit 1
}

dotnet build "$CORE_LIBRARY_PATH\AmazonGameLiftPlugin.Core.csproj"

echo "Core library dependencies built and saved in $CORE_LIBRARY_PLUGINS_PATH"
