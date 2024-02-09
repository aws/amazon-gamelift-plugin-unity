$SAMPLES_PATH=".\Samples~"
$SAMPLE_GAME_PATH="$SAMPLES_PATH\SampleGame"
$SAMPLE_GAME_OUTPUT_PATH="$SAMPLES_PATH\SampleGame.unitypackage"
$SAMPLE_GAME_BUILD_LOG_PATH="$SAMPLES_PATH\SampleGameBuildLog.txt"
$SAMPLE_GAME_EXPORT_TIMEOUT=30
$EXPORT_START_TIME = Get-Date

echo "Exporting Sample Game..."

if (Test-Path -Path $SAMPLE_GAME_OUTPUT_PATH)
{
	del $SAMPLE_GAME_OUTPUT_PATH
}

if ((Get-Command "Unity.exe" -ErrorAction SilentlyContinue) -eq $null) {
	echo "Cannot run Unity.exe. Please add the Unity editor folder (e.g. 'C:\Program Files\Unity\Hub\Editor\<version>\Editor\') to the Windows PATH environment variable."
	echo "Export failed"
	exit 1
}

if (-Not (Test-Path -Path $SAMPLE_GAME_PATH))
{
	echo "$SAMPLE_GAME_PATH directory is not found in the working directory. Make sure you are executing the script from the project root."
	exit 1
}

Unity.exe -batchmode -quit -projectPath $SAMPLE_GAME_PATH -logFile $SAMPLE_GAME_BUILD_LOG_PATH -executeMethod UnityPackageExporter.Export

echo "Unity is exporting the sample game... The final build artifact will be saved in $SAMPLE_GAME_OUTPUT_PATH. Check log in $SAMPLE_GAME_BUILD_LOG_PATH to troubleshoot any issues."

echo "Waiting for export to complete..."
while ((-not (Test-Path $SAMPLE_GAME_OUTPUT_PATH)) -and $EXPORT_START_TIME.AddSeconds($SAMPLE_GAME_EXPORT_TIMEOUT) -gt (Get-Date)) {
	Start-Sleep -m 1000
}

if (Test-Path -Path $SAMPLE_GAME_OUTPUT_PATH)
{
	echo "Sample Game export completed! The exported artifact is saved in $SAMPLE_GAME_OUTPUT_PATH"
}
else
{
	echo "Sample Game export failed! Check logs in $SAMPLE_GAME_BUILD_LOG_PATH to troubleshoot any issues."
	exit 1
}
