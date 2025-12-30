$DIST_Client = "dist/Extraordinary.App"

Get-ChildItem -Path . -Directory -Filter "bin" -Recurse | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Get-ChildItem -Path . -Directory -Filter "obj" -Recurse | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Get-ChildItem -Path . -Directory -Filter "paket-files" | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Remove-Item -Path "dist" -Recurse

dotnet restore src\Extraordinary.sln
if ($LASTEXITCODE -ne 0) {
    throw ;
}

dotnet build src\Extraordinary.sln
if ($LASTEXITCODE -ne 0) {
    throw ;
}

dotnet publish src\Extraordinary.App\Extraordinary.App.csproj `
    -c Release `
    -f net8.0-windows10.0.17763.0 `
    --runtime win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:IncludeAppHostInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $DIST_Client

if ($LASTEXITCODE -ne 0) {
    throw ;
}

Write-Host "app is published: " -NoNewline -ForegroundColor Green
Write-Host $DIST_Client -ForegroundColor Blue

