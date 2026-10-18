# 把 setup.exe 的清单从 asInvoker 改成 requireAdministrator（等长字节替换）
#
# 为什么不用 mt.exe：Inno Setup 的安装数据是追加在 PE 映像之后的 overlay，
# mt.exe 重写资源时会把 overlay 丢掉（实测 4.78 MB -> 0.92 MB）。
# 这里只做「等长」替换：requireAdministrator 比 asInvoker 多 11 字节，
# 从清单里那段 278 个空格的空白里剪掉 11 个补回来，文件总长度不变，
# PE 结构和 overlay 全部保持原样。
#
# 用法：pwsh -File installer/patch-manifest.ps1 -Exe <setup.exe>

param(
    [Parameter(Mandatory = $true)]
    [string]$Exe,

    [string]$From = 'level="asInvoker"',
    [string]$To   = 'level="requireAdministrator"'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Exe)) { throw "找不到文件：$Exe" }

$bytes = [System.IO.File]::ReadAllBytes($Exe)
$originalLength = $bytes.Length
$ascii = [System.Text.Encoding]::ASCII.GetString($bytes)

if ($ascii.Contains($To)) {
    Write-Host "清单已是 requireAdministrator，跳过。"
    return
}

$hit = $ascii.IndexOf($From)
if ($hit -lt 0) { throw "清单里找不到 $From" }
if ($ascii.IndexOf($From, $hit + 1) -ge 0) { throw "清单里 $From 出现多次，拒绝处理" }

$start = $ascii.LastIndexOf('<?xml', $hit)
if ($start -lt 0) { throw "找不到清单起点 <?xml" }
$endTag = '</assembly>'
$end = $ascii.IndexOf($endTag, $hit)
if ($end -lt 0) { throw "找不到清单终点 </assembly>" }
$end += $endTag.Length

$block = $ascii.Substring($start, $end - $start)
$patched = $block.Replace($From, $To)

$delta = $patched.Length - $block.Length
if ($delta -le 0) { throw "替换后长度未增加（delta=$delta），逻辑有误" }

# 从清单内的空白里剪掉等量的空格，保证总长度不变
$ws = [regex]::Match($patched, '[ ]{' + $delta + ',}')
if (-not $ws.Success) { throw "清单里没有足够的空白可用于等长补偿（需要 $delta 个空格）" }

$patched = $patched.Remove($ws.Index, $delta)
if ($patched.Length -ne $block.Length) {
    throw "等长补偿失败：$($patched.Length) != $($block.Length)"
}

$patchedBytes = [System.Text.Encoding]::ASCII.GetBytes($patched)
[Array]::Copy($patchedBytes, 0, $bytes, $start, $patchedBytes.Length)
[System.IO.File]::WriteAllBytes($Exe, $bytes)

# 校验
$after = [System.IO.File]::ReadAllBytes($Exe)
if ($after.Length -ne $originalLength) {
    throw "文件长度变化：$($after.Length) != $originalLength"
}
$check = [System.Text.Encoding]::ASCII.GetString($after)
if (-not $check.Contains($To)) { throw "写入后校验失败：未找到 requireAdministrator" }
if ($check.Contains($From)) { throw "写入后校验失败：仍存在 $From" }

Write-Host ("清单已改写为 requireAdministrator（{0:N2} MB，长度不变）" -f ($after.Length / 1MB))
