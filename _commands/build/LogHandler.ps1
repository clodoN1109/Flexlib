. "$PSScriptRoot\LogBeautifier.ps1"
. "$PSScriptRoot\LogParser.ps1"

function HandleLog($LogStream) {

    $logsGroupedByProjectAndFile = GroupLogsByProjectAndFile $LogStream 

# Counting Errors and Warnings
    $errorCount = 0
    $warningCount = 0
    foreach ($project in $logsGroupedByProjectAndFile.Values) {
        foreach ($fileEntries in $project.Values) {
            foreach ($entry in $fileEntries) {
                if ($entry.Type -eq 'error')   { $errorCount++ }
                elseif ($entry.Type -eq 'warning') { $warningCount++ }
            }
        }
    }

# Print organized output
    PrintABeautifulLog $logsGroupedByProjectAndFile $errorCount $warningCount

    return $errorCount, $WarningCount

}
