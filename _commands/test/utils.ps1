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

