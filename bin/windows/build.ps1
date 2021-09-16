$ROOT_DIR='.'
$UNITY_PROJECT_PATH=Join-Path $ROOT_DIR 'GameLift-Unity'

if (-Not (Test-Path -Path $UNITY_PROJECT_PATH))
{
	echo "'GameLift-Unity' directory is not found in the working directory. Make sure you are executing the script from the project root."
	Read-Host -Prompt "Press ENTER to continue"
	exit 1
}

$BUILD_PATH=Join-Path $ROOT_DIR 'build'
$LOG_PATH=Join-Path $BUILD_PATH 'build_log.txt'
$BUILD_ARTIFACT_PATH_PATTERN=Join-Path $BUILD_PATH 'com.amazonaws.gamelift-*.tar'
$BUILD_START_TIME = Get-Date

echo "Building Amazon GameLift Unity Plugin artifacts into a tarball file"
Unity.exe -batchmode -quit -projectPath $UNITY_PROJECT_PATH -logFile $LOG_PATH -executeMethod AWS.GameLift.Editor.PluginPacker.Pack

echo .
echo "Unity is building the plugin... The final build artifact will be saved in $BUILD_PATH. Check log in $LOG_PATH to troubleshoot any issues."

echo "Waiting for build to complete..."
while ((-not (Test-Path $BUILD_ARTIFACT_PATH_PATTERN)) -and $BUILD_START_TIME.AddSeconds(30) -gt (Get-Date)) {
  Start-Sleep -m 1000
}
	
Read-Host -Prompt "Press ENTER to continue"