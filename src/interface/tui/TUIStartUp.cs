using Flexlib.Interface;
using Flexlib.Interface.Input;
using Flexlib.Infrastructure.Modelling;
using System.Text;
using Flexlib.Infrastructure.Environment;

namespace Flexlib.Interface.TUI;

    
public class TUIStartUp : ParsedInput
{
    public string? Theme { get; }
    public string? Language { get; }
    public string[] Config { get; }

    public TUIStartUp(string[] tuiConfig)
    {
        Config = tuiConfig;
        Theme = Config.Length > 0 ? Config[0] : null;
        Language = Config.Length > 1 ? Config[1] : null;
    }

    public override bool IsValid()
    {
        return Config.Length >= 0 && Config.Length <= 2;
    }

    public bool IsHelp()
    {
        return Config.Length > 0 ? Config[0] == "help" : false;
    }
    
    public UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string>(),
            Title = "tui",
            Description = "Launches the text user interface for Flexlib.",
            Syntax = "flexlib tui [theme] [language]",
            Options = new List<Option>
            {
                new Option
                {
                    Name = "theme",
                    Description = "Selects the color palette.",
                    OptionDomain = new VariableDomain("dark", "light"),
                    DefaultValue = "dark"
                },
                new Option
                {
                    Name = "language",
                    Description = "Selects the language used to present UI text.",
                    OptionDomain = new VariableDomain("en", "pt"),
                    DefaultValue = "en"
                }
            }
        };
    }

    public override string ToString()
    {
        var info = GetUsageInfo(); // ← safely pull from your method
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine($"ACTION: {info.Title.ToUpperInvariant()}");
        sb.AppendLine(new string('-', Env.GetSafeWindowWidth()));
        sb.AppendLine($"Description : {info.Description}");
        sb.AppendLine($"Syntax      : {info.Syntax}");
        sb.AppendLine();

        if (info.Options is { Count: > 0 })
        {
            sb.AppendLine("Options:");
            foreach (var opt in info.Options)
            {
                var domain = opt.OptionDomain != null
                    ? $" (choices: {string.Join(", ", opt.OptionDomain.IncludedValues)})"
                    : string.Empty;

                var defaultValue = !string.IsNullOrWhiteSpace(opt.DefaultValue)
                    ? $" [default: {opt.DefaultValue}]"
                    : string.Empty;

                sb.AppendLine($"  --{opt.Name} : {opt.Description}{domain}{defaultValue}");
            }
        }

        if (info.Meta is { Count: > 0 })
        {
            sb.AppendLine();
            sb.AppendLine("Meta:");
            foreach (var meta in info.Meta)
                sb.AppendLine($"  - {meta}");
        }

        return sb.ToString();
    }


}
