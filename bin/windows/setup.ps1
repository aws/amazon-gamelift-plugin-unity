# 'dotnet' command is included with the .NET SDK https://docs.microsoft.com/en-us/dotnet/core/sdk
# You need to install the .NET 4.5 Developer Pack to build the Server SDK.
$RUNTIME_PLUGINS_PATH="GameLift-Unity\Assets\com.amazonaws.gamelift\Runtime\Plugins"
$TESTS_PLUGINS_PATH="GameLift-Unity\Assets\com.amazonaws.gamelift\Tests\Plugins"

echo .
echo "Downloading NuGet started"
iwr https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile "nuget.exe"
echo "Adding nuget.org source"
.\nuget sources Add -Name "nuget.org" -Source https://api.nuget.org/v3/index.json
.\nuget sources Enable -Name "nuget.org"

echo .
echo "Restoring Moq started"
mkdir -p "tmp_packages"
if ( Test-Path -Path "tmp_packages" -PathType Container ) { rm -Recurse -Force "tmp_packages" }
.\nuget.exe install Moq -Version "4.16.1" -Framework "net45" -OutputDirectory "tmp_packages"
.\nuget.exe install Castle.Core -Version "4.4.0" -Framework "net45" -OutputDirectory "tmp_packages"
gci "tmp_packages\Castle.Core.4.4.0\lib\net45" | move -Force -Destination $TESTS_PLUGINS_PATH
gci "tmp_packages\Moq.4.16.1\lib\net45" | move -Force -Destination $TESTS_PLUGINS_PATH
gci "tmp_packages\System.Runtime.CompilerServices.Unsafe.4.5.3\lib\net461" | move -Force -Destination $TESTS_PLUGINS_PATH
gci "tmp_packages\System.Threading.Tasks.Extensions.4.5.4\lib\net461" | move -Force -Destination $TESTS_PLUGINS_PATH
rm -Recurse -Force "tmp_packages"

echo .
echo "AmazonGameLiftPlugin.Core build started"
$CORE_PATH="AmazonGameLiftPlugin.Core"
.\nuget.exe restore "$CORE_PATH\AmazonGameLiftPlugin.Core.sln" -PackagesDirectory "$CORE_PATH\packages"
dotnet build "$CORE_PATH\AmazonGameLiftPlugin.Core\AmazonGameLiftPlugin.Core.csproj" --configuration Release --output $RUNTIME_PLUGINS_PATH
del "$RUNTIME_PLUGINS_PATH\Newtonsoft.Json.dll"
del "$RUNTIME_PLUGINS_PATH\Newtonsoft.Json.xml"

echo .
echo "Downloading GameLift Managed Servers SDK..."
$SDK_ZIP_PATH = "C:\Temp\GameLiftServerSDK.zip"
$SDK_TMP_PATH = "C:\Temp\GameLiftServerSDK"
iwr https://gamelift-release.s3-us-west-2.amazonaws.com/GameLift_06_03_2021.zip -OutFile $SDK_ZIP_PATH
Expand-Archive -Force -LiteralPath $SDK_ZIP_PATH -DestinationPath $SDK_TMP_PATH
del $SDK_ZIP_PATH
$SDK_PATH="$SDK_TMP_PATH\GameLift-SDK-Release-4.0.2\GameLift-CSharp-ServerSDK-4.0.2"

echo . 
echo "Updating Newtonsoft.Json version used to 12.0.3 for compatibility with Unity 2020.3"
$SDK_CSPROJ_PATH="$SDK_PATH\Net45\GameLiftServerSDKNet45.csproj"
$SDK_PKG_CONFIG_PATH="$SDK_PATH\Net45\packages.config"
(Get-Content $SDK_CSPROJ_PATH).replace('Newtonsoft.Json, Version=9.0.0.0', 'Newtonsoft.Json, Version=12.0.0.0') | Set-Content $SDK_CSPROJ_PATH
(Get-Content $SDK_CSPROJ_PATH).replace('Newtonsoft.Json.9.0.1', 'Newtonsoft.Json.12.0.3') | Set-Content $SDK_CSPROJ_PATH
(Get-Content $SDK_PKG_CONFIG_PATH).replace('id="Newtonsoft.Json" version="9.0.1"', 'id="Newtonsoft.Json" version="12.0.3"') | Set-Content $SDK_PKG_CONFIG_PATH

echo .
echo "GameLiftServerSDK build started"
.\nuget.exe restore "$SDK_PATH\GameLiftServerSDKNet45.sln" -PackagesDirectory "$SDK_PATH\packages"
dotnet build "$SDK_PATH\Net45\GameLiftServerSDKNet45.csproj" --configuration Release --output "$RUNTIME_PLUGINS_PATH\GameLiftServerSDK"
del "$RUNTIME_PLUGINS_PATH\GameLiftServerSDK\Newtonsoft.Json.dll"
del "$RUNTIME_PLUGINS_PATH\GameLiftServerSDK\Newtonsoft.Json.xml"
rm -Recurse -Force $SDK_TMP_PATH
del "nuget.exe"

if ((Get-Command "Unity.exe" -ErrorAction SilentlyContinue) -eq $null) {
	echo "You need to add target Unity editor folder (e.g. 'C:\Program Files\Unity\Hub\Editor\<version>\Editor\') to the Windows PATH environment variable."
	echo "Setup failed"
	Read-Host -Prompt "Press ENTER to continue"
	exit
}

Unity.exe -batchmode -quit -projectPath "GameLift-SampleGame" -executeMethod UnityPackageExporter.Export
Unity.exe -batchmode -quit -projectPath "GameLift-Unity" -executeMethod AWS.GameLift.Editor.CustomScenarioPackageExporter.Export
echo "Setup complete"
Read-Host -Prompt "Press ENTER to continue"