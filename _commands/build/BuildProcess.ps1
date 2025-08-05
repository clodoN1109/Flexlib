function ExecuteBuildProcess {
    param (
        [string]$Configuration = "Debug",
        [int]$BuildId,
        [string]$Version
    )

    if (-not $Version) { $Version = "0.0.0" }

    $projectPath = Join-Path $PSScriptRoot "..\..\src\Flexlib.csproj"
    $runtimeId = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier

    $commonArgs = @(
        $projectPath
        "-c", $Configuration
        "-p:Version=$Version"
        "-p:InformationalVersion=$Version+build.$BuildId"
        "-p:RuntimeIdentifier=$runtimeId"
        "-v:q"
    )

    if ($Configuration -ieq "Release") {
        # Use dotnet publish for Release builds
        $publishArgs = $commonArgs + @(
            "--self-contained", "true",
            "--output", (Join-Path $PSScriptRoot "..\..\builds\last\bin\Release\$runtimeId")
        )
        $output = dotnet publish @publishArgs 2>&1
    }
    else {
        # Use dotnet build for Debug builds
        $output = dotnet build @commonArgs 2>&1
    }

    return $output
}

