# Resolve paths
$flexlibPath       = Resolve-Path "$PSScriptRoot/../flexlib.ps1"
$flexlibDataPath   = "$HOME/Projects/Incubator/FlexLib/Dev/builds/last/Debug/net8.0/data"
$dataPath          = "$PSScriptRoot/data"  
$resultsPath       = "$dataPath/results"
$item1             = "$dataPath/input/Item1.pdf"
$item2             = "$dataPath/input/Item2.pdf"
$item3             = "$dataPath/input/Item3.pdf"
$item4             = "$dataPath/input/Item4.pdf"

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

Write-Host ""
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
& $flexlibPath set-prop author newton Item1 TestLibrary 
& $flexlibPath set-prop author pascal Item2 TestLibrary 
& $flexlibPath set-prop author einstein Item3 TestLibrary 
& $flexlibPath set-prop author euler Item4 TestLibrary 

& $flexlibPath set-prop publisher dover Item1 TestLibrary 
& $flexlibPath set-prop publisher dover Item2 TestLibrary 
& $flexlibPath set-prop publisher 'nova fronteira' Item3 TestLibrary 
& $flexlibPath set-prop publisher 'alta books' Item4 TestLibrary 

& $flexlibPath set-prop theme mathematics Item1 TestLibrary 
& $flexlibPath set-prop theme physics Item1 TestLibrary 

& $flexlibPath set-prop theme mathematics Item2 TestLibrary 
& $flexlibPath set-prop theme philosophy Item2 TestLibrary 

& $flexlibPath set-prop theme mathematics Item3 TestLibrary 
& $flexlibPath set-prop theme history Item3 TestLibrary 

& $flexlibPath set-prop theme history Item4 TestLibrary 
& $flexlibPath set-prop theme physics Item4 TestLibrary 

& $flexlibPath set-prop year 1920 Item1 TestLibrary 
& $flexlibPath set-prop year 1940 Item2 TestLibrary 
& $flexlibPath set-prop year 1945 Item3 TestLibrary 
& $flexlibPath set-prop year 2001 Item4 TestLibrary 
# --------------------------------------------------------
& $flexlibPath set-layout TestLibrary theme/publisher/author/year
# --------------------------------------------------------


