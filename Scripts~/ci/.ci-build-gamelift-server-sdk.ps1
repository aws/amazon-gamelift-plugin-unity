$ROOT_DIR = "."
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$TEMP_PATH="$ROOT_DIR\Temp~"
$NUGET_EXE_PATH="$TEMP_PATH\nuget.exe"
$TEMP_ZIP_PATH = "$TEMP_PATH\GameLiftServerSDK.zip"
$TEMP_EXTRACTED_PATH = "$TEMP_PATH\GameLiftServerSDK"
$SDK_PATH="$TEMP_EXTRACTED_PATH\GameLift-SDK-Release-4.0.2\GameLift-CSharp-ServerSDK-4.0.2"
$SDK_CSPROJ_PATH="$SDK_PATH\Net45\GameLiftServerSDKNet45.csproj"
$SDK_PACKAGES_CONFIG_PATH="$SDK_PATH\Net45\packages.config"
$RUNTIME_PLUGINS_PATH="$RUNTIME_PATH\Plugins"

echo "Building GameLift Server SDK..."

if ((Get-Command "dotnet" -ErrorAction SilentlyContinue) -eq $null) {
	throw "dotnet is not in PATH"
}

if (!(Test-Path -Path $TEMP_PATH -PathType Container)) {
	mkdir -Path $TEMP_PATH
}

echo .
echo "Downloading GameLift Managed Servers SDK..."
iwr https://gamelift-release.s3-us-west-2.amazonaws.com/GameLift_06_03_2021.zip -OutFile $TEMP_ZIP_PATH -UseBasicParsing
Expand-Archive -Force -LiteralPath $TEMP_ZIP_PATH -DestinationPath $TEMP_EXTRACTED_PATH

echo "Updating Newtonsoft.Json version used to 12.0.3 for compatibility with Unity 2020.3"

(Get-Content $SDK_CSPROJ_PATH).replace('Newtonsoft.Json, Version=9.0.0.0', 'Newtonsoft.Json, Version=12.0.0.0') | Set-Content $SDK_CSPROJ_PATH
(Get-Content $SDK_CSPROJ_PATH).replace('Newtonsoft.Json.9.0.1', 'Newtonsoft.Json.12.0.3') | Set-Content $SDK_CSPROJ_PATH
(Get-Content $SDK_PACKAGES_CONFIG_PATH).replace('id="Newtonsoft.Json" version="9.0.1"', 'id="Newtonsoft.Json" version="12.0.3"') | Set-Content $SDK_PACKAGES_CONFIG_PATH

if (-Not (Test-Path -Path $NUGET_EXE_PATH) )
{
	echo "Downloading NuGet started"
	iwr https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $NUGET_EXE_PATH -UseBasicParsing
}
else
{
	echo "nuget.exe is already downloaded and stored at NUGET_EXE_PATH, skip downloading."
}

echo "Building GameLift Server SDK..."
Invoke-Expression '& $NUGET_EXE_PATH restore "$SDK_PATH\GameLiftServerSDKNet45.sln" -PackagesDirectory "$SDK_PATH\packages"'

$process = Start-Process dotnet.exe -ArgumentList "build $SDK_CSPROJ_PATH --configuration Release --output $RUNTIME_PLUGINS_PATH" -PassThru -NoNewWindow
$process | Wait-Process -Timeout 30 -ErrorAction SilentlyContinue

if ($process.ExitCode -ne 0) {
	throw "GameLiftServerSDK build failed."
}

# Newtonsoft.json is deleted in favor of the Newtonsoft.json dependency in Unity. See "com.unity.nuget.newtonsoft-json: x.x.x" dependency in package.json.
del "$RUNTIME_PLUGINS_PATH\Newtonsoft.Json.dll"
del "$RUNTIME_PLUGINS_PATH\Newtonsoft.Json.xml"

echo "GameLift Server SDK build completed!"
