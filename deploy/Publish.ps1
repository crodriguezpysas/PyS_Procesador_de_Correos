param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Output = "./publish/win-x64"
)

$project = "../src/ProcesadorCorreosPYS.Wpf/ProcesadorCorreosPYS.Wpf.csproj"

dotnet publish $project `
  -c $Configuration `
  -r $Runtime `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  -o $Output

Write-Host "Publicación generada en $Output"
