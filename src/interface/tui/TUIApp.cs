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


public partial class TUIApp : ITUIApp
{
    private Window win = new();
    private TextView outputPane = new();
    private FrameView outputFrame = new();
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
    private Theme theme;
    private Theme helpTheme;
    private Theme selectedFrameTheme;

    private bool IsHelpActive { get; set; } = false;
    public TUIApp(TUIConfig config)
    {
        _config = config;
        theme = Themes.Get(_config.Theme);
        helpTheme = config.Theme == "dark" ? Themes.Get("dark-help") : Themes.Get("light-help");
        selectedFrameTheme = config.Theme == "dark" ? Themes.Get("selected-dark-frame") : Themes.Get("selected-light-frame");
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
            TideUpTerminal();
        }
    }

    private Toplevel RenderTUI(IUser user)
    {
        var top = Terminal.Gui.Application.Top;

        var scheme = theme.ToColorScheme();
        var helpScheme = helpTheme.ToColorScheme();

        RenderWindow(top, scheme);
        RenderTitleBar(scheme);
        RenderFooter(scheme);
        RenderPrompt(scheme, user);
        RenderOutputPane(scheme);
        RenderHelpPane(helpScheme);
        RenderAuthLabel(scheme, user);

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
        AddToCommandHistory(input);

        var args = input.ToArrayOfStrings();
        string command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "login":
            case "signup":
                outputPane.Text = "Please log out before logging in or signing up as a new user.";
                return;

            case "gui":
            case "tui":
                outputPane.Text = "Cannot create a nested interface. \nUse the CLI commands or available shortcuts instead.";
                return;
        }

        string outputStream = RunFlexlib(args);

        switch (command)
        {
            case "logout":
            case "exit":
                ExitTUI();
                return;

            case "help":
                ActivateHelpFrame(TUIHelp.PromptUSage());
                return;

            case "cls":
            case "clear":
                outputPane.Text = "";
                helpPane.Text = "";
                DeactivateHelpFrame();
                break;

            case "dark":
                theme = Themes.Get("dark");
                helpTheme = Themes.Get("dark-help");
                selectedFrameTheme = Themes.Get("selected-dark-frame");
                UpdateThemes();
                break;

            case "light":
                theme = Themes.Get("light");
                helpTheme = Themes.Get("light-help");
                selectedFrameTheme = Themes.Get("selected-light-frame");
                UpdateThemes();
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

    private void UpdateThemes()
    {
        win.ColorScheme = theme.ToColorScheme();
        leftTopLabel.ColorScheme = theme.ToColorScheme();
        rightTopLabel.ColorScheme = theme.ToColorScheme();
        outputFrame.ColorScheme = theme.ToColorScheme();
        outputPane.ColorScheme = theme.ToColorScheme();
        helpFrame.ColorScheme = helpTheme.ToColorScheme();
        helpPane.ColorScheme = helpTheme.ToColorScheme();
        promptPane.ColorScheme = theme.ToColorScheme();
        promptLabel.ColorScheme = theme.ToColorScheme();
        footerPane.ColorScheme = theme.ToColorScheme();
        authLabel.ColorScheme = theme.ToColorScheme();

        outputPane.DrawContent += (_) =>
        {
            outputScrollBar.Size = outputPane.Lines;
            outputScrollBar.ColorScheme = theme.ToColorScheme();
            outputScrollBar.Position = outputPane.TopRow;
            outputScrollBar.Refresh();
        };
        helpPane.DrawContent += (_) =>
        {
            helpScrollBar.Size = helpPane.Lines;
            helpScrollBar.ColorScheme = helpTheme.ToColorScheme();
            helpScrollBar.Position = helpPane.TopRow;
            helpScrollBar.Refresh();
        };

        string meta = $"{theme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
        rightTopLabel.Text = meta;
    }

    private void ActivateHelpFrame(string outputStream)
    {
        int rows = outputStream.RowCount();

        // Clamp rows to max 15
        int visibleRows = Math.Min(rows, 15);


        helpPane.Text = outputStream; // Set text on the TextView
        helpFrame.Height = visibleRows + 2; // Border + padding
        helpFrame.Y = Pos.Top(promptPane) - (visibleRows + 2);
        helpFrame.Visible = true;
        helpFrame.CanFocus = true;
        Terminal.Gui.Application.Refresh();
        IsHelpActive = true;
        helpPane.SetFocus();
    }


    private void DeactivateHelpFrame()
    {
        helpFrame.Visible = false;
        helpFrame.Height = 0;
        helpFrame.CanFocus = false;
        Terminal.Gui.Application.Refresh();
        IsHelpActive = false;
    }

    private string RunFlexlib(string[] args)
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

    private static void TideUpTerminal()
    {
        Console.ResetColor();
        Console.CursorVisible = true;
        Console.Clear();
    }

    private void AddToCommandHistory(string command)
    {
        if (!string.IsNullOrWhiteSpace(command) &&
            (commandHistory.Count == 0 || commandHistory[^1] != command))
        {
            commandHistory.Add(command);
        }
        historyIndex = commandHistory.Count;
    }
}

