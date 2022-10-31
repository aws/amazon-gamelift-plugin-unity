$ROOT_DIR = "."
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$CORE_LIBRARY_PATH="$RUNTIME_PATH\Core"
$CORE_LIBRARY_PLUGINS_PATH="$CORE_LIBRARY_PATH\Plugins"

echo "Building core library dependencies..."

if ((Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) -eq $null) {
	throw "dotnet is not in PATH"
}

$process = Start-Process dotnet.exe -ArgumentList "build $CORE_LIBRARY_PATH\AmazonGameLiftPlugin.Core.csproj --configuration Release" -PassThru -NoNewWindow
$process | Wait-Process -Timeout 60 -ErrorAction Stop

if ($process.ExitCode -ne 0) {
	throw "Core library build failed."
}

echo "Core library dependencies built and saved in $CORE_LIBRARY_PLUGINS_PATH"
