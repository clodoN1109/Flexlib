using Terminal.Gui;

namespace Flexlib.Interface.TUI;


public class Theme(string name)
{
    public string Name { get; } = name;
    public Color Background { get; set; }
    public Color Foreground { get; set; }
    public Color Accent { get; set; }
    public Color HelpText { get; set; }
    public string Icon { get; set; } = "";

    public ColorScheme ToColorScheme()
    {
        return new ColorScheme
        {
            Normal = Terminal.Gui.Application.Driver.MakeAttribute(Foreground, Background),
            Focus = Terminal.Gui.Application.Driver.MakeAttribute(Accent, Background),
            HotNormal = Terminal.Gui.Application.Driver.MakeAttribute(Accent, Background),
            HotFocus = Terminal.Gui.Application.Driver.MakeAttribute(Foreground, Accent)
        };
    }
}

public static class Themes
{
    public static readonly Dictionary<string, Theme> All = new(StringComparer.OrdinalIgnoreCase)
    {
        ["light"] = new Theme("light")
        {
            Background = Color.DarkGray,
            Foreground = Color.Red,
            Accent = Color.Black,
            HelpText = Color.Magenta,
            Icon = "☀"
        },
        ["dark"] = new Theme("dark")
        {
            Background = Color.Black,
            Foreground = Color.DarkGray,
            Accent = Color.Gray,
            HelpText = Color.Gray,
            Icon = "☽"
        },
        ["dark-help"] = new Theme("dark-help")
        {
            Background = Color.Black,
            Foreground = Color.Red,
            Accent = Color.BrightYellow,
            HelpText = Color.Red,
            Icon = "☽"
        },
        ["light-help"] = new Theme("light-help")
        {
            Background = Color.Blue,
            Foreground = Color.Red,
            Accent     = Color.BrightYellow,
            HelpText   = Color.Red,
            Icon       = "☀" 
        }

    };

    public static Theme Get(string name)
    {
        if (All.TryGetValue(name, out var theme))
            return theme;
        return All["dark"]; // default
    }
}

