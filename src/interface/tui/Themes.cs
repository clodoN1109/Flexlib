using Terminal.Gui;

namespace Flexlib.Interface.TUI;


public class Theme
{
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
        ["light"] = new Theme
        {
            Background = Color.White,
            Foreground = Color.Black,
            Accent     = Color.Blue,
            HelpText   = Color.DarkGray,
            Icon       = "🞻" 
        },
        ["dark"] = new Theme
        {
            Background = Color.Black,
            Foreground = Color.White,
            Accent     = Color.Cyan,
            HelpText   = Color.Gray,
            Icon       = "☽" 
        }
    };

    public static Theme Get(string name)
    {
        if (All.TryGetValue(name, out var theme))
            return theme;
        return All["dark"]; // default
    }
}

