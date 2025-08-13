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
            $projectPath
            "-c", $Configuration
            "-p:Version=$Version"
            "-p:InformationalVersion=$Version+build.$BuildId"
            "-p:RuntimeIdentifier=$RuntimeId"
            "-v:q"
        )

        if ($Configuration -ieq "Release") {
            # Use dotnet publish for Release builds
            $publishArgs = $commonArgs + @(
                "--self-contained", "true",
                "--output", (Join-Path $PSScriptRoot "..\..\builds\last\bin\Release\$RuntimeId")
            )
            $output = dotnet publish @publishArgs 2>&1
        }
        else {
            # Use dotnet build for Debug builds
            $output = dotnet build @commonArgs 2>&1
        }

        $allOutputs += $output
    }

    return $allOutputs
}