using Microsoft.Win32;

namespace DesktopIconAutoHide;

internal static class AutoStartManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "DesktopIconAutoHide";

    internal static bool IsEnabled()
    {
        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            return runKey?.GetValue(RunValueName) is string value && !string.IsNullOrWhiteSpace(value);
        }
        catch
        {
            return false;
        }
    }

    internal static bool TrySetEnabled(bool enabled)
    {
        try
        {
            using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (runKey is null)
            {
                return false;
            }

            if (enabled)
            {
                var executablePath = Application.ExecutablePath;
                runKey.SetValue(RunValueName, Quote(executablePath), RegistryValueKind.String);
                return true;
            }

            runKey.DeleteValue(RunValueName, throwOnMissingValue: false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string Quote(string path) => $"\"{path}\"";
}
