using Flexlib.Infrastructure.Environment;
using System.Text.Json;

namespace Flexlib.Infrastructure.Config;


public class TUIConfig
{
    private static readonly string? ConfigFilePath = GetConfigFilePath();

    public string Theme { get; set; } = "dark";
    public string Language { get; set; } = "en";

    public TUIConfig(string? theme, string? language)
    {
        if (ConfigFilePath != null && !File.Exists(ConfigFilePath))
            CreateTUIConfigFile();

        var configFromFile = ReadTUIConfigFile();

        Theme = theme ?? configFromFile?.Theme ?? "dark";
        Language = language ?? configFromFile?.Language ?? "en";
    }

    public TUIConfig() { }

    private static string? GetConfigFilePath()
    {
        var location = Env.GetExecutingAssemblyLocation();
        return location != null
            ? Path.Combine(location, "config", "TUIConfig.json")
            : null;
    }

    private TUIConfig? ReadTUIConfigFile()
    {
        try
        {
            if (ConfigFilePath == null) return null;
            var json = File.ReadAllText(ConfigFilePath);
            return JsonSerializer.Deserialize<TUIConfig>(json);
        }
        catch
        {
            return null;
        }
    }

    private void CreateTUIConfigFile()
    {
        if (ConfigFilePath == null) return;

        var defaultConfig = new TUIConfig
        {
            Theme = "dark",
            Language = "en"
        };

        var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var directory = Path.GetDirectoryName(ConfigFilePath);
        if (directory != null)
            Directory.CreateDirectory(directory);

        File.WriteAllText(ConfigFilePath, json);
    }
}

