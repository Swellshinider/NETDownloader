param (
    [switch]$Release
)

if ($Release) {
    dotnet build --configuration Release
    dotnet run --project .\NETDownloader --configuration Release
} else {
    dotnet build --configuration Debug
    dotnet run --project .\NETDownloader --configuration Debug
}