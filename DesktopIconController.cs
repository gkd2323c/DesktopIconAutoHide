using System.Runtime.InteropServices;
using System.Text;

namespace DesktopIconAutoHide;

internal static class DesktopIconController
{
    private const int SwHide = 0;
    private const int SwShow = 5;
    private const uint GaRoot = 2;

    private const string ProgmanClass = "Progman";
    private const string WorkerWClass = "WorkerW";
    private const string ShellDefViewClass = "SHELLDLL_DefView";
    private const string ListViewClass = "SysListView32";
    private const string FolderViewName = "FolderView";

    internal static bool SetDesktopIconsVisible(bool visible)
    {
        var listView = FindDesktopListViewHandle();
        if (listView == IntPtr.Zero)
        {
            return false;
        }

        ShowWindow(listView, visible ? SwShow : SwHide);
        return true;
    }

    internal static bool? AreDesktopIconsVisible()
    {
        var listView = FindDesktopListViewHandle();
        if (listView == IntPtr.Zero)
        {
            return null;
        }

        return IsWindowVisible(listView);
    }

    internal static bool IsDesktopForeground()
    {
        var foreground = GetForegroundWindow();
        if (foreground == IntPtr.Zero)
        {
            return false;
        }

        var shellWindow = GetShellWindow();
        if (foreground == shellWindow)
        {
            return true;
        }

        var root = GetAncestor(foreground, GaRoot);
        if (root == IntPtr.Zero)
        {
            root = foreground;
        }

        var rootClass = GetWindowClass(root);
        if (IsDesktopContainerClass(rootClass))
        {
            return true;
        }

        var defView = FindDesktopDefViewHandle();
        if (defView != IntPtr.Zero)
        {
            if (foreground == defView || IsChild(defView, foreground))
            {
                return true;
            }

            var listView = FindWindowEx(defView, IntPtr.Zero, ListViewClass, FolderViewName);
            if (listView != IntPtr.Zero && (foreground == listView || IsChild(listView, foreground)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDesktopContainerClass(string className) =>
        className.Equals(ProgmanClass, StringComparison.Ordinal) ||
        className.Equals(WorkerWClass, StringComparison.Ordinal);

    private static IntPtr FindDesktopListViewHandle()
    {
        var defView = FindDesktopDefViewHandle();
        if (defView == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        return FindWindowEx(defView, IntPtr.Zero, ListViewClass, FolderViewName);
    }

    private static IntPtr FindDesktopDefViewHandle()
    {
        var progman = FindWindow(ProgmanClass, null);
        if (progman != IntPtr.Zero)
        {
            var defViewInProgman = FindWindowEx(progman, IntPtr.Zero, ShellDefViewClass, null);
            if (defViewInProgman != IntPtr.Zero)
            {
                return defViewInProgman;
            }
        }

        var foundDefView = IntPtr.Zero;
        EnumWindows((topLevelWindow, _) =>
        {
            var defView = FindWindowEx(topLevelWindow, IntPtr.Zero, ShellDefViewClass, null);
            if (defView == IntPtr.Zero)
            {
                return true;
            }

            foundDefView = defView;
            return false;
        }, IntPtr.Zero);

        return foundDefView;
    }

    private static string GetWindowClass(IntPtr windowHandle)
    {
        var builder = new StringBuilder(256);
        _ = GetClassName(windowHandle, builder, builder.Capacity);
        return builder.ToString();
    }

    private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
}
