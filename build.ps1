echo .
echo "Building Amazon GameLift Unity Plugin artifacts into a tarball file"
Unity.exe -batchmode -quit -projectPath "GameLift-Unity" -executeMethod AWS.GameLift.Editor.PluginPacker.Pack

echo .
echo "Build completed"
Read-Host -Prompt "Press ENTER to continue"