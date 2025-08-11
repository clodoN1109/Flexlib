using Terminal.Gui;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Processing;
using Flexlib.Infrastructure.Interop;
using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using Flexlib.Infrastructure.Modelling;

namespace Flexlib.Interface.TUI;


public class TUIApp : ITUIApp
{
    private Window win = new();
    private TextView outputPane = new();
    private FrameView helpFrame = new();
    private TextView helpPane = new();
    private TextField promptPane = new();
    private Label promptLabel = new();
    private Label leftTopLabel = new();
    private Label rightTopLabel = new();
    private Label authLabel = new();
    private Label footerPane = new();
    private ScrollBarView helpScrollBar = new();
    private ScrollBarView outputScrollBar = new();
    private string margin = "     ";

    private readonly TUIConfig _config;
    private readonly Theme _theme;
    private readonly Theme _helpTheme;
    
    private bool IsHelpActive { get; set; } = false;

    public TUIApp(TUIConfig config)
    {
        _config = config;
        _theme = Themes.Get(_config.Theme);
        _helpTheme = config.Theme == "dark" ? Themes.Get("dark-help") : Themes.Get("light-help");
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

        win = new Window("")
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
        leftTopLabel = new Label(logo)
        {
            X = 0,
            Y = 0,
            Width = Dim.Sized(logo.Length),
            Height = 3,
            TextAlignment = TextAlignment.Left,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        leftTopLabel.CanFocus = false;

        string meta = $"{_theme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
        rightTopLabel = new Label(meta)  // pass text here!
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

        win.Add(leftTopLabel, rightTopLabel);

        // Footer Pane
        footerPane = new Label()
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


        // Prompt Pane
        promptLabel = new Label(">")
        {
            X = margin.Length,
            Y = Pos.Top(footerPane) - 1,
            Width = 2,
            Height = 1,
            ColorScheme = scheme
        };
        promptLabel.CanFocus = false;

        promptPane = new TextField("")
        {
            X = Pos.Right(promptLabel),
            Y = Pos.Top(footerPane) - 1,
            Width = Dim.Fill() - user.Id.Length,
            Height = 1,
            ColorScheme = scheme
        };

        // --- Output Pane ---
        int outputScrollbarMargin = 1; // space between text and scrollbar

        outputPane = new TextView()
        {
            X = margin.Length,
            Y = Pos.Bottom(rightTopLabel) + 1,
            Width = Dim.Fill() - (margin.Length + outputScrollbarMargin + 1),
            Height = Dim.Fill() - 6,
            ReadOnly = false,
            ColorScheme = scheme,
            DesiredCursorVisibility = CursorVisibility.Invisible
        };

        outputPane.Enter += (_) => outputPane.ReadOnly = true;
        outputPane.Leave += (_) => outputPane.ReadOnly = false;
        BindScrollToArrowKeys(outputPane);
        win.Add(outputPane);

        // Add vertical scroll bar, positioned after the margin
        outputScrollBar = new ScrollBarView(outputPane, true)
        {
            X = Pos.Right(outputPane) + outputScrollbarMargin
        };

        outputScrollBar.ChangedPosition += () =>
        {
            outputPane.TopRow = outputScrollBar.Position;
            outputPane.SetNeedsDisplay();
        };

        outputPane.DrawContent += (_) =>
        {
            outputScrollBar.Size = outputPane.Lines;
            outputScrollBar.ColorScheme = scheme;
            outputScrollBar.Position = outputPane.TopRow;
            outputScrollBar.Refresh();
        };

        // --- Help Frame & Help Pane ---
        helpFrame = new FrameView()
        {
            X = margin.Length,
            Y = Pos.Top(promptPane),
            Width = Dim.Fill() - margin.Length - 35,
            Height = 0,
            ColorScheme = helpScheme,
            CanFocus = false,
            Visible = false
        };

        // Make the helpPane narrower so there’s a margin before the scrollbar
        int scrollbarMargin = 1; // space between text and scrollbar
        helpPane = new TextView()
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill() - (2 + scrollbarMargin), // space for frame border + margin + scrollbar
            Height = Dim.Fill(),
            ReadOnly = true,
            ColorScheme = helpScheme,
            DesiredCursorVisibility = CursorVisibility.Invisible,
        };
        BindScrollToArrowKeys(helpPane);
        helpPane.Enter += (s) => helpPane.ReadOnly = true;

        helpFrame.Add(helpPane);
        win.Add(helpFrame);

        // Add vertical scroll bar, positioned after the margin
        helpScrollBar = new ScrollBarView(helpPane, true)
        {
            X = Pos.Right(helpPane) + scrollbarMargin
        };

        helpScrollBar.ChangedPosition += () =>
        {
            helpPane.TopRow = helpScrollBar.Position;
            helpPane.SetNeedsDisplay();
        };

        helpPane.DrawContent += (_) =>
        {
            helpScrollBar.Size = helpPane.Lines;
            helpScrollBar.ColorScheme = scheme;
            helpScrollBar.Position = helpPane.TopRow;
            helpScrollBar.Refresh();
        };

        authLabel = new Label($"{user.Id}")
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

                ProcessCommand(input);

                promptPane.Text = "";
                args.Handled = true;
            }

            if (args.KeyEvent.Key == Key.Esc)
            {
                DeactivateHelpFrame();

                promptPane.Text = "";
                args.Handled = true;

                promptPane.SetFocus();
            }
        };

        win.Add(promptLabel, promptPane, authLabel);

        promptPane.SetFocus();

        return top;

    }
    
    static void BindScrollToArrowKeys(TextView textView)
    {
        textView.KeyDown += (args) =>
        {
            int visibleHeight = textView.Bounds.Height;
            int totalLines = textView.Lines;
            int topRow = textView.CurrentRow;

            switch (args.KeyEvent.Key)
            {
                case Key.CursorDown:
                    if (topRow + visibleHeight < totalLines)
                        textView.ScrollTo(topRow + 3);
                    args.Handled = true;
                    break;

                case Key.CursorUp:
                    if (topRow > 0)
                        textView.ScrollTo(topRow - 3);
                    args.Handled = true;
                    break;
            }
        };
    }

    private void ProcessCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        var args = input.ToArrayOfStrings();
        string command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "login":
            case "signup":
            case "logout":
                outputPane.Text = "Leave the TUI to perform authentication operations.";
                return;

            case "gui":
            case "tui":
                outputPane.Text = "Cannot create a nested interface. \nUse the CLI commands or available shortcuts instead.";
                return;
        }

        string outputStream = RunFlexlib(args, outputPane);

        Theme newGeneralTheme;
        Theme newHelpTheme;
        switch (command)
        {
            case "exit":
                ExitTUI();
                return;

            case "help":
                ActivateHelpFrame(outputStream);
                return;

            case "cls":
            case "clear":
                outputPane.Text = "";
                helpPane.Text   = "";
                DeactivateHelpFrame();
                break;

            case "dark":
                newGeneralTheme = Themes.Get("dark");
                newHelpTheme    = Themes.Get("dark-help");
                UpdateTheme(newGeneralTheme, newHelpTheme);
                break;

            case "light":
                newGeneralTheme = Themes.Get("light");
                newHelpTheme    = Themes.Get("light-help");
                UpdateTheme(newGeneralTheme, newHelpTheme);
                break;

            default:
                if (args.Length > 1 && args[1].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    ActivateHelpFrame(outputStream);
                    return;
                }
                DeactivateHelpFrame();
                outputPane.Text = outputStream;
                break;
        }
    }

    private void UpdateTheme(Theme generalTheme, Theme helpTheme)
    {
        win.ColorScheme = generalTheme.ToColorScheme();
        leftTopLabel.ColorScheme = generalTheme.ToColorScheme();
        rightTopLabel.ColorScheme = generalTheme.ToColorScheme();
        outputPane.ColorScheme = generalTheme.ToColorScheme();
        helpFrame.ColorScheme = helpTheme.ToColorScheme();
        helpPane.ColorScheme = helpTheme.ToColorScheme();
        promptPane.ColorScheme = generalTheme.ToColorScheme();
        promptLabel.ColorScheme = generalTheme.ToColorScheme();
        footerPane.ColorScheme = generalTheme.ToColorScheme();
        authLabel.ColorScheme = generalTheme.ToColorScheme();

        outputPane.DrawContent += (_) =>
        {
            outputScrollBar.Size = outputPane.Lines;
            outputScrollBar.ColorScheme = generalTheme.ToColorScheme();
            outputScrollBar.Position = outputPane.TopRow;
            outputScrollBar.Refresh();
        };
        helpPane.DrawContent += (_) =>
        {
            helpScrollBar.Size = helpPane.Lines;
            helpScrollBar.ColorScheme = generalTheme.ToColorScheme();
            helpScrollBar.Position = helpPane.TopRow;
            helpScrollBar.Refresh();
        };
        
        string meta = $"{generalTheme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
        rightTopLabel.Text = meta; 
    }

    private void ActivateHelpFrame(string outputStream)
    {
        int rows = outputStream.RowCount();

        // Clamp rows to max 15
        int visibleRows = Math.Min(rows, 15);

        helpFrame.Visible = true;
        helpPane.Text = outputStream; // Set text on the TextView
        helpFrame.Height = visibleRows + 2; // Border + padding
        helpFrame.Y = Pos.Top(promptPane) - (visibleRows + 2);

        helpFrame.CanFocus = true;
        Terminal.Gui.Application.Refresh();

        promptLabel.Text = "˄";

        IsHelpActive = true;
        helpPane.SetFocus();
    }


    private void DeactivateHelpFrame()
    {
        helpFrame.Visible = false;
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

