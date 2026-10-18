# 一键构建安装包
#
#   1. dotnet publish 出「共享 runtime」单文件版本（dist/framework-dep）
#   2. 用 Inno Setup 编译安装包（installer/Output）
#
# 用户端需要 .NET 10 Desktop Runtime（安装时会检测，缺失则询问后自动安装）。
# 用法：pwsh -File installer/build-installer.ps1

param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$proj = Join-Path $repo 'src\WallpaperEngine_Extractor\WallpaperEngine_Extractor.csproj'
$dist = Join-Path $repo 'dist\framework-dep'
$iss = Join-Path $PSScriptRoot 'installer.iss'
$iscc = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'

if (-not (Test-Path $iscc)) {
    throw "找不到 Inno Setup 6 编译器：$iscc"
}

Write-Host "==> publish (framework-dependent, single file)" -ForegroundColor Cyan
Remove-Item $dist -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish $proj -c $Configuration -r $Runtime `
    -p:PublishSingleFile=true -p:SelfContained=false `
    -v minimal -o $dist
if ($LASTEXITCODE -ne 0) { throw "dotnet publish 失败" }

Write-Host "==> compile installer" -ForegroundColor Cyan
& $iscc $iss
if ($LASTEXITCODE -ne 0) { throw "ISCC 编译失败" }

$setup = Join-Path $PSScriptRoot 'Output\WallpaperEngine_Extractor_Setup.exe'
$size = [math]::Round((Get-Item $setup).Length / 1MB, 2)
Write-Host "==> done: $setup ($size MB)" -ForegroundColor Green
