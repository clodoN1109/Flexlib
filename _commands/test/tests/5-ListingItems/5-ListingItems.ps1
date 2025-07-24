param (
    [switch]$UpdateReferences
)

# Resolve paths
$flexlibPath       = Resolve-Path "$PSScriptRoot/../../../flexlib.ps1"
$flexlibDataPath   = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/data"
$dataPath          = "$PSScriptRoot/data"  
$resultsPath       = "$dataPath/results"
$referencesPath     = "$dataPath/references"
$item1             = "$dataPath/input/Item1.pdf"
$item2             = "$dataPath/input/Item2.pdf"
$item3             = "$dataPath/input/Item3.pdf"
$item4             = "$dataPath/input/Item4.pdf"

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

# Run test
& $flexlibPath new-lib TestLibrary $resultsPath
# --------------------------------------------------------
& $flexlibPath new-item $item1 Item1 TestLibrary
& $flexlibPath new-item $item2 Item2 TestLibrary
& $flexlibPath new-item $item3 Item3 TestLibrary
& $flexlibPath new-item $item4 Item4 TestLibrary
# --------------------------------------------------------
& $flexlibPath new-prop author TestLibrary string
& $flexlibPath new-prop publisher TestLibrary string
& $flexlibPath new-prop theme TestLibrary list
& $flexlibPath new-prop year TestLibrary int
# --------------------------------------------------------
& $flexlibPath set-prop author newton 1 TestLibrary 
& $flexlibPath set-prop author pascal 2 TestLibrary 
& $flexlibPath set-prop author einstein 3 TestLibrary 
& $flexlibPath set-prop author euler 4 TestLibrary 

& $flexlibPath set-prop publisher dover 1 TestLibrary 
& $flexlibPath set-prop publisher dover 2 TestLibrary 
& $flexlibPath set-prop publisher 'nova fronteira' 3 TestLibrary 
& $flexlibPath set-prop publisher 'alta books' 4 TestLibrary 

& $flexlibPath set-prop theme mathematics 1 TestLibrary 
& $flexlibPath set-prop theme physics 1 TestLibrary 

& $flexlibPath set-prop theme mathematics 2 TestLibrary 
& $flexlibPath set-prop theme philosophy 2 TestLibrary 

& $flexlibPath set-prop theme mathematics 3 TestLibrary 
& $flexlibPath set-prop theme history 3 TestLibrary 

& $flexlibPath set-prop theme history 4 TestLibrary 
& $flexlibPath set-prop theme physics 4 TestLibrary 

& $flexlibPath set-prop year 1920 1 TestLibrary 
& $flexlibPath set-prop year 1940 2 TestLibrary 
& $flexlibPath set-prop year 1945 3 TestLibrary 
& $flexlibPath set-prop year 2001 4 TestLibrary 
# --------------------------------------------------------
& $flexlibPath set-layout TestLibrary theme/publisher/author/year
# --------------------------------------------------------
$output = & $flexlibPath list-items TestLibrary 'mathematics/*/newton,pascal,euler/1935-2002'
$output | Set-Content "$resultsPath/output.txt"
# --------------------------------------------------------

if ($UpdateReferences) {

    Safe-Cleanup $referencesPath
    Safe-CopyItems $resultsPath     $referencesPath
    Safe-CopyItems $flexlibDataPath $referencesPath
}

# Compare artifacts
$diff_1 = Compare-Folders -Expected "$referencesPath/TestLibrary" -Actual "$resultsPath/TestLibrary"
$diff_2 = Compare-Object `
    (Get-Content "$flexlibDataPath/libraries.json") `
    (Get-Content "$referencesPath/libraries.json")
$diff_3 = Compare-Object `
    (Get-Content "$resultsPath/output.txt") `
    (Get-Content "$referencesPath/output.txt")

# Clean up after test
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

# Return result
if ($diff_1 -or $diff_2 -or $diff_3) {
    return $false
} else {
    return $true
}
