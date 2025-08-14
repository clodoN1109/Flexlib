using Terminal.Gui;

namespace Flexlib.Interface.TUI;


public class Theme(string name)
{
    public string Name { get; } = name;
    public Color Background { get; set; }
    public Color Foreground { get; set; }
    public Color Accent { get; set; }
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
        ["basic"] = new Theme("basic")
        {
            Background = Color.Gray,
            Foreground = Color.White,
            Accent = Color.BrightYellow,
            Icon = "-"
        },
        ["light"] = new Theme("light")
        {
            Background = Color.Gray,
            Foreground = Color.BrightRed,
            Accent = Color.Black,
            Icon = "☀"
        },
        ["dark"] = new Theme("dark")
        {
            Background = Color.Black,
            Foreground = Color.DarkGray,
            Accent = Color.Gray,
            Icon = "☽"
        },
        ["dark-help"] = new Theme("dark-help")
        {
            Background = Color.Black,
            Foreground = Color.DarkGray,
            Accent = Color.DarkGray,
            Icon = "☽"
        },
        ["light-help"] = new Theme("light-help")
        {
            Background = Color.DarkGray,
            Foreground = Color.Red,
            Accent = Color.Black,
            Icon = "☀"
        },
        ["selected-dark-frame"] = new Theme("selected-dark-frame")
        {
            Background = Color.Black,
            Foreground = Color.Cyan,
            Accent = Color.Cyan,
            Icon = "☽"
        },
        ["selected-light-frame"] = new Theme("selected-light-frame")
        {
            Background = Color.Red,
            Foreground = Color.DarkGray,
            Accent = Color.DarkGray,
            Icon = "☀"
        },
        ["success-dark-frame"] = new Theme("success-dark-frame")
        {
            Background = Color.Black,
            Foreground = Color.BrightGreen,
            Accent = Color.Green,
            Icon = "☽"
        },
        ["error-dark-frame"] = new Theme("error-dark-frame")
        {
            Background = Color.Black,
            Foreground = Color.BrightRed,
            Accent = Color.Red,
            Icon = "☽"
        },
        ["warning-dark-frame"] = new Theme("warning-dark-frame")
        {
            Background = Color.Black,
            Foreground = Color.BrightYellow,
            Accent = Color.Red,
            Icon = "☽"
        },
        ["success-light-frame"] = new Theme("success-light-frame")
        {
            Background = Color.DarkGray,
            Foreground = Color.BrightGreen,
            Accent = Color.Red,
            Icon = "☀"
        },
        ["error-light-frame"] = new Theme("error-light-frame")
        {
            Background = Color.DarkGray,
            Foreground = Color.BrightRed,
            Accent = Color.Red,
            Icon = "☀"
        },
        ["warning-light-frame"] = new Theme("warning-light-frame")
        {
            Background = Color.DarkGray,
            Foreground = Color.BrightYellow,
            Accent = Color.Red,
            Icon = "☀"
        },


    };

    public static Theme Get(string name)
    {
        if (All.TryGetValue(name, out var theme))
            return theme;
        return All["dark"]; // default
    }
}

