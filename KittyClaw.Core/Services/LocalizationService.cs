using System.Reflection;
using System.Text.Json;

namespace KittyClaw.Core.Services;

public class LocalizationService
{
    private readonly AppSettingsService _settings;
    private readonly Dictionary<string, Dictionary<string, string>> _cache = [];

    public LocalizationService(AppSettingsService settings)
    {
        _settings = settings;
        _settings.OnLanguageChanged += () => OnLanguageChanged?.Invoke();
        Load();
    }

    public string Lang => _settings.Language;
    public event Action? OnLanguageChanged;

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        var lang = Lang;
        if (_cache.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var value))
            return value;
        return key;
    }

    public string Get(string key, params object[] args) => string.Format(Get(key), args);

    private void Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var prefix = "KittyClaw.Core.Localization.";

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(prefix) || !resourceName.EndsWith(".json"))
                continue;

            // e.g. KittyClaw.Core.Localization.Board.fr.json → lang = "fr"
            var fileName = resourceName[prefix.Length..]; // e.g. Board.fr.json
            var parts = fileName.Split('.');
            if (parts.Length < 3) continue;
            var lang = parts[^2]; // second-to-last part

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;

            if (!_cache.TryGetValue(lang, out var dict))
            {
                dict = [];
                _cache[lang] = dict;
            }

            foreach (var (k, v) in entries)
                dict[k] = v;
        }
    }
}
