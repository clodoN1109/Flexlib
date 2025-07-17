function GroupLogsByProjectAndFile ( $LogStream ) {

    $logsGroupedByProjectAndFile = @{}

    foreach ($line in $LogStream) {
        if ($line -match "^(?<file>.*\.cs)\((?<line>\d+),\d+\): (?<type>warning|error) (?<code>CS\d{4}): (?<message>.+?) \[(?<project>.+\.csproj)\]") {
            $entry = @{
                File    = $matches.file
                Line    = $matches.line
                Type    = $matches.type.ToLower()
                Code    = $matches.code
                Message = $matches.message.Trim()
            }

            $project = $matches.project

            if (-not $logsGroupedByProjectAndFile.ContainsKey($project)) {
                $logsGroupedByProjectAndFile[$project] = @{}
            }

            if (-not ($logsGroupedByProjectAndFile[$project].ContainsKey($entry.File))) {
                $logsGroupedByProjectAndFile[$project][$entry.File] = @()
            }

            # Avoid duplicate entries
            $entryKey = "$($entry.File):$($entry.Line):$($entry.Type):$($entry.Code):$($entry.Message)"
            $existingKeys = $logsGroupedByProjectAndFile[$project][$entry.File] | ForEach-Object {
                "$($_.File):$($_.Line):$($_.Type):$($_.Code):$($_.Message)"
            }

            if (-not ($existingKeys -contains $entryKey)) {
                $logsGroupedByProjectAndFile[$project][$entry.File] += $entry
            }
        }
    }

    return $logsGroupedByProjectAndFile
}
