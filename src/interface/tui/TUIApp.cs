using Terminal.Gui;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Processing;
using Flexlib.Infrastructure.Interop;
using System.Diagnostics;

namespace Flexlib.Interface.TUI;


public class TUIApp : ITUIApp
{
    private readonly TUIConfig _config;
    private readonly Theme _theme;

    public TUIApp(TUIConfig config)
    {
        _config = config;
        _theme = Themes.Get(config.Theme);
    }

    public void Run(IUser user)
    {
        Terminal.Gui.Application.Init();
        var tui = RenderTUI(user);
        Terminal.Gui.Application.Run();
    }

    private Terminal.Gui.Toplevel RenderTUI(IUser user)
    {
        var top = Terminal.Gui.Application.Top;

        // Create a shared color scheme based on _theme
        var scheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(_theme.Foreground, _theme.Background),
            Focus = new Terminal.Gui.Attribute(_theme.Accent, _theme.Background),
            HotNormal = new Terminal.Gui.Attribute(_theme.Accent, _theme.Background),
            HotFocus = new Terminal.Gui.Attribute(_theme.Background, _theme.Accent)
        };

        string margin = "     ";

        var win = new Window("")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = scheme,
            Border = new Border() { BorderStyle = BorderStyle.None }
        };
        top.Add(win);

        string logo = $"{margin}>::> flexlib";  // Removed leading spaces to avoid confusion
        var leftLabel = new Label(logo)
        {
            X = 0,
            Y = 0,
            Width = Dim.Sized(logo.Length),
            Height = 3,
            TextAlignment = TextAlignment.Left,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };

        string meta = $"{_theme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
        var rightLabel = new Label(meta)  // pass text here!
        {
            X = Pos.AnchorEnd(meta.Length),
            Y = 0,
            Width = Dim.Sized(meta.Length),
            Height = 3,
            TextAlignment = TextAlignment.Right,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };

        win.Add(leftLabel, rightLabel);

        var outputPane = new TextView()
        {
            X = margin.Length,
            Y = Pos.Bottom(rightLabel),
            Width = Dim.Fill(),
            Height = Dim.Percent(60),
            ReadOnly = true,
            ColorScheme = scheme
        };
        win.Add(outputPane);

        var helpPane = new TextView()
        {
            X = margin.Length,
            Y = Pos.Bottom(outputPane),
            Width = Dim.Fill(),
            Height = Dim.Percent(30),
            ReadOnly = true,
            ColorScheme = scheme
        };
        win.Add(helpPane);

        var footerPane = new Label($"{user.Id}")
        {
            X = margin.Length,
            Y = Pos.AnchorEnd(3),
            Width = Dim.Fill(),
            Height = 3,
            TextAlignment = TextAlignment.Left,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        win.Add(footerPane);

        var promptLabel = new Label(">")
        {
            X = margin.Length,
            Y = Pos.Top(footerPane) - 1,   
            Width = 2,
            Height = 1,
            ColorScheme = scheme
        };

        var promptPane = new TextField("")
        {
            X = Pos.Right(promptLabel),
            Y = Pos.Top(footerPane) - 1,
            Width = Dim.Fill() - 2,
            Height = 1,
            ColorScheme = scheme
        };

        win.Add(promptLabel, promptPane);


        promptPane.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                string input = promptPane.Text.ToString() ?? "";
                ProcessCommand(input, outputPane, helpPane);
                promptPane.Text = "";
                args.Handled = true;
            }
        };

        promptPane.SetFocus();

        return top;
    }

    private void ProcessCommand(string input, TextView outputPane, TextView helpPane)
    {
        if (string.IsNullOrWhiteSpace(input)) 
            return;

        var args = input.ToArrayOfStrings();

        string outputStream = RunFlexlib(args, outputPane);

        if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            helpPane.Text = outputStream;
            return;
        }

        outputPane.Text = outputStream;
    }

    private string RunFlexlib(string[] args, TextView outputPane)
    {
        string flexlibExe = Path.Combine(AppContext.BaseDirectory, "Flexlib.exe");

        // Determine a virtual console width for CLI rendering
        int tuiWidth = outputPane.Bounds.Width; // or some other measurement
        var allArgs = args.Concat(new[] { $"--width={tuiWidth}" });

        var psi = new ProcessStartInfo
        {
            FileName = flexlibExe,
            Arguments = string.Join(" ", allArgs),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new System.Diagnostics.Process { StartInfo = psi };
        process.Start();

        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return string.IsNullOrEmpty(stderr) ? stdout : stdout + Environment.NewLine + stderr;
    }


}

