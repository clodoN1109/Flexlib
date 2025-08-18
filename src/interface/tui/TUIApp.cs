using Terminal.Gui;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Processing;
using System.Diagnostics;
using Flexlib.Interface.Input;
using Flexlib.Application.UseCases;
using Flexlib.Application.Common;
using Flexlib.Interface.CLI;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Interface.Output;
using Flexlib.Infrastructure.Authentication;
using System.Collections;

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
    private bool IsHelpActive { get; set; } = false;
    private static readonly ILibraryRepository _libRepo = new JsonLibraryRepository();
    private static readonly IUserRepository _userRepo = new JsonUserRepository();
    private static readonly IReader _reader = new TUIReader(RenderTUITextReader);
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

            outputPane.GetCurrentWidth(out int currentOutputPaneWidth);
            var libraryList = ListLibs.Execute(_libRepo);
            if (libraryList.Payload is List<Library> libs)
            {
                outputPane.Text = new ConsoleRenderer().FormatLibraryTable(libs, Env.GetSafeWindowWidth()).ToMultiRowString();
            }

            Terminal.Gui.Application.Run();
        }
        finally
        {
            Terminal.Gui.Application.Shutdown();
            TideUpTerminal();

            switch (_finalAction){
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

    private Toplevel RenderTUI(IUser user)
    {
        var top = Terminal.Gui.Application.Top;

        var scheme = _generalTheme.ToColorScheme();
        var helpScheme = _helpTheme.ToColorScheme();

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
        string commandName = args[0].ToLowerInvariant();

        //Special TUI commands
        switch (commandName)
        {
            case "gui":
            case "tui":
                outputPane.Text = "Cannot create a nested interface. \nUse the CLI commands or available shortcuts instead.";
                return;

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
                return;

            case "dark":
                UpdateThemes("dark");
                UpdateSchemes();
                return;

            case "light":
                UpdateThemes("light");
                UpdateSchemes();
                return;

        }
        
        // CLI commands
        InputPreProcessing.Execute(input.ToArrayOfStrings(), out PreProcessingResult processed);

        if (!processed.IsValid || processed.Value is not Input.Command cmd || !Input.Command.IsKnownCommandName(commandName))
        {
            RenderResult(Result.Fail($"Invalid command '{input}'. \n\nFor the list of available commands, use 'help'."));            return;
        }

        if (cmd.IsSpecificHelp())
        {
            string outputStream = RunFlexlib(args);
            ActivateHelpFrame(outputStream);
            return;
        }

        if (!cmd.IsValid())
        {
            RenderResult(Result.Fail("Invalid command usage. For details, run: <command name> help."));
            return;
        }
        
        DeactivateHelpFrame();

        string promptMessage;
        switch (cmd)
        {
            case LoginCommand:
                _finalAction = "login";
                ExitTUI();
                return;

            case SignUpCommand:
                _finalAction = "signup";
                ExitTUI();
                return;

            case LogoutCommand:
                _auth.Logout();
                ExitTUI();
                return;

            case GetItemOriginCommand c:
                _result = GetItemOrigin.Execute(c.ItemId, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case EditNoteCommand c:

                _result = SelectItemNote.Execute(c.ItemId, c.NoteId, c.LibName, _libRepo);

                if ((_result.Payload is Domain.Note currentNote) && !string.IsNullOrWhiteSpace(currentNote.Text))
                {
                    itemName = _libRepo.GetByName(c.LibName)?.GetItemById(c.ItemId)?.Name;
                    _result = RenderTUITextReader(_selectedFrameTheme.ToColorScheme(),
                                        $"{c.LibName}/{(itemName?.IsCompound() ?? false ? $"\'{itemName}\'" : itemName) ?? c.ItemId} | Note {c.NoteId}",
                                        currentNote.Text,
                                        margin,
                                        margin.Length - 2,
                                        Pos.Top(promptLabel) - _dialogHeight, _dialogHeight
                                        );
                }
                if (_result?.IsSuccess ?? false)
                    _result = EditNote.Execute(c.ItemId, c.NoteId, c.LibName, (string)(_result?.Payload ?? ""), _libRepo);
                else
                {
                    _result = Result.Warn("Note editing canceled.");
                }
                RenderResult(_result);
                break;

            case NewNoteCommand c:
                itemName = _libRepo.GetByName(c.LibName)?.GetItemById(c.ItemId)?.Name;
                _result = RenderTUITextReader(_selectedFrameTheme.ToColorScheme(),
                    $"{c.LibName}/{(itemName?.IsCompound() ?? false ? $"\'{itemName}\'" : itemName) ?? c.ItemId}",
                    "",
                    margin,
                    margin.Length - 2,
                    Pos.Top(promptLabel) - _dialogHeight, _dialogHeight
                    );
                if (_result?.IsSuccess ?? false)
                    _result = NewNote.Execute(c.ItemId, c.LibName, (string) (_result?.Payload ?? ""), _user!, _libRepo);
                else
                {
                    _result = Result.Warn("Note creation canceled.");
                }
                RenderResult(_result);
                break;

            case RemoveNoteCommand c:
                _result = SelectItemNote.Execute(c.ItemId, c.NoteId, c.LibName, _libRepo);

                if ((_result.Payload is Domain.Note targetNote) && !string.IsNullOrWhiteSpace(targetNote.Text))
                {
                    itemName = _libRepo.GetByName(c.LibName)?.GetItemById(c.ItemId)?.Name;
                    promptMessage = $"\nAre you sure you want to delete note of ID {c.NoteId} from item '{itemName ?? ""}' from library '{c.LibName ?? ""}'?\n\n";
                    if (!ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), promptMessage))
                        _result = Result.Warn("Note deletion canceled.");
                    else
                    {
                        _result = RemoveNote.Execute(c.ItemId, c.NoteId, c.LibName!, _libRepo);                        
                    }
                }
                RenderResult(_result);
                break;

            case RemoveItemCommand c:
                _selectedLibrary = _libRepo.GetByName(c.LibraryName)!;
                _selectedItem = _selectedLibrary.GetItemById(c.ItemId);
                if (_selectedItem == null)
                {
                    _result = Result.Fail($"Item with ID {c.ItemId} not found in library {c.LibraryName}.");
                    RenderResult(_result);
                    break;
                }

                promptMessage = $"\nAre you sure you want to delete the item '{_selectedItem?.Name ?? ""}' from library '{_selectedLibrary?.Name ?? ""}'?\n\n";
                if (! ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), promptMessage))
                    break;
                _result = RemoveItem.Execute(c.ItemId, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case RemoveLibraryCommand c:
                _selectedLibrary = _libRepo.GetByName(c.Name)!;
                if (_selectedLibrary == null)
                {
                    _result = Result.Fail($"Library named {c.Name} not found.");
                    RenderResult(_result);
                    break;
                }

                promptMessage = $"\nAre you sure you want to delete the library '{c.Name}' at path:\n\n  {_selectedLibrary.Path} ?\n";
                if (! ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), promptMessage))
                    break;
                _result = RemoveLibrary.Execute(c.Name, _libRepo);
                RenderResult(_result);
                break;

            default:
                string outputStream = RunFlexlib(args);
                DeactivateHelpFrame();
                outputPane.Text = outputStream;
                break;
        }

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

}

