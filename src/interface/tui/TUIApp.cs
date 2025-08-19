using Terminal.Gui;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;
using Flexlib.Application.Ports;
using System.Diagnostics;
using Flexlib.Application.UseCases;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Interface.Output;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Services.Media;

namespace Flexlib.Interface.TUI;


public partial class TUIApp : ITUIApp
{
    private readonly TUIConfig _config;
    private bool IsHelpActive { get; set; } = false;
    private static ILibraryRepository _libRepo = new JsonLibraryRepository();
    private static readonly IUserRepository _userRepo = new JsonUserRepository();
    private static readonly IReader _reader = new TUIReader(RenderTUITextReader);
    private readonly IMediaService _mediaService = MediaServiceFactory.CreateDefault();
    private static readonly Authenticator _auth = new Authenticator(_userRepo);
    private static LibraryItem? _selectedItem { get; set; }
    private static Library? _selectedLibrary { get; set; }
    private static Result? _result { get; set; }
    private static int _dialogHeight = 15;
    private static string? _finalAction;
    private static string? itemName;
    private IUser? _user { get; set; }

    public TUIApp(TUIConfig config)
    {
        _config = config;
        _generalTheme = Themes.Get(_config.Theme);
        _helpTheme = config.Theme == "dark" ? Themes.Get("dark-help") : Themes.Get("light-help");
        _selectedFrameTheme = config.Theme == "dark" ? Themes.Get("selected-dark-frame") : Themes.Get("selected-light-frame");
        _errorFrameTheme = config.Theme == "dark" ? Themes.Get("error-dark-frame") : Themes.Get("error-light-frame");
        _warningFrameTheme = config.Theme == "dark" ? Themes.Get("warning-dark-frame") : Themes.Get("warning-light-frame");
        _successFrameTheme = config.Theme == "dark" ? Themes.Get("success-dark-frame") : Themes.Get("success-light-frame");
    }

    public void Run(IUser user)
    {
        _user = user;

        try
        {
            Terminal.Gui.Application.Init();
            var tui = RenderTUI(user);

            pagePane.GetCurrentWidth(out int currentPagePaneWidth);
            var libraryList = ListLibs.Execute(_libRepo);
            if (libraryList.Payload is List<Library> libs)
            {
                pagePane.Text = new ConsoleRenderer().FormatLibraryTable(libs, Env.GetSafeWindowWidth()).ToMultiRowString();
            }

            Terminal.Gui.Application.Run();
        }
        finally
        {
            Terminal.Gui.Application.Shutdown();
            TideUpTerminal();

            switch (_finalAction)
            {
                case "login":
                    _auth.Logout();
                    RestartTUI(["tui"]);
                    break;
                case "signup":
                    _auth.Logout();
                    _auth.RegisterUser();
                    RestartTUI(["tui"]);
                    break;
            }
        }
    }

    private string RunFlexlib(string[] args)
    {
        string flexlibExe = Path.Combine(AppContext.BaseDirectory, "Flexlib.exe");

        // Determine a virtual console width for CLI rendering
        int tuiWidth = pagePane.Bounds.Width; // or some other measurement
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

    private void RestartTUI(string[] args)
    {
        _finalAction = null;
        Program.Main(args);
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
        if (!string.IsNullOrWhiteSpace(command))
        {
            command = command.TrimEnd();
            if (command.EndsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                command = command.Substring(0, command.Length - 4).TrimEnd();
            }

            if (!string.IsNullOrWhiteSpace(command) && (commandHistory.Count == 0 || commandHistory[^1] != command))
            {
                commandHistory.Add(command);
            }
        }

        historyIndex = commandHistory.Count;
    }
    
    private void UpdatePagePane()
    {
        TUIController(_page?.Address ?? "list-libs", false);
    }   

}

