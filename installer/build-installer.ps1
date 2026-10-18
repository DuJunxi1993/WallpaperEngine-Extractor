# 一键构建安装包
#
#   1. dotnet publish 出「共享 runtime」单文件版本（dist/framework-dep）
#   2. 用 Inno Setup 编译安装包（installer/Output）
#   3. 把 setup.exe 的清单改写成 requireAdministrator
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

# ---------------------------------------------------------------------------
# 强制 setup.exe 以管理员身份启动
#
# Inno Setup 6.3+ 即使写了 PrivilegesRequired=admin，生成的 setup.exe 清单也是
# asInvoker：它会先解包到 %TEMP%，到安装阶段才请求提权。用户看到的 "Access is
# denied" 往往就发生在这段先解包的逻辑里。这里把清单改成 requireAdministrator，
# 让提权从进程启动那一刻就生效。
# ---------------------------------------------------------------------------
Write-Host "==> patch setup manifest to requireAdministrator" -ForegroundColor Cyan
& (Join-Path $PSScriptRoot 'patch-manifest.ps1') -Exe $setup
if ($LASTEXITCODE -ne 0) { throw "清单改写失败" }

$size = [math]::Round((Get-Item $setup).Length / 1MB, 2)
Write-Host "==> done: $setup ($size MB, requires administrator)" -ForegroundColor Green
