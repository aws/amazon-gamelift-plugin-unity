param(
	[Parameter(Mandatory)]
	[Alias("ServerSdk", "SdkVersion", "Sdk")]
	[ValidatePattern("\d+\.\d+\.\d+")]
	[string] $ServerSdkVersion
)

$ROOT_DIR='.'
$STAGING_PATH="$ROOT_DIR\staging-release"
$README_PATH="$ROOT_DIR\Scripts~\PACKAGE_FOR_RELEASE.md"
$SERVER_SDK_FILENAME="GameLift-CSharp-ServerSDK-UnityPlugin-$ServerSdkVersion.zip";
$SERVER_SDK_PATH="$ROOT_DIR\$SERVER_SDK_FILENAME"

echo "Getting version number from 'package.json'"

$PLUGIN_VERSION=Select-String -LiteralPath package.json -Pattern '"version": ".*",' | % { $_.Matches.Value } | % { $_.substring(12, $_.length-14) }

echo "Identified package version as $PLUGIN_VERSION"

$TARBALL_PATH="$ROOT_DIR\com.amazonaws.gamelift-$PLUGIN_VERSION.tgz"
$DESTINATION_PATH="$ROOT_DIR\amazon-gamelift-plugin-unity-release-$PLUGIN_VERSION.zip"

echo "Staging files for release..."

New-Item -Force -Type Directory -Path $STAGING_PATH -ErrorAction Stop
Copy-Item -Force -LiteralPath $TARBALL_PATH, $SERVER_SDK_PATH -Destination $STAGING_PATH -ErrorAction Stop
Copy-Item -Force -LiteralPath $README_PATH -Destination "$STAGING_PATH\README.md" -ErrorAction Stop

echo "Packaging zip file for release..."

Compress-Archive -Force -Path "$STAGING_PATH\*" -DestinationPath $DESTINATION_PATH -ErrorAction Stop

echo "Packaging successful! Release artifact located at '$DESTINATION_PATH'"

exit 0