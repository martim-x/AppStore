param([string]$ProcessName = "Project", [switch]$CloseSmall)

Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class WinEnum {
    public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr lp);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder sb, int max);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr w, IntPtr l);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
}
"@

$target = (Get-Process -Name $ProcessName -ErrorAction Stop).Id
$script:rows = @()
$cb = [WinEnum+EnumProc]{
    param($h, $l)
    $pid2 = 0
    [void][WinEnum]::GetWindowThreadProcessId($h, [ref]$pid2)
    if ($pid2 -eq $target -and [WinEnum]::IsWindowVisible($h)) {
        $len = [WinEnum]::GetWindowTextLength($h)
        $sb = New-Object System.Text.StringBuilder ($len + 1)
        [void][WinEnum]::GetWindowText($h, $sb, $sb.Capacity)
        $r = New-Object WinEnum+RECT
        [void][WinEnum]::GetWindowRect($h, [ref]$r)
        $w = $r.Right - $r.Left; $hgt = $r.Bottom - $r.Top
        $script:rows += [pscustomobject]@{ hwnd = $h; title = $sb.ToString(); w = $w; h = $hgt; x = $r.Left; y = $r.Top }
        if ($CloseSmall -and $w -lt 600) {
            [void][WinEnum]::SendMessage($h, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero)  # WM_CLOSE
        }
    }
    return $true
}
[void][WinEnum]::EnumWindows($cb, [IntPtr]::Zero)
$script:rows | Format-Table -AutoSize
