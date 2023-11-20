& "$PSScriptRoot\clean.ps1"
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\build.ps1" }
if ($LASTEXITCODE -eq 0) { & "$PSScriptRoot\export.ps1" }
exit $LASTEXITCODE
