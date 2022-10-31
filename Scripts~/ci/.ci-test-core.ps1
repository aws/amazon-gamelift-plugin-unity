$ROOT_DIR = "."
$SAMPLE_PROJECT_PATH = "$ROOT_DIR\Samples~\SampleGame"
$ABS_DIR = Resolve-Path -LiteralPath $ROOT_DIR
$ABS_LOG_DIR_PATH = "$ABS_DIR\build"
$SAMPLE_TEST_LOG_PATH="$ROOT_DIR\build\CoreTestLog.txt"
$TEST_ASM = "AmazonGameLiftPlugin.Runtime.UnitTests"

echo "Running Core tests..."

if (!(Test-Path -Path $ABS_LOG_DIR_PATH -PathType Container)) {
	mkdir -Path $ABS_LOG_DIR_PATH
}

Start-Process -FilePath "C:\Unity\Editor\Unity.exe" -Wait -NoNewWindow -ArgumentList "-batchmode -nographics -runTests -projectPath $SAMPLE_PROJECT_PATH -logFile $SAMPLE_TEST_LOG_PATH -testResults $ABS_LOG_DIR_PATH\test_results_core.xml -testPlatform playmode -assemblyNames $TEST_ASM"

echo .
echo "Tests complete"