# Input files
$itemPaths = @(
    "$dataPath/input/Item1.pdf",
    "$dataPath/input/Item2.pdf",
    "$dataPath/input/Item3.pdf",
    "$dataPath/input/Item4.pdf",
    "$dataPath/input/Item5.pdf",
    "$dataPath/input/Item6.pdf",
    "$dataPath/input/Item7.pdf",
    "$dataPath/input/Item8.pdf",
    "$dataPath/input/Item9.pdf"
)

# Clean up previous test output
$ProgressPreference = 'SilentlyContinue'
Safe-Cleanup $resultsPath
Safe-Cleanup $flexlibDataPath

Write-Host ""

# ========== LIBRARIES =========

& $flexlibPath new-lib GeneralLibrary  $resultsPath
& $flexlibPath new-lib ScienceLibrary  $resultsPath
& $flexlibPath new-lib CultureLibrary  $resultsPath

# ========== ITEMS ==========

& $flexlibPath new-item $itemPaths[0] Item1  GeneralLibrary
& $flexlibPath new-item $itemPaths[1] Item2  GeneralLibrary
& $flexlibPath new-item $itemPaths[2] Item3  GeneralLibrary

& $flexlibPath new-item $itemPaths[3] Item4  ScienceLibrary
& $flexlibPath new-item $itemPaths[4] Item5  ScienceLibrary
& $flexlibPath new-item $itemPaths[5] Item6  ScienceLibrary

& $flexlibPath new-item $itemPaths[6] Item7  CultureLibrary
& $flexlibPath new-item $itemPaths[7] Item8  CultureLibrary
& $flexlibPath new-item $itemPaths[8] Item9  CultureLibrary

# ========== PROPERTIES ==========

$allLibs = @("GeneralLibrary", "ScienceLibrary", "CultureLibrary")

foreach ($lib in $allLibs) {
    & $flexlibPath new-prop author        $lib string
    & $flexlibPath new-prop publisher     $lib string
    & $flexlibPath new-prop theme         $lib list
    & $flexlibPath new-prop year          $lib int
    & $flexlibPath new-prop language      $lib string
    & $flexlibPath new-prop difficulty    $lib string
}

# ========== PROPERTIES VALUES ==========

# GeneralLibrary Items
& $flexlibPath set-prop author      Newton              Item1 GeneralLibrary
& $flexlibPath set-prop author      Pascal              Item2 GeneralLibrary
& $flexlibPath set-prop author      Leibniz             Item3 GeneralLibrary

& $flexlibPath set-prop publisher   dover               Item1 GeneralLibrary
& $flexlibPath set-prop publisher   springer            Item2 GeneralLibrary
& $flexlibPath set-prop publisher   'cambridge press'   Item3 GeneralLibrary

& $flexlibPath set-prop theme       mathematics         Item1 GeneralLibrary
& $flexlibPath set-prop theme       physics             Item1 GeneralLibrary
& $flexlibPath set-prop theme       logic               Item2 GeneralLibrary
& $flexlibPath set-prop theme       philosophy          Item3 GeneralLibrary

& $flexlibPath set-prop year        1687                Item1 GeneralLibrary
& $flexlibPath set-prop year        1654                Item2 GeneralLibrary
& $flexlibPath set-prop year        1710                Item3 GeneralLibrary

& $flexlibPath set-prop language    latin               Item1 GeneralLibrary
& $flexlibPath set-prop language    french              Item2 GeneralLibrary
& $flexlibPath set-prop language    german              Item3 GeneralLibrary

& $flexlibPath set-prop difficulty  hard                Item1 GeneralLibrary
& $flexlibPath set-prop difficulty  medium              Item2 GeneralLibrary
& $flexlibPath set-prop difficulty  hard                Item3 GeneralLibrary

# ScienceLibrary Items
& $flexlibPath set-prop author      Curie               Item4 ScienceLibrary
& $flexlibPath set-prop author      Einstein            Item5 ScienceLibrary
& $flexlibPath set-prop author      Feynman             Item6 ScienceLibrary

& $flexlibPath set-prop publisher   "nobel house"       Item4 ScienceLibrary
& $flexlibPath set-prop publisher   "princeton"         Item5 ScienceLibrary
& $flexlibPath set-prop publisher   "mit press"         Item6 ScienceLibrary

& $flexlibPath set-prop theme       chemistry           Item4 ScienceLibrary
& $flexlibPath set-prop theme       physics             Item5 ScienceLibrary
& $flexlibPath set-prop theme       quantum             Item6 ScienceLibrary

& $flexlibPath set-prop year        1911                Item4 ScienceLibrary
& $flexlibPath set-prop year        1916                Item5 ScienceLibrary
& $flexlibPath set-prop year        1965                Item6 ScienceLibrary

& $flexlibPath set-prop language    french              Item4 ScienceLibrary
& $flexlibPath set-prop language    german              Item5 ScienceLibrary
& $flexlibPath set-prop language    english             Item6 ScienceLibrary

& $flexlibPath set-prop difficulty  hard                Item4 ScienceLibrary
& $flexlibPath set-prop difficulty  hard                Item5 ScienceLibrary
& $flexlibPath set-prop difficulty  medium              Item6 ScienceLibrary

# CultureLibrary Items
& $flexlibPath set-prop author      Homer               Item7 CultureLibrary
& $flexlibPath set-prop author      Shakespeare         Item8 CultureLibrary
& $flexlibPath set-prop author      Camus               Item9 CultureLibrary

& $flexlibPath set-prop publisher   penguin             Item7 CultureLibrary
& $flexlibPath set-prop publisher   'oxford press'        Item8 CultureLibrary
& $flexlibPath set-prop publisher   gallimard           Item9 CultureLibrary

& $flexlibPath set-prop theme       literature          Item7 CultureLibrary
& $flexlibPath set-prop theme       tragedy             Item8 CultureLibrary
& $flexlibPath set-prop theme       absurd              Item9 CultureLibrary

& $flexlibPath set-prop year        '-700'              Item7 CultureLibrary
& $flexlibPath set-prop year        1603                Item8 CultureLibrary
& $flexlibPath set-prop year        1942                Item9 CultureLibrary

& $flexlibPath set-prop language    greek               Item7 CultureLibrary
& $flexlibPath set-prop language    english             Item8 CultureLibrary
& $flexlibPath set-prop language    french              Item9 CultureLibrary

& $flexlibPath set-prop difficulty  medium              Item7 CultureLibrary
& $flexlibPath set-prop difficulty  hard                Item8 CultureLibrary
& $flexlibPath set-prop difficulty  medium              Item9 CultureLibrary

# ========== LAYOUTS ==========

& $flexlibPath set-layout GeneralLibrary  theme/publisher/author/year
& $flexlibPath set-layout ScienceLibrary  theme/author/year
& $flexlibPath set-layout CultureLibrary  author/theme

