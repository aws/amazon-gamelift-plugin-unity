$ROOT_DIR="."
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$CORE_LIBRARY_PLUGINS_PATH="$RUNTIME_PATH\Core\Plugins"
$SERVER_SDK_PLUGINS_PATH="$RUNTIME_PATH\Plugins"
$SAMPLE_GAME_PACKAGE_PATH="Samples~\SampleGame.unitypackage"

if (-Not (Test-Path -Path $RUNTIME_PATH))
{
	echo "$RUNTIME_PATH directory is not found in the working directory. Make sure you are executing the script from the project root."
	Read-Host -Prompt "Press ENTER to continue"
	exit 1
}

if (Test-Path -Path $CORE_LIBRARY_PLUGINS_PATH)
{
	echo "Removing all Core Library DLLs"
	Get-ChildItem $CORE_LIBRARY_PLUGINS_PATH -Include *.dll -Recurse | Remove-Item
}
else
{
	echo "$CORE_LIBRARY_PLUGINS_PATH not found. Continuing..."
}

if (Test-Path -Path $SERVER_SDK_PLUGINS_PATH)
{
	echo "Removing all GameLift Server SDK DLLs"
	Get-ChildItem $SERVER_SDK_PLUGINS_PATH -Include *.dll -Recurse | Remove-Item
}
else
{
	echo "$SERVER_SDK_PLUGINS_PATH not found. Continuing..."
}

if (Test-Path -Path $SAMPLE_GAME_PACKAGE_PATH)
{
	echo "Removing packaged sample game"
	rm -Recurse -Force $SAMPLE_GAME_PACKAGE_PATH
}
else
{
	echo "$SAMPLE_GAME_PACKAGE_PATH not found. Continuing..."
}

echo "Builds clean up completed!"
