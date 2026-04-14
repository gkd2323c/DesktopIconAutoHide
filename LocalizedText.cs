using System.Globalization;

namespace DesktopIconAutoHide;

internal enum AppLanguageMode
{
    Auto = 0,
    Chinese = 1,
    English = 2
}

internal static class LocalizedText
{
    private static AppLanguageMode _languageMode = AppLanguageMode.Auto;

    internal static AppLanguageMode LanguageMode => _languageMode;

    internal static void SetLanguageMode(AppLanguageMode mode) =>
        _languageMode = mode;

    private static AppLanguageMode EffectiveLanguageMode
    {
        get
        {
            if (_languageMode == AppLanguageMode.Auto)
            {
                var isSystemChinese = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                    .Equals("zh", StringComparison.OrdinalIgnoreCase);
                return isSystemChinese ? AppLanguageMode.Chinese : AppLanguageMode.English;
            }

            return _languageMode;
        }
    }

    private static bool IsChinese =>
        EffectiveLanguageMode == AppLanguageMode.Chinese;

    private static string Pick(string chinese, string english) =>
        IsChinese ? chinese : english;

    internal static string BuildIdleTooltip(int seconds) =>
        seconds <= 0
            ? AutoHideDisabledTooltip
            : (IsChinese
                ? $"\u684c\u9762\u56fe\u6807\u81ea\u52a8\u9690\u85cf\uff08{seconds} \u79d2\uff09"
                : $"Desktop icons auto-hide ({seconds}s)");

    internal static string AutoHideDisabledTooltip =>
        Pick(
            "\u81ea\u52a8\u9690\u85cf\u5df2\u5173\u95ed",
            "Auto-hide disabled");

    internal static string HiddenTooltip =>
        Pick(
            "\u684c\u9762\u56fe\u6807\u5df2\u9690\u85cf\uff0c\u79fb\u52a8\u9f20\u6807\u5373\u53ef\u6062\u590d",
            "Desktop icons hidden. Move mouse to restore.");

    internal static string MenuShowNow =>
        Pick("\u7acb\u5373\u663e\u793a\u56fe\u6807", "Show icons now");

    internal static string MenuHideNow =>
        Pick("\u7acb\u5373\u9690\u85cf\u56fe\u6807", "Hide icons now");

    internal static string MenuSettings =>
        Pick("\u8bbe\u7f6e...", "Settings...");

    internal static string MenuExit =>
        Pick("\u9000\u51fa", "Exit");

    internal static string SettingsTitle =>
        Pick("\u81ea\u52a8\u9690\u85cf\u8bbe\u7f6e", "Auto-hide Settings");

    internal static string SettingsIdleLabel =>
        Pick(
            "\u9f20\u6807\u9759\u6b62\u540e\u81ea\u52a8\u9690\u85cf\u684c\u9762\u56fe\u6807\uff08\u79d2\uff0c0=\u5173\u95ed\uff09\uff1a",
            "Hide desktop icons after idle (seconds, 0=off):");

    internal static string SettingsLanguageLabel =>
        Pick("\u754c\u9762\u8bed\u8a00\uff1a", "Interface language:");

    internal static string SettingsAutoStartLabel =>
        Pick(
            "\u5f00\u673a\u542f\u52a8\uff08\u767b\u5f55 Windows \u540e\u81ea\u52a8\u8fd0\u884c\uff09",
            "Start with Windows (after sign in)");

    internal static string LanguageOptionAuto =>
        Pick("\u8ddf\u968f\u7cfb\u7edf", "System default");

    internal static string LanguageOptionChinese =>
        Pick("\u4e2d\u6587", "Chinese");

    internal static string LanguageOptionEnglish =>
        Pick("\u82f1\u6587", "English");

    internal static string SettingsSave =>
        Pick("\u4fdd\u5b58", "Save");

    internal static string SettingsCancel =>
        Pick("\u53d6\u6d88", "Cancel");
}
