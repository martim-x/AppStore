param(
    [string]$ProcessName = "Project",
    [string]$Out = "shot.png",
    [int]$Pad = 0,
    [switch]$AllWindows,
    [switch]$SmallWindow
)

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Win32Shot {
    public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int cmd);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr lp);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder sb, int max);
    [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, out RECT rect, int size);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
}
"@

# 1. Прячем оверлей агента (розовый курсор) - отдельное окно процесса cua-driver
$cua = Get-Process -Name "cua-driver" -ErrorAction SilentlyContinue
if ($cua) {
    $ids = @($cua.Id)
    $cb = [Win32Shot+EnumProc]{
        param($h, $l)
        $wpid = 0
        [void][Win32Shot]::GetWindowThreadProcessId($h, [ref]$wpid)
        if ($ids -contains $wpid) {
            $sb = New-Object System.Text.StringBuilder 256
            [void][Win32Shot]::GetWindowText($h, $sb, 256)
            if ($sb.ToString() -like "Cua.AgentCursorOverlay*") {
                [void][Win32Shot]::ShowWindow($h, 0)
            }
        }
        return $true
    }
    [void][Win32Shot]::EnumWindows($cb, [IntPtr]::Zero)
}

# 2. Курсор в угол и пауза на затухание оверлея
[System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point(4, 4)
Start-Sleep -Milliseconds 700

$proc = Get-Process -Name $ProcessName -ErrorAction Stop | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { throw "Процесс $ProcessName не найден или окно не создано" }

$hwnd = $proc.MainWindowHandle
# Поднимаем окно только если оно не в фокусе: SetForegroundWindow закрывает попапы
if ([Win32Shot]::GetForegroundWindow() -ne $hwnd) {
    [void][Win32Shot]::SetForegroundWindow($hwnd)
    Start-Sleep -Milliseconds 250
}

$rect = New-Object Win32Shot+RECT
$attr = 9  # DWMWA_EXTENDED_FRAME_BOUNDS
$hr = [Win32Shot]::DwmGetWindowAttribute($hwnd, $attr, [ref]$rect, 16)
if ($hr -ne 0 -or ($rect.Right - $rect.Left) -le 0) {
    [void][Win32Shot]::GetWindowRect($hwnd, [ref]$rect)
}

# Объединение со всеми видимыми окнами процесса: попапы (профиль, фильтры)
# выходят за границы главного окна и иначе обрезаются
$procIds = @($proc.Id)
$script:smallW = 0
$script:smallRect = $null
$union = @{ L = $rect.Left; T = $rect.Top; R = $rect.Right; B = $rect.Bottom }
$cb2 = [Win32Shot+EnumProc]{
    param($h, $l)
    $wpid = 0
    [void][Win32Shot]::GetWindowThreadProcessId($h, [ref]$wpid)
    if ($procIds -contains $wpid -and [Win32Shot]::IsWindowVisible($h)) {
        $r = New-Object Win32Shot+RECT
        [void][Win32Shot]::GetWindowRect($h, [ref]$r)
        if (($r.Right - $r.Left) -gt 20) {
            $union.L = [Math]::Min($union.L, $r.Left)
            $union.T = [Math]::Min($union.T, $r.Top)
            $union.R = [Math]::Max($union.R, $r.Right)
            $union.B = [Math]::Max($union.B, $r.Bottom)
            # Окно-попап (уже главного) - для кадра только панели профиля
            $w = $r.Right - $r.Left
            if ($w -lt 600 -and $w -gt $script:smallW) {
                $script:smallW = $w
                $script:smallRect = $r
            }
        }
    }
    return $true
}
[void][Win32Shot]::EnumWindows($cb2, [IntPtr]::Zero)
if ($AllWindows) {
    $rect.Left = $union.L; $rect.Top = $union.T
    $rect.Right = $union.R; $rect.Bottom = $union.B
}
if ($SmallWindow -and $script:smallRect) {
    $rect = $script:smallRect
}

$width = $rect.Right - $rect.Left + 2 * $Pad
$height = $rect.Bottom - $rect.Top + 2 * $Pad
$x = $rect.Left - $Pad
$y = $rect.Top - $Pad

$bmp = New-Object System.Drawing.Bitmap $width, $height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($x, $y, 0, 0, (New-Object System.Drawing.Size $width, $height))
$g.Dispose()

$dir = Split-Path -Parent $Out
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Output ("saved {0} {1}x{2} at {3},{4}" -f $Out, $width, $height, $x, $y)
