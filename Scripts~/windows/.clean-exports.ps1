$ROOT_DIR="."
$STAGING_DIRECTORY="$ROOT_DIR\staging-release"
$BUILD_ARTIFACT_PATH_PATTERN="$ROOT_DIR\com.amazonaws.gamelift-*.tgz"
$SERVER_SDK_ARTIFACT_PATH_PATTERN="$ROOT_DIR\GameLift-CSharp-ServerSDK-*.zip"
$RELEASE_ARTIFACT_PATH_PATTERN="$ROOT_DIR\amazon-gamelift-plugin-unity-*.zip"

& "$PSScriptRoot\.verify-working-directory.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (Test-Path -Path $BUILD_ARTIFACT_PATH_PATTERN)
{
	echo "Removing plugin tarball file(s)"
	rm -Recurse -Force $BUILD_ARTIFACT_PATH_PATTERN
}
else
{
	echo "$BUILD_ARTIFACT_PATH_PATTERN not found. Continuing..."
}

if (Test-Path -Path $SERVER_SDK_ARTIFACT_PATH_PATTERN)
{
	echo "Removing re-packaged server sdk zip file(s)"
	rm -Recurse -Force $SERVER_SDK_ARTIFACT_PATH_PATTERN
}
else
{
	echo "$SERVER_SDK_ARTIFACT_PATH_PATTERN not found. Continuing..."
}

if (Test-Path -Path $RELEASE_ARTIFACT_PATH_PATTERN)
{
	echo "Removing release zip file(s)"
	rm -Recurse -Force $RELEASE_ARTIFACT_PATH_PATTERN
}
else
{
	echo "$RELEASE_ARTIFACT_PATH_PATTERN not found. Continuing..."
}

if (Test-Path -Path $STAGING_DIRECTORY)
{
	echo "Removing release staging directory"
	rm -Recurse -Force $STAGING_DIRECTORY
}
else
{
	echo "$STAGING_DIRECTORY not found. Continuing..."
}

echo "Exports clean up completed!"

exit 0
