$TEMP_PATH="C:\Temp"
$TEMP_ZIP_PATH = "$TEMP_PATH\GameLiftServerSDK.zip"
$TEMP_EXTRACTED_PATH = "$TEMP_PATH\GameLiftServerSDK"

if (Test-Path -Path $TEMP_ZIP_PATH)
{
	echo "Removing temp zip download path(s)"
	rm -Recurse -Force $TEMP_ZIP_PATH
}
else
{
	echo "$TEMP_ZIP_PATH not found. Continuing..."
}

if (Test-Path -Path $TEMP_EXTRACTED_PATH)
{
	echo "Removing temp zip extraction path(s)"
	rm -Recurse -Force $TEMP_EXTRACTED_PATH
}
else
{
	echo "$TEMP_EXTRACTED_PATH not found. Continuing..."
}

Write-Host "Download file clean up completed!" -ForegroundColor Yellow

exit 0