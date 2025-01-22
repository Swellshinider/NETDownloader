param (
    [switch]$Release
)

# Configuration based on the -r flag
$configuration = if ($r) { "Release" } else { "Debug" }

dotnet build --configuration $configuration
Start-Process -FilePath "dotnet" -ArgumentList "run --project .\NETDownloader\ --configuration $configuration"