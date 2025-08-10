using Terminal.Gui;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Processing;
using Flexlib.Infrastructure.Interop;
using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;

namespace Flexlib.Interface.TUI;


public class TUIApp : ITUIApp
{
    private readonly TUIConfig _config;
    private readonly Theme _theme;
    private readonly Theme _helpTheme;
    
    private bool IsHelpActive { get; set; } = false;

    public TUIApp(TUIConfig config)
    {
        _config = config;
        _theme = Themes.Get(config.Theme);
        _helpTheme = Themes.Get("help");
    }

    public void Run(IUser user)
    {
        try
        {
            Terminal.Gui.Application.Init();
            var tui = RenderTUI(user);
            Terminal.Gui.Application.Run();    
        }
        finally
        {
            Terminal.Gui.Application.Shutdown();
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.Clear();
        }     
    }

    private Terminal.Gui.Toplevel RenderTUI(IUser user)
    {
        var top = Terminal.Gui.Application.Top;

        // Create a shared color scheme based on _theme
        var scheme = _theme.ToColorScheme();
        var helpScheme = _helpTheme.ToColorScheme();

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

        string logo = $"{margin}>::>  flexlib";  // Removed leading spaces to avoid confusion
        var lefTopLabel = new Label(logo)
        {
            X = 0,
            Y = 0,
            Width = Dim.Sized(logo.Length),
            Height = 3,
            TextAlignment = TextAlignment.Left,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        lefTopLabel.CanFocus = false;

        string meta = $"{_theme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
        var rightTopLabel = new Label(meta)  // pass text here!
        {
            X = Pos.AnchorEnd(meta.Length),
            Y = 0,
            Width = Dim.Sized(meta.Length),
            Height = 3,
            TextAlignment = TextAlignment.Right,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        rightTopLabel.CanFocus = false;

        win.Add(lefTopLabel, rightTopLabel);

        var outputPane = new TextView()
        {
            X = margin.Length,
            Y = Pos.Bottom(rightTopLabel),
            Width = Dim.Fill() - margin.Length,
            Height = Dim.Percent(60),
            ReadOnly = false,
            ColorScheme = scheme
        };
        outputPane.CanFocus = false;
        win.Add(outputPane);

        var footerPane = new Label()
        {
            X = margin.Length,
            Y = Pos.AnchorEnd(3),
            Width = Dim.Fill(),
            Height = 3,
            TextAlignment = TextAlignment.Left,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        footerPane.CanFocus = false;
        win.Add(footerPane);

        var promptLabel = new Label(">")
        {
            X = margin.Length,
            Y = Pos.Top(footerPane) - 1,   
            Width = 2,
            Height = 1,
            ColorScheme = scheme
        };
        promptLabel.CanFocus = false;

        var promptPane = new TextField("")
        {
            X = Pos.Right(promptLabel),
            Y = Pos.Top(footerPane) - 1,
            Width = Dim.Fill() - user.Id.Length,
            Height = 1,
            ColorScheme = scheme
        };

        // Creation
        var helpFrame = new FrameView()
        {
            X = margin.Length,
            Y = Pos.Top(promptPane),
            Width = Dim.Fill() - margin.Length,
            Height = 0,
            ColorScheme = helpScheme,
            CanFocus = false
        };

        var helpPane = new TextView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            ColorScheme = helpScheme
        };
        helpPane.Enter += (s) => helpPane.ReadOnly = true;

        helpFrame.Add(helpPane);
        win.Add(helpFrame);

        var authLabel = new Label($"{user.Id}")
        {
            X = Pos.AnchorEnd(user.Id.Length) - margin.Length,
            Y = Pos.Top(footerPane) - 1,
            Width = 2,
            Height = 1,
            ColorScheme = scheme
        };
    
        promptPane.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                string input = promptPane.Text?.ToString() ?? "";

                ProcessCommand(input, outputPane, helpFrame, helpPane, promptLabel, promptPane);

                promptPane.Text = "";
                args.Handled = true;
            }

            if (args.KeyEvent.Key == Key.Esc)
            {
                DeactivateHelpFrame(helpFrame, promptLabel);

                promptPane.Text = "";
                args.Handled = true;

                promptPane.SetFocus();
            }
        };

        win.Add(promptLabel, promptPane, authLabel);
        
        promptPane.SetFocus();

        return top;
    }

    private void ProcessCommand(string input, TextView outputPane, FrameView helpFrame, TextView helpPane, Label promptLabel, TextField promptPane)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        var args = input.ToArrayOfStrings();

        string outputStream = RunFlexlib(args, outputPane);

        if (args[0].Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            ExitTUI();
            return;
        }

        if (args[0].Equals("help", StringComparison.OrdinalIgnoreCase) ||
            (args.Length > 1 && args[1].Equals("help", StringComparison.OrdinalIgnoreCase)))
        {
            ActivateHelpFrame(helpFrame, helpPane, promptPane, promptLabel, outputStream);
            return;
        }

        DeactivateHelpFrame(helpFrame, promptLabel);
        outputPane.Text = outputStream;
    }
    private void ActivateHelpFrame(FrameView helpFrame, TextView helpPane, TextField promptPane, Label promptLabel, string outputStream)
    {
        int rows = outputStream.RowCount();

        // Clamp rows to max 15
        int visibleRows = Math.Min(rows, 15);

        helpPane.Text = outputStream; // Set text on the TextView
        helpFrame.Height = visibleRows + 2; // Border + padding
        helpFrame.Y = Pos.Top(promptPane) - (visibleRows + 2);

        helpFrame.CanFocus = true;
        Terminal.Gui.Application.Refresh();

        promptLabel.Text = "˄";

        IsHelpActive = true;
        helpPane.SetFocus();
    }


    private void DeactivateHelpFrame(FrameView helpFrame, Label promptLabel)
    {
        helpFrame.Height = 0;
        helpFrame.CanFocus = false;
        Terminal.Gui.Application.Refresh();

        promptLabel.Text = ">";

        IsHelpActive = false;
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

    private void ExitTUI()
    {
        Terminal.Gui.Application.RequestStop();
    }
}

