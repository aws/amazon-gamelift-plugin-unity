$ROOT_DIR="."
$BUILD_ARTIFACT_PATH_PATTERN="$ROOT_DIR\com.amazonaws.gamelift-*.tgz"

& "$PSScriptRoot\.verify-working-directory.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

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

exit 0
