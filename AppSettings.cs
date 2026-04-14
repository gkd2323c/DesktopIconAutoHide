using System.Text.Json;

namespace DesktopIconAutoHide;

internal readonly record struct AppSettings(int IdleSeconds, AppLanguageMode LanguageMode);

internal static class AppSettingsStore
{
    internal const int DefaultIdleSeconds = 8;
    internal const int MinIdleSeconds = 1;
    internal const int MaxIdleSeconds = 3600;
    internal const AppLanguageMode DefaultLanguageMode = AppLanguageMode.Auto;

    private static readonly string SettingsPath = Path.Combine(
        AppContext.BaseDirectory,
        "settings.json");

    internal static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings(DefaultIdleSeconds, DefaultLanguageMode);
            }

            var json = File.ReadAllText(SettingsPath);
            var model = JsonSerializer.Deserialize<SettingsFileModel>(json);
            var idleSeconds = ClampIdleSeconds(model?.IdleSeconds ?? DefaultIdleSeconds);
            var languageMode = ParseLanguageMode(model?.LanguageMode);
            return new AppSettings(idleSeconds, languageMode);
        }
        catch
        {
            return new AppSettings(DefaultIdleSeconds, DefaultLanguageMode);
        }
    }

    internal static void Save(AppSettings settings)
    {
        var idleSeconds = ClampIdleSeconds(settings.IdleSeconds);
        var model = new SettingsFileModel
        {
            IdleSeconds = idleSeconds,
            LanguageMode = ToStorageLanguageMode(settings.LanguageMode)
        };

        var json = JsonSerializer.Serialize(model, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(SettingsPath, json);
    }

    private static int ClampIdleSeconds(int value) =>
        Math.Clamp(value, MinIdleSeconds, MaxIdleSeconds);

    private static AppLanguageMode ParseLanguageMode(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return DefaultLanguageMode;
        }

        return rawValue.Trim().ToLowerInvariant() switch
        {
            "auto" => AppLanguageMode.Auto,
            "zh" => AppLanguageMode.Chinese,
            "zh-cn" => AppLanguageMode.Chinese,
            "chinese" => AppLanguageMode.Chinese,
            "en" => AppLanguageMode.English,
            "en-us" => AppLanguageMode.English,
            "english" => AppLanguageMode.English,
            _ => DefaultLanguageMode
        };
    }

    private static string ToStorageLanguageMode(AppLanguageMode mode) =>
        mode switch
        {
            AppLanguageMode.Chinese => "zh",
            AppLanguageMode.English => "en",
            _ => "auto"
        };

    private sealed class SettingsFileModel
    {
        public int IdleSeconds { get; set; } = DefaultIdleSeconds;
        public string LanguageMode { get; set; } = "auto";
    }
}
