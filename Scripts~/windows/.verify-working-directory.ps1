$ROOT_DIR="."
$RUNTIME_PATH="$ROOT_DIR\Runtime"

if (-Not (Test-Path -Path $RUNTIME_PATH))
{
	echo "$RUNTIME_PATH is not found in the working directory. Make sure you are executing the script from the project root."
	exit 1
}

exit 0