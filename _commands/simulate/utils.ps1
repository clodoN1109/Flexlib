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

    # Optionally: confirm (remove for automation)
    Write-Host "`nAbout to delete contents of '$resolvedPath'. Are you sure? [y/N]`n"
    $confirm = Read-Host
    if ($confirm -ne 'y') {
        Write-Host "`nSkipped cleanup of '$resolvedPath'.`n"
        return
    }

    Get-ChildItem -Path $resolvedPath.Path -Force |
        Remove-Item -Recurse -Force -ErrorAction Stop

    Write-Host "`nCleaned: $resolvedPath`n"
}

