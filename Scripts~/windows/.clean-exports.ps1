$ROOT_DIR="."
$STAGING_DIRECTORY="$ROOT_DIR\staging-release"
$BUILD_ARTIFACT_PATH_PATTERN="$ROOT_DIR\com.amazonaws.gamelift-*.tgz"
$SERVER_SDK_ARTIFACT_PATH_PATTERN="$ROOT_DIR\GameLift-CSharp-ServerSDK-*.zip"
$RELEASE_ARTIFACT_PATH_PATTERN="$ROOT_DIR\amazon-gamelift-plugin-unity-*.zip"

& "$PSScriptRoot\.verify-working-directory.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (Test-Path -Path $BUILD_ARTIFACT_PATH_PATTERN)
{
	Write-Host "Removing plugin tarball file(s)"
	Remove-Item -Recurse -Force $BUILD_ARTIFACT_PATH_PATTERN
}
else
{
	Write-Host "$BUILD_ARTIFACT_PATH_PATTERN not found. Continuing..."
}

if (Test-Path -Path $SERVER_SDK_ARTIFACT_PATH_PATTERN)
{
	Write-Host "Removing re-packaged server sdk zip file(s)"
	Remove-Item -Recurse -Force $SERVER_SDK_ARTIFACT_PATH_PATTERN
}
else
{
	Write-Host "$SERVER_SDK_ARTIFACT_PATH_PATTERN not found. Continuing..."
}

if (Test-Path -Path $RELEASE_ARTIFACT_PATH_PATTERN)
{
	Write-Host "Removing release zip file(s)"
	Remove-Item -Recurse -Force $RELEASE_ARTIFACT_PATH_PATTERN
}
else
{
	Write-Host "$RELEASE_ARTIFACT_PATH_PATTERN not found. Continuing..."
}

if (Test-Path -Path $STAGING_DIRECTORY)
{
	Write-Host "Removing release staging directory"
	Remove-Item -Recurse -Force $STAGING_DIRECTORY
}
else
{
	Write-Host "$STAGING_DIRECTORY not found. Continuing..."
}

Write-Host "Exports clean up completed!" -ForegroundColor Yellow

exit 0
