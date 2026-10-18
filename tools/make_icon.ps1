$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$srcPng = 'D:\Development\WallpaperEngine-Extractor\logo.png'
$outIco = 'D:\Development\WallpaperEngine-Extractor\src\WallpaperEngine_Extractor\app.ico'

# 正方形画布边长与内边距（logo 本身 589x577，几乎正方，居中留少量透明边）
$master = 1024
$padRatio = 0.02
$sizes = @(16, 20, 24, 32, 40, 48, 64, 96, 128, 256)

$src = [System.Drawing.Image]::FromFile($srcPng)
try {
    $canvas = New-Object System.Drawing.Bitmap($master, $master, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($canvas)
    try {
        $g.Clear([System.Drawing.Color]::Transparent)
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

        $inner = [int]($master * (1 - 2 * $padRatio))
        $scale = [Math]::Min($inner / $src.Width, $inner / $src.Height)
        $w = [int][Math]::Round($src.Width * $scale)
        $h = [int][Math]::Round($src.Height * $scale)
        $x = [int](($master - $w) / 2)
        $y = [int](($master - $h) / 2)
        $g.DrawImage($src, $x, $y, $w, $h)
    } finally { $g.Dispose() }

    # 生成各尺寸 PNG 载荷
    $payloads = @()
    foreach ($size in $sizes) {
        $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $sg = [System.Drawing.Graphics]::FromImage($bmp)
        try {
            $sg.Clear([System.Drawing.Color]::Transparent)
            $sg.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $sg.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $sg.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $sg.DrawImage($canvas, 0, 0, $size, $size)
        } finally { $sg.Dispose() }

        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $bmp.Dispose()
        $payloads += ,@($size, $ms.ToArray())
        $ms.Dispose()
    }

    # 写 ICO 容器：ICONDIR + ICONDIRENTRY[]
    $fs = [System.IO.File]::Create($outIco)
    $bw = New-Object System.IO.BinaryWriter($fs)
    try {
        $bw.Write([UInt16]0)              # reserved
        $bw.Write([UInt16]1)              # type = icon
        $bw.Write([UInt16]$payloads.Count)

        $offset = 6 + 16 * $payloads.Count
        foreach ($p in $payloads) {
            $size = [int]$p[0]
            $data = [byte[]]$p[1]
            $dim = if ($size -ge 256) { 0 } else { $size }
            $bw.Write([byte]$dim)         # width
            $bw.Write([byte]$dim)         # height
            $bw.Write([byte]0)            # color count
            $bw.Write([byte]0)            # reserved
            $bw.Write([UInt16]1)          # planes
            $bw.Write([UInt16]32)         # bit count
            $bw.Write([UInt32]$data.Length)
            $bw.Write([UInt32]$offset)
            $offset += $data.Length
        }

        foreach ($p in $payloads) {
            $bw.Write([byte[]]$p[1])
        }
    } finally {
        $bw.Dispose()
        $fs.Dispose()
    }

    $canvas.Dispose()
} finally {
    $src.Dispose()
}

$ico = Get-Item $outIco
"Icon written: $($ico.FullName) ($($ico.Length) bytes)"
