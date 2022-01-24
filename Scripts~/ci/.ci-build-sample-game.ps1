$ROOT_DIR = "."
$SAMPLES_PATH="$ROOT_DIR\Samples~"
$SAMPLE_GAME_PATH="$SAMPLES_PATH\SampleGame"
$SAMPLE_GAME_OUTPUT_PATH="$SAMPLES_PATH\SampleGame.unitypackage"
$SAMPLE_GAME_BUILD_LOG_PATH="$ROOT_DIR\build\SampleGameBuildLog.txt"

echo "Building and exporting Sample Game..."

Start-Process -FilePath "C:\Unity\Editor\Unity.exe" -Wait -NoNewWindow -ArgumentList "-batchmode -nographics -quit -projectPath $SAMPLE_GAME_PATH -logFile $SAMPLE_GAME_BUILD_LOG_PATH -executeMethod UnityPackageExporter.Export"

echo .

if (Test-Path -Path $SAMPLE_GAME_OUTPUT_PATH)
{
	echo "Sample Game export completed! The exported artifact is saved in $SAMPLE_GAME_OUTPUT_PATH"
}
else
{
	throw "Sample Game export failed! Check logs in $SAMPLE_GAME_BUILD_LOG_PATH to troubleshoot any issues."
}
