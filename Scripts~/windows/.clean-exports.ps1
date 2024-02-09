$ROOT_DIR="."
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$BUILD_ARTIFACT_PATH_PATTERN="$ROOT_DIR\com.amazonaws.gamelift-*.tgz"

if (-Not (Test-Path -Path $RUNTIME_PATH))
{
	echo "$RUNTIME_PATH directory is not found in the working directory. Make sure you are executing the script from the project root."
	Read-Host -Prompt "Press ENTER to continue"
	exit 1
}

if (Test-Path -Path $BUILD_ARTIFACT_PATH_PATTERN)
{
	echo "Removing build directory and tarball file(s)"
	rm -Recurse -Force $BUILD_ARTIFACT_PATH_PATTERN
}
else
{
	echo "$BUILD_ARTIFACT_PATH_PATTERN not found. Continuing..."
}

echo "Exports clean up completed!"
