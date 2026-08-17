using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Sprout.Core.Services.WindowSize;

public static class WindowScreenSizer
{
    public static void SizeToScreen(this Window window, double widthFactor = 0.85, double heightFactor = 0.85)
    {
        var cursorPos = Cursor.Position;
        var screen = Screen.FromPoint(cursorPos);
        var (scaleX, scaleY) = GetDpiScale(cursorPos);

        window.Width = screen.WorkingArea.Width * widthFactor / scaleX;
        window.Height = screen.WorkingArea.Height * heightFactor / scaleY;
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// This helps calculate properly when the screen has scaling
    /// </summary>
    private static (double X, double Y) GetDpiScale(System.Drawing.Point point)
    {
        var hMonitor = MonitorFromPoint(new POINT { X = point.X, Y = point.Y }, MONITOR_DEFAULTTONEAREST);

        if (GetDpiForMonitor(hMonitor, 0 /* MDT_Effective_DPI */, out uint dpiX, out uint dpiY) == 0)
            return (dpiX / 96.0, dpiY / 96.0);

        return (1.0, 1.0); // fallback for pre-8.1 systems
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("Shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    private const uint MONITOR_DEFAULTTONEAREST = 2;
}