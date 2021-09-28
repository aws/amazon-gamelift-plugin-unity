$TEMP_PATH="C:\Temp"
$NUGET_EXE_PATH="$TEMP_PATH\nuget.exe"
$TEMP_ZIP_PATH = "$TEMP_PATH\GameLiftServerSDK.zip"
$TEMP_EXTRACTED_PATH = "$TEMP_PATH\GameLiftServerSDK"

if (Test-Path -Path $NUGET_EXE_PATH)
{
	echo "Removing $NUGET_EXE_PATH"
	rm -Recurse -Force $NUGET_EXE_PATH
}
else
{
	echo "$NUGET_EXE_PATH not found. Continuing..."
}

if (Test-Path -Path $TEMP_ZIP_PATH)
{
	echo "Removing TEMP_ZIP_PATH"
	rm -Recurse -Force $TEMP_ZIP_PATH
}
else
{
	echo "$TEMP_ZIP_PATH not found. Continuing..."
}

if (Test-Path -Path $TEMP_EXTRACTED_PATH)
{
	echo "Removing TEMP_EXTRACTED_PATH"
	rm -Recurse -Force $TEMP_EXTRACTED_PATH
}
else
{
	echo "$TEMP_EXTRACTED_PATH not found. Continuing..."
}

echo "Download file clean up completed!"
