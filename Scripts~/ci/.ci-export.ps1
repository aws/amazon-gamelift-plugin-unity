$ROOT_DIR = "."
$BUILD__PATH="build"

echo "Running export..."

if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null) {
	throw "NPM is not in PATH"
}

cd $ROOT_DIR
npm pack --pack-destination $BUILD__PATH

echo .
echo "Export complete"
