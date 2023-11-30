param(
	[Parameter(Mandatory)]
	[Alias("ServerSdk", "SdkVersion", "Sdk")]
	[ValidatePattern("\d+\.\d+\.\d+")]
	[string] $ServerSdkVersion
)

$ROOT_DIR='.'
$SERVER_SDK_FILENAME="GameLift-CSharp-ServerSDK-UnityPlugin-$ServerSdkVersion.zip";
$DESTINATION_PATH="$ROOT_DIR\$SERVER_SDK_FILENAME"
$TEMP_PATH="C:\Temp"
$TEMP_ZIP_PATH = "$TEMP_PATH\GameLiftServerSDK.zip"
$TEMP_EXTRACTED_PATH = "$TEMP_PATH\GameLiftServerSDK"
$S3_SERVER_SDK_BUCKET="https://gamelift-server-sdk-release.s3.us-west-2.amazonaws.com/unity"
$S3_SERVER_SDK_DOWNLOAD_LINK="$S3_SERVER_SDK_BUCKET/$SERVER_SDK_FILENAME"

& "$PSScriptRoot\.verify-working-directory.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (Test-Path -Path $DESTINATION_PATH)
{
	Write-Host "Amazon GameLift Server SDK zip has already been prepared, skipping download and extraction." -ForegroundColor DarkYellow
	exit 0
}

Write-Host "Preparing Amazon GameLift Server SDK for release..."

if (-Not (Test-Path -Path $TEMP_ZIP_PATH) )
{
	Write-Host "Downloading Amazon GameLift Server SDK $ServerSdkVersion..."
	# Download link should be public and require no credentials
	iwr $S3_SERVER_SDK_DOWNLOAD_LINK -OutFile $TEMP_ZIP_PATH
}
else
{
	Write-Host "Amazon GameLift Server SDK is already downloaded and stored at $TEMP_ZIP_PATH, skipping download."
}

Write-Host "Extracting and removing license file from Server SDK zip..."

Expand-Archive -LiteralPath $TEMP_ZIP_PATH -DestinationPath $TEMP_EXTRACTED_PATH -ErrorAction Stop
# See internal runbook for details
Remove-Item -LiteralPath "$TEMP_EXTRACTED_PATH\LICENSE.txt" -Force -ErrorAction Stop

Write-Host "Re-packaging Server SDK zip into project directory..."

Compress-Archive -Force -Path "$TEMP_EXTRACTED_PATH\*" -DestinationPath $DESTINATION_PATH -ErrorAction Stop

Write-Host "Server SDK has been re-packaged without license file at '$DESTINATION_PATH'" -ForegroundColor Yellow

exit 0