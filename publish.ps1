$DIST_Client = "dist/Extraordinary.App"
$DIST_WebApi = "dist/Extraordinary.WebApi"

Write-Host "clear..." -ForegroundColor Yellow

Get-ChildItem -Path . -Directory -Filter "bin" -Recurse | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Get-ChildItem -Path . -Directory -Filter "obj" -Recurse | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Get-ChildItem -Path . -Directory -Filter "paket-files" | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Remove-Item -Path "dist" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "dotnet restore..." -ForegroundColor Yellow
dotnet restore src\Extraordinary.sln
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore Error"
}

Write-Host "dotnet build src\Extraordinary.sln..." -ForegroundColor Yellow
dotnet build src\Extraordinary.sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build Error"
}

Write-Host "dotnet publish Extraordinary.App..." -ForegroundColor Yellow
dotnet publish src\Extraordinary.App\Extraordinary.App.csproj `
    -c Release `
    -f net10.0-windows10.0.17763.0 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:IncludeAppHostInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $DIST_Client

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish Error Extraordinary.App"
}

Write-Host "dotnet publish Extraordinary.App Success: " -NoNewline -ForegroundColor Green
Write-Host $DIST_Client -ForegroundColor Blue

Write-Host "dotnet publish Extraordinary.WebApi..." -ForegroundColor Yellow
dotnet publish src\Extraordinary.WebApi\Extraordinary.WebApi.csproj `
    -c Release `
    -f net10.0 `
    --self-contained false `
    -p:IncludeAppHostInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $DIST_WebApi

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish Error Extraordinary.WebApi"
}

Write-Host "dotnet publish Extraordinary.WebApi Success: " -NoNewline -ForegroundColor Green
Write-Host $DIST_WebApi -ForegroundColor Blue
