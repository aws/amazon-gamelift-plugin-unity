$ROOT_DIR="."
$RUNTIME_PATH="$ROOT_DIR\Runtime"

if (-Not (Test-Path -Path $RUNTIME_PATH))
{
	Write-Host "$RUNTIME_PATH is not found in the working directory. Make sure you are executing the script from the project root." -ForegroundColor Red
	exit 1
}

exit 0