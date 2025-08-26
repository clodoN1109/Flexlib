function ExecuteBuildProcess {
    param (
        [string]$Configuration = "Debug",
        [int]$BuildId,
        [string]$Version,
        [string[]]$RuntimeIds = @([System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier)
    )

    if (-not $Version) { $Version = "0.0.0" }

    $projectPath = Join-Path $PSScriptRoot "..\..\src\Flexlib.csproj"
    $allOutputs = @()

    foreach ($RuntimeId in $RuntimeIds) {
        $commonArgs = @(
            $projectPath,
            "-c", $Configuration,
            "-p:Version=$Version",
            "-p:InformationalVersion=$Version+build.$BuildId",
            "-p:RuntimeIdentifier=$RuntimeId",
            "-v:q"
        )
        if ($Configuration -ieq "Release") {
            # Normalize RIDs
            if ($RuntimeId -eq "win10-x64") { $RuntimeId = "win-x64" }

            # Specify target framework (hardcode or read from csproj)
            $TargetFramework = "net8.0"

            # Compose output path including RID and TFM
            $OutputPath = Join-Path $PSScriptRoot "..\..\builds\last\Release\single-file\$TargetFramework\$RuntimeId"

            $publishArgs = $commonArgs + @(
                "--self-contained", "true",
                "-p:PublishSingleFile=true",
                "-p:PublishTrimmed=false",
                "-p:PublishReadyToRun=true",
                "-p:IncludeAllContentForSelfExtract=true",
                "-p:IncludeNativeLibrariesForSelfExtract=true",
                "-p:DebugType=None",
                "-p:DebugSymbols=false",
                "--output", $OutputPath
            )

            $output = dotnet publish @publishArgs 2>&1
        }
        else {
            $output = dotnet build @commonArgs 2>&1
        }

        $allOutputs += $output
    }

    return $allOutputs
}
