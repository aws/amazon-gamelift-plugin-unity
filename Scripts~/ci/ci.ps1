echo "Running Unity Plugin CI Scripts..."

echo "Fetching Unity Activation Secrets..."
$UNITY_SECRETS = aws secretsmanager --region us-west-2 get-secret-value --secret-id UnityLicenseActivationSecrets --query SecretString --output text | ConvertFrom-Json
$UNITY_USERNAME = $UNITY_SECRETS.UnityUsername
$UNITY_PASSWORD = $UNITY_SECRETS.UnityPassword
$UNITY_SERIAL = $UNITY_SECRETS.UnitySerial

echo "Activating Unity..."
mkdir build
rm C:\ProgramData\Unity\Unity_lic.ulf
Start-Process -FilePath "C:\Unity\Editor\Unity.exe" -Wait -NoNewWindow -ArgumentList "-batchmode -nographics -quit -username $UNITY_USERNAME -password $UNITY_PASSWORD -serial $UNITY_SERIAL -projectPath .\Samples~\SampleGame -logfile build/UnityActivationLog.txt"
echo "Unity Activated!"

try {
    & "$PSScriptRoot\.ci-build-core-library.ps1"
    & "$PSScriptRoot\.ci-build-gamelift-server-sdk.ps1"

    $error.clear()

    try {
        & "$PSScriptRoot\.ci-test-core.ps1"
    } catch {
        echo $error
        echo "Core testing failed."
        # no error to preserve Unity logs
        exit
    }

    $error.clear()

    try {
        & "$PSScriptRoot\.ci-test-plugin.ps1"
    } catch {
        echo $error
        echo "Plugin testing failed."
        # no error to preserve Unity logs
        exit
    }

    $error.clear()

    try {
        & "$PSScriptRoot\.ci-test-sample-game.ps1"
    } catch {
        echo $error
        echo "Sample testing failed."
        # no error to preserve Unity logs
        exit
    }

    try {
        & "$PSScriptRoot\.ci-build-sample-game.ps1"
    } catch {
        echo $error
        echo "Sample export failed."
        # no error to preserve Unity logs
        exit
    }

    # Install Node.JS just now to preserve previous output. For some reason nothing is written after it.
    choco install -y nodejs
    $env:Path += ";C:\Program Files\nodejs"
    & "$PSScriptRoot\.ci-export.ps1"

    if ($?) {
        echo .
        echo "CI complete"
    }
} finally {
    echo "Deactivating Unity..."
    Start-Process -FilePath "C:\Unity\Editor\Unity.exe" -Wait -NoNewWindow -ArgumentList "-batchmode -username $UNITY_SECRETS.UnityUsername -password $UNITY_SECRETS.UnityPassword -returnlicense -logfile build/UnityDeactivationLog.txt"
    echo "Unity deactivated!"
}
