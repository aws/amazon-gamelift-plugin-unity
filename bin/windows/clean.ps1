$ROOT_DIR="."
$CORE_LIB_PACKAGE_PATH=Join-Path $ROOT_DIR "AmazonGameLiftPlugin.Core\packages"
$UNITY_PROJECT_PATH=Join-Path $ROOT_DIR 'GameLift-Unity'
$RUNTIME_PLUGINS_PATH=Join-Path $UNITY_PROJECT_PATH "Assets\com.amazonaws.gamelift\Runtime\Plugins"
$EXAMPLE_PATH=Join-Path $ROOT_DIR "GameLift-Unity\Assets\com.amazonaws.gamelift\Examples~"
$BUILD_PATH=Join-Path $ROOT_DIR "build"

if (-Not (Test-Path -Path $UNITY_PROJECT_PATH))
{
	echo "'GameLift-Unity' directory is not found in the working directory. Make sure you are executing the script from the project root."
	Read-Host -Prompt "Press ENTER to continue"
	exit 1
}

if (Test-Path -Path $CORE_LIB_PACKAGE_PATH)
{
	echo "Removing all resolved NuGet dependencies for the Core Library"
	rm -Recurse -Force $CORE_LIB_PACKAGE_PATH
}
else
{
	echo "$CORE_LIB_PACKAGE_PATH not found. Continuing..."
}

if (Test-Path -Path $RUNTIME_PLUGINS_PATH)
{
	echo "Removing all DLLs from the Core Library and GameLift Server SDK"
	Get-ChildItem $RUNTIME_PLUGINS_PATH -Include *.dll -Recurse | Remove-Item
}
else
{
	echo "$RUNTIME_PLUGINS_PATH not found. Continuing..."
}

if (Test-Path -Path $EXAMPLE_PATH)
{
	echo "Removing example game and custom scenario template"
	rm -Recurse -Force $EXAMPLE_PATH
}
else
{
	echo "$EXAMPLE_PATH not found. Continuing..."
}

if (Test-Path -Path $BUILD_PATH)
{
	echo "Removing build directory and tarball file(s)"
	rm -Recurse -Force $BUILD_PATH
}
else
{
	echo "$BUILD_PATH not found. Continuing..."
}

echo "Clean up completed!"
Read-Host -Prompt "Press ENTER to continue"