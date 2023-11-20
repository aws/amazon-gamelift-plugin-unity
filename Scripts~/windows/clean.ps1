& "$PSScriptRoot\.clean-builds.ps1"
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\.clean-exports.ps1" }
exit $LASTEXITCODE
