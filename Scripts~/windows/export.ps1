$ROOT_DIR='.'
$RUNTIME_PATH="$ROOT_DIR\Runtime"
$DESTINATION_PATH="$ROOT_DIR\.build"

if (-Not (Test-Path -Path $RUNTIME_PATH))
{
	echo "$RUNTIME_PATH directory is not found in the working directory. Make sure you are executing the script from the project root."
	Read-Host -Prompt "Press ENTER to continue"
	exit 1
}

if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null) 
{ 
	Write-Host "Unable to find 'npm' executable in your PATH. Please install nodejs first: https://nodejs.org/en/download/"
	exit 1
}

npm pack
