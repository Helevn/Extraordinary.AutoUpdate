$DIST_Client = "../dist/Extraordinary.App"

$DIST_FILE = "../release/Extraordinary.App.zip"

Get-ChildItem -Path . -Directory -Filter "bin" -Recurse | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Get-ChildItem -Path . -Directory -Filter "obj" -Recurse | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

Get-ChildItem -Path . -Directory -Filter "paket-files" | ForEach-Object { 
    Remove-Item -Path $_.FullName -Recurse -Force 
}

dotnet restore
if ($LASTEXITCODE -ne 0) {
    throw ;
}

dotnet build 
if ($LASTEXITCODE -ne 0) {
    throw ;
}

dotnet publish .\Extraordinary.App\Extraordinary.App.csproj -c Release -f net6.0-windows10.0.17763.0 --runtime win-x64 --self-contained false -o $DIST_Client
if ($LASTEXITCODE -ne 0) {
    throw ;
}

$Exist=Test-Path "../release" 
if($Exist -ne "True")
{
    New-Item -Path "../release" -type directory
}

Compress-Archive -Path $DIST_Client -DestinationPath $DIST_FILE -Force
Remove-Item -Path "../dist" -Recurse

Write-Host "app is published: " -NoNewline -ForegroundColor Green
Write-Host $DIST_FILE -ForegroundColor Blue

