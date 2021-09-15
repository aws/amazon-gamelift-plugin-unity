$CORE_PATH="AmazonGameLiftPlugin.Core"
$RUNTIME_PLUGINS_PATH="GameLift-Unity\Assets\com.amazonaws.gamelift\Runtime\Plugins"
$EXAMPLE_PATH="GameLift-Unity\Assets\com.amazonaws.gamelift\Examples~"
$BUILD_PATH="build"

echo .
echo "Removing all resolved NuGet dependencies for the Core Library"
rm -Recurse -Force $CORE_PATH\packages

echo .
echo "Removing all DLLs from the Core Library and GameLift Server SDK"
Get-ChildItem $RUNTIME_PLUGINS_PATH -Include *.dll -Recurse | Remove-Item

echo .
echo "Removing example game and custom scenario template"
rm -Recurse -Force $EXAMPLE_PATH

echo .
echo "Removing build directory and tarball file(s)"
rm -Recurse -Force $BUILD_PATH

echo .
echo "Clean up completed"
Read-Host -Prompt "Press ENTER to continue"