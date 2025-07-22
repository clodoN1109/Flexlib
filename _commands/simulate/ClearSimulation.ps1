# Resolve paths
$dataPath          = "$PSScriptRoot/data"  
$resultsPath       = "$dataPath/results"
$flexlibDataPath   = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/data"

# Clean up previous test output
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

