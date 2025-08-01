function Write-NoNewLine {
    param (
        [string]$Text = "",
        [int]$Rest = $Host.UI.RawUI.WindowSize.Width,
        [string]$ForegroundColor = "White"
    )
    
    $Rest = $Rest - $Text.Length
    if ($Rest -lt 0) { $Rest = 0 }
    
    Write-Host $Text -NoNewLine -ForegroundColor $ForegroundColor

    return $Rest

}

function Write-Fill {
    param (
        [string]$Text = "",
        [char]$FillChar = '░',
        [string]$ForegroundColor = 'White'
    )

    $Prefix = [string]$FillChar * 4
    $width = $Host.UI.RawUI.WindowSize.Width
    $fillLength = $width - $Text.Length - $Prefix.Length
    if ($fillLength -lt 0) { $fillLength = 0 }

    Write-Host "`n$Prefix $Text $([string]$FillChar * ($fillLength - 2))`n" -ForegroundColor $ForegroundColor

}

function Safe-Cleanup {
    param (
        [string]$TargetPath
    )

    if ([string]::IsNullOrWhiteSpace($TargetPath)) {
        Write-Warning "Target path is empty. Skipping cleanup."
        return
    }

    $resolvedPath = Resolve-Path -Path $TargetPath -ErrorAction SilentlyContinue

    if (-not $resolvedPath) {
        Write-Warning "Path '$TargetPath' does not exist. Skipping."
        return
    }

    # Extra safety: prevent deletion of root directories
    $root = [System.IO.Path]::GetPathRoot($resolvedPath.Path)
    if ($resolvedPath.Path -eq $root) {
        Write-Warning "Refusing to delete contents of system root '$root'. Skipping."
        return
    }

    Get-ChildItem -Path $resolvedPath.Path -Force |
        Remove-Item -Recurse -Force -ErrorAction Stop

}

function Safe-CopyItems {
    param (
        [string]$Source,
        [string]$Destination
    )

    # Guard against null, empty, or unsafe-looking paths
    if ([string]::IsNullOrWhiteSpace($Source) -or $Source -match '^[\\\/]$') {
        Write-Warning "Aborting copy: Invalid or unsafe source path: '$Source'"
        return
    }

    if ([string]::IsNullOrWhiteSpace($Destination) -or $Destination -match '^[\\\/]$') {
        Write-Warning "Aborting copy: Invalid or unsafe destination path: '$Destination'"
        return
    }

    if (!(Test-Path $Source)) {
        Write-Warning "Source path does not exist: $Source"
        return
    }

    if (!(Test-Path $Destination)) {
        try {
            New-Item -ItemType Directory -Path $Destination -Force | Out-Null
        } catch {
            Write-Warning "Failed to create destination directory: $Destination"
            return
        }
    }

    try {
        Copy-Item "$Source/*" -Destination $Destination -Recurse -Force -ErrorAction Stop
    } catch {
        Write-Warning "Failed to copy from '$Source' to '$Destination'. $_"
    }
}

function CursorMoveX {
    param (
        [int]$Offset
    )

    $left = [System.Console]::CursorLeft + $Offset
    $top  = [System.Console]::CursorTop

    $left = [Math]::Max(0, $left)
    [System.Console]::SetCursorPosition($left, $top)
}

function SetCursorX {
    param (
        [int]$XPosition
    )

    $top  = [System.Console]::CursorTop

    [System.Console]::SetCursorPosition($XPosition, $top)
}

function CursorMoveY {
    param (
        [int]$Offset
    )

    $left = [System.Console]::CursorLeft
    $top  = [System.Console]::CursorTop + $Offset

    $top = [Math]::Max(0, $top)
    [System.Console]::SetCursorPosition($left, $top)
}

function Remove-TimeFields($obj) {
    if ($obj -is [System.Collections.IDictionary]) {
        $obj.Remove('CreatedTime')
        $obj.Remove('EditedTime')
        foreach ($key in $obj.Keys) {
            Remove-TimeFields $obj[$key]
        }
    }
    elseif ($obj -is [System.Collections.IEnumerable] -and -not ($obj -is [string])) {
        foreach ($item in $obj) {
            Remove-TimeFields $item
        }
    }
}

