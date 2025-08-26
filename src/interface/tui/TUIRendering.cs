using Flexlib.Infrastructure.Interop;
using Terminal.Gui;
using Flexlib.Infrastructure.Environment;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Processing;
using Flexlib.Interface.Output;

namespace Flexlib.Interface.TUI;


public partial class TUIApp : ITUIApp
{
    private static Theme _generalTheme = Themes.Get("basic");
    private static Theme _helpTheme = Themes.Get("basic");
    private static Theme _selectedFrameTheme = Themes.Get("basic");
    private static Theme _selectedPromptTheme = Themes.Get("basic");
    private static Theme _errorFrameTheme = Themes.Get("basic");
    private static Theme _warningFrameTheme = Themes.Get("basic");
    private static Theme _successFrameTheme = Themes.Get("basic");
    private TextView bodyPane = new();
    private TextView outputFrame = new();
    private FrameView helpFrame = new();
    private TextView helpPane = new();
    private Label titleLabel = new();
    private FrameView bottomInfoFrame = new();
    private FrameView topInfoFrame = new();
    private FrameView bodyFrame = new();
    private TextField cmdLinePane = new();
    private Label prompt = new();
    private Label windowTopLeft = new();
    private Label windowTopRight = new();
    private Label topLeftLabel = new();
    private Label topRightLabel = new();
    private Label bottomLeftLabel = new();
    private Label bottomRightLabel = new();
    private Label authLabel = new();
    private Label footerPane = new();
    private ScrollBarView helpScrollBar = new();
    private ScrollBarView outputScrollBar = new();
    private string margin = "     ";
    private Window _tui = new();
    private TUIPage? _page;
    private readonly ConsoleRenderer _renderer = new();

    private void RenderTUI(IUser user)
    {
        var scheme = _generalTheme.ToColorScheme();
        var helpScheme = _helpTheme.ToColorScheme();

        _tui = RenderWindow(scheme);
        RenderTopBar(scheme).ForEach(v => _tui.Add(v));
        RenderFooter(scheme).ForEach(v => _tui.Add(v));
        RenderPrompt(scheme, user).ForEach(v => _tui.Add(v));
        RenderPage(scheme).ForEach(v => _tui.Add(v));
        RenderHelpPane(helpScheme).ForEach(v => _tui.Add(v));
        RenderAuthLabel(scheme, user).ForEach(v => _tui.Add(v));

        _page = new(bodyPane, titleLabel, topLeftLabel, topRightLabel, bottomLeftLabel, bottomRightLabel);

        cmdLinePane.SetFocus();
    }

    private Window RenderWindow(ColorScheme scheme)
    {
        var win = new Window()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = scheme,
            Border = new Border() { BorderStyle = BorderStyle.None }
        };

        return win;

    }

    private List<View> RenderTopBar(ColorScheme scheme)
    {
        string logo = $"{margin}>::>  flexlib";  // Removed leading spaces to avoid confusion
        windowTopLeft = new Label(logo)
        {
            X = 0,
            Y = 0,
            Width = Dim.Sized(logo.Length),
            Height = 3,
            TextAlignment = TextAlignment.Left,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        windowTopLeft.CanFocus = false;

        string meta = $"{_generalTheme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
        windowTopRight = new Label(meta)
        {
            X = Pos.AnchorEnd(meta.Length),
            Y = 0,
            Width = Dim.Sized(meta.Length),
            Height = 3,
            TextAlignment = TextAlignment.Right,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        topRightLabel.CanFocus = false;

        return new List<View>() {windowTopLeft, windowTopRight};
    }

    private List<View> RenderFooter(ColorScheme scheme)
    {
        footerPane = new Label()
        {
            X = margin.Length,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            Height = 1,
            TextAlignment = TextAlignment.Left,
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = scheme
        };
        footerPane.CanFocus = false;
        return new List<View>() {footerPane};
    }

    private List<string> commandHistory = new();
    private int historyIndex = -1;

    private List<View> RenderPrompt(ColorScheme scheme, IUser user)
    {
        // Prompt Label
        prompt = new Label(">")
        {
            X = margin.Length,
            Y = Pos.Top(footerPane) - 1,
            Width = 2,
            Height = 1,
            ColorScheme = scheme,
            CanFocus = false
        };

        // Prompt Text Field
        cmdLinePane = new TextField("")
        {
            X = Pos.Right(prompt),
            Y = Pos.Top(footerPane) - 1,
            Width = Dim.Fill() - user.Id.Length - 10,
            Height = 1,
            ColorScheme = scheme
        };

        // Change prompt symbol on focus/blur
        cmdLinePane.Enter += (_) =>
        {
            prompt.Text = ">";
            prompt.ColorScheme = _selectedPromptTheme.ToColorScheme();
        };
        cmdLinePane.Leave += (_) =>
        {
            prompt.Text = "˄";
            prompt.ColorScheme = _generalTheme.ToColorScheme();
        };

        cmdLinePane.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                string input = cmdLinePane.Text?.ToString() ?? "";

                TUIRouter(input);

                cmdLinePane.Text = "";
                args.Handled = true;
            }

            if (args.KeyEvent.Key == Key.Esc)
            {
                DeactivateHelpFrame();

                cmdLinePane.Text = "";
                args.Handled = true;

                cmdLinePane.SetFocus();
            }
        };

        // Key handling for history navigation
        cmdLinePane.KeyPress += (e) =>
        {
            if (e.KeyEvent.Key == Key.CursorUp)
            {
                if (commandHistory.Count > 0 && historyIndex > 0)
                {
                    historyIndex--;
                    cmdLinePane.Text = commandHistory[historyIndex] + " ";
                }
                e.Handled = true;
            }
            else if (e.KeyEvent.Key == Key.CursorDown)
            {
                if (commandHistory.Count > 0 && historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    cmdLinePane.Text = commandHistory[historyIndex] + " ";
                }
                else
                {
                    historyIndex = commandHistory.Count;
                    cmdLinePane.Text = "";
                }
                e.Handled = true;
            }
            if (e.KeyEvent.Key == Key.CursorUp || e.KeyEvent.Key == Key.CursorDown)
            {
                cmdLinePane.CursorPosition = cmdLinePane.Text.Length;
            }
        };

        return new List<View> { prompt, cmdLinePane };
    }

    public void DisplayLogo(string noddingPhrase, string productName, string catchPhrase)
    {
        var logoWin = new Window()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = _generalTheme.ToColorScheme(),
            Border = new Border() { BorderStyle = BorderStyle.None } // clean background
        };

        var logo = new View()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            VerticalTextAlignment = VerticalTextAlignment.Middle,
            ColorScheme = _generalTheme.ToColorScheme()
        };

        var noddingLabel = new Label($"{noddingPhrase}\n")
        {
            X = Pos.Center(),
            Y = Pos.Center() - 4
        };
        logo.Add(noddingLabel);

        var productLabel = new Label(Components.AsciiArt(productName))
        {
            X = Pos.Center(),
            Y = Pos.Bottom(noddingLabel),
            TextAlignment = TextAlignment.Centered
        };
        logo.Add(productLabel);

        var catchLabel = new Label($"{catchPhrase}")
        {
            X = Pos.Center(),
            Y = Pos.Bottom(productLabel)
        };
        logo.Add(catchLabel);

        logoWin.Add(logo);

        _tui.Add(logoWin);
        Terminal.Gui.Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(3), (_) =>
        {
            _tui.Remove(logoWin);
            cmdLinePane.DesiredCursorVisibility = CursorVisibility.Invisible;
            TUIRouter("list-libs", false);
            return false;
        });
    }
    private View RenderTitleRow(string title, ColorScheme scheme)
    {
        titleLabel = new Label(title)
        {
            X = margin.Length + 1,
            Y = Pos.Bottom(windowTopLeft) + 1,
            Width = Dim.Fill() - margin.Length,
            Height = 2,
            ColorScheme = scheme,
            CanFocus = false,
            TextAlignment = TextAlignment.Left
        };
        return titleLabel;
    }

    private View RenderTopInfoRow(string leftInfo, string rightInfo, ColorScheme scheme)
    {
        topInfoFrame = new FrameView()
        {
            X = margin.Length + 1,
            Height = 1,
            Width = Dim.Fill() - margin.Length - 1,
            Y = Pos.Bottom(titleLabel),
            CanFocus = false,
            ColorScheme = scheme,
            Border = new Border() { BorderStyle = BorderStyle.None }
        };

        topLeftLabel = new Label(leftInfo ?? "")
        {
            X = 0,
            Y = 0,
            CanFocus = false,
            ColorScheme = scheme
        };

        topRightLabel = new Label(rightInfo ?? "")
        {
            X = Pos.AnchorEnd(),
            Y = 0,
            CanFocus = false,
            ColorScheme = scheme
        };

        topInfoFrame.Add(topLeftLabel);
        topInfoFrame.Add(topRightLabel);

        return topInfoFrame;
    }

    private View RenderBottomInfoRow(string leftInfo, string rightInfo, ColorScheme scheme)
    {
        bottomInfoFrame = new FrameView()
        {
            X = margin.Length + 1,
            Height = 1,
            Width = Dim.Fill() - margin.Length - 1, // match top frame
            Y = Pos.Bottom(bodyFrame),
            CanFocus = false,
            ColorScheme = scheme,
            Border = new Border() { BorderStyle = BorderStyle.None }
        };

        bottomLeftLabel = new Label(leftInfo ?? "")
        {
            X = 0,
            Y = 0,
            CanFocus = false,
            ColorScheme = scheme
        };

        bottomRightLabel = new Label(rightInfo ?? "")
        {
            X = Pos.AnchorEnd(),
            Y = 0,
            CanFocus = false,
            ColorScheme = scheme
        };

        bottomInfoFrame.Add(bottomLeftLabel);
        bottomInfoFrame.Add(bottomRightLabel);

        return bottomInfoFrame;
    }

    private (View Frame, TextView bodyPane) RenderBodyPane(ColorScheme scheme)
    {
        int outputScrollbarMargin = 1;

        bodyFrame = new FrameView()
        {
            X = margin.Length,
            Y = Pos.Bottom(topInfoFrame), // below title
            Width = Dim.Fill() - margin.Length,
            Height = Dim.Fill() - 5,
            ColorScheme = scheme,
            CanFocus = false
        };

        bodyPane = new TextView()
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - (outputScrollbarMargin + 1) - 1,
            Height = Dim.Fill() - 1,
            ReadOnly = false,
            ColorScheme = scheme,
            DesiredCursorVisibility = CursorVisibility.Invisible
        };
        bodyFrame.Add(bodyPane);

        // focus events
        bodyPane.Enter += (_) =>
        {
            bodyPane.ReadOnly = true;
            bodyFrame.ColorScheme = _selectedFrameTheme.ToColorScheme();
            bodyPane.ColorScheme = _selectedFrameTheme.ToColorScheme();
        };
        bodyPane.Leave += (_) =>
        {
            bodyPane.ReadOnly = false;
            bodyFrame.ColorScheme = _generalTheme.ToColorScheme();
            bodyPane.ColorScheme = _generalTheme.ToColorScheme();
        };
        BindScrollToArrowKeys(bodyPane);

        // scrollbar
        var scrollBar = new ScrollBarView(bodyPane, true)
        {
            X = Pos.Right(bodyPane) + outputScrollbarMargin
        };
        scrollBar.ChangedPosition += () =>
        {
            bodyPane.TopRow = scrollBar.Position;
            bodyPane.SetNeedsDisplay();
        };
        bodyPane.DrawContent += (_) =>
        {
            scrollBar.Size = bodyPane.Lines;
            scrollBar.ColorScheme = scheme;
            scrollBar.Position = bodyPane.TopRow;
            scrollBar.Refresh();
        };

        return (bodyFrame, bodyPane);
    }

    public List<View> RenderPage(ColorScheme scheme,
                                    string title = "LIBRARIES", 
                                    string topLeftInfo = "TOP LEFT INFO",
                                    string topRightInfo = "TOP RIGHT INFO",
                                    string bottomLeftInfo = "BOTTOM LEFT INFO",
                                    string bottomRightInfo = "BOTTOM RIGHT INFO")
    {
        var titleRow = RenderTitleRow(title, scheme);
        var topInfoRow = RenderTopInfoRow(topLeftInfo, topRightInfo, scheme);
        var (bodyFrame, bodyPane) = RenderBodyPane(scheme);
        var bottomInfoRow = RenderBottomInfoRow(bottomLeftInfo, bottomRightInfo, scheme);

        // save refs if needed
        bodyPane.ReadOnly = false;

        return new List<View> { titleRow, bodyFrame, topInfoRow, bottomInfoRow };
    }

    private List<View> RenderHelpPane(ColorScheme helpScheme)
    {
        // --- Help Frame & Help Pane ---
        helpFrame = new FrameView("? TUI Help")
        {
            X = margin.Length - 2,
            Y = Pos.Top(cmdLinePane),
            Width = Dim.Fill() - margin.Length - 35,
            Height = 0,
            ColorScheme = helpScheme,
            CanFocus = false,
            Visible = false,
        };

        // Make the helpPane narrower so there’s a margin before the scrollbar
        int scrollbarMargin = 1; // space between text and scrollbar
        helpPane = new TextView()
        {
            X = 2,
            Y = 1,
            Width = Dim.Fill() - (2 + scrollbarMargin) - 1,
            Height = Dim.Fill() - 1,
            ReadOnly = true,
            ColorScheme = helpScheme,
            DesiredCursorVisibility = CursorVisibility.Invisible,
            WordWrap = true,
        };
        BindScrollToArrowKeys(helpPane);

        helpPane.Enter += (_) =>
        {
            helpPane.ReadOnly = true;
            helpFrame.ColorScheme = _selectedFrameTheme.ToColorScheme();
            helpPane.ColorScheme = _selectedFrameTheme.ToColorScheme();
        };
        helpPane.Leave += (_) =>
        {
            helpPane.ReadOnly = false;
            helpFrame.ColorScheme = _generalTheme.ToColorScheme();
            helpPane.ColorScheme = _generalTheme.ToColorScheme();
        };

        helpFrame.Add(helpPane);

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
            helpScrollBar.ColorScheme = helpScheme;
            helpScrollBar.Position = helpPane.TopRow;
            helpScrollBar.Refresh();
        };

        return new List<View>() { helpFrame };

    }

    private List<View> RenderAuthLabel(ColorScheme scheme, IUser user)
    {
        authLabel = new Label($"{user.Id}")
        {
            X = Pos.AnchorEnd(user.Id.Length) - margin.Length,
            Y = Pos.Top(footerPane) - 1,
            Width = 2,
            Height = 1,
            ColorScheme = scheme
        };

        return new List<View>() { authLabel };
    }
    public delegate Result? RenderTextDelegate(ColorScheme scheme,
                                                    string prompt,
                                                string initialText,
                                                string margin,
                                                Pos xPosition,
                                                Pos yPosition,
                                                int height
                                                );
    private static Result? RenderTUITextReader(ColorScheme scheme,
                                                string prompt,
                                                string initialText,
                                                string margin,
                                                Pos xPosition,
                                                Pos yPosition,
                                                int height
                                                )
    {
        Result? result = null;

        // Dialog box
        int dialogWidth = 80;
        int dialogHeight = height;
        int scrollbarMargin = 1;
        var dialog = new Window($"📓 {prompt}")
        {
            X = xPosition,
            Y = yPosition,
            Width = dialogWidth,
            Height = dialogHeight,
            ColorScheme = scheme
        };

        // Bottom hint (1 line reserved)
        var controlInfo = new Label("Ctrl+X (Save) | ESC (Cancel)")
        {
            X = 1,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill() - 2,
            Height = 1,
            ColorScheme = scheme,
            CanFocus = false
        };

        // Multiline editor fills everything above the hint line
        var textView = new TextView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - (scrollbarMargin + 2),   // leave space for scrollbar
            Height = Dim.Fill() - 2,
            ColorScheme = scheme,
            Text = initialText,
            WordWrap = true,
            CanFocus = true
        };

        // Add TextView first so ScrollBarView has a SuperView
        dialog.Add(textView);

        // Create ScrollBarView
        var scrollBar = new ScrollBarView(textView, true)
        {
            X = Pos.Right(textView) + scrollbarMargin
        };

        // Sync scroll with TextView
        scrollBar.ChangedPosition += () =>
        {
            textView.TopRow = scrollBar.Position;
            textView.SetNeedsDisplay();
        };

        textView.DrawContent += (_) =>
        {
            scrollBar.Size = textView.Lines;
            scrollBar.Position = textView.TopRow;
            scrollBar.ColorScheme = scheme;
            scrollBar.Refresh();
        };

        // Allow keyboard navigation
        BindScrollToArrowKeys(textView);

        // Key handling for confirm/cancel
        textView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Esc)
            {
                result = Result.Fail("Operation cancelled."); // canceled
                Terminal.Gui.Application.RequestStop();
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == (Key.CtrlMask | Key.X)) // confirm
            {
                result = Result.Success("Text successfully read.", textView.Text.ToString());
                Terminal.Gui.Application.RequestStop();
                args.Handled = true;
            }
        };

        dialog.Add(scrollBar, controlInfo);
        Terminal.Gui.Application.Run(dialog);

        return result;
    }

    private bool ConfirmationPrompt(ColorScheme scheme, string promptMessage)
    {
        bool result = false;

        int dialogWidth = 80;
        int dialogHeight = 15;
        int scrollbarMargin = 1;

        var dialog = new Window($"⚠  Confirmation")
        {
            X = margin.Length - 2,
            Y = Pos.Top(prompt) - dialogHeight,
            Width = dialogWidth,
            Height = dialogHeight,
            ColorScheme = scheme
        };

        // TextView first (so ScrollBarView has a SuperView)
        var textView = new TextView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - (scrollbarMargin + 2), // space for scrollbar
            Height = Dim.Fill() - 2, // space for bottom label
            ColorScheme = scheme,
            Text = promptMessage,
            WordWrap = true,
            CanFocus = true,
            ReadOnly = true,
            DesiredCursorVisibility = CursorVisibility.Invisible
        };
        dialog.Add(textView);

        // ScrollBarView
        var scrollBar = new ScrollBarView(textView, true)
        {
            X = Pos.Right(textView) + scrollbarMargin
        };

        scrollBar.ChangedPosition += () =>
        {
            textView.TopRow = scrollBar.Position;
            textView.SetNeedsDisplay();
        };

        textView.DrawContent += (_) =>
        {
            scrollBar.Size = textView.Lines;
            scrollBar.Position = textView.TopRow;
            scrollBar.ColorScheme = scheme;
            scrollBar.Refresh();
        };

        BindScrollToArrowKeys(textView);

        // Key handling
        textView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Esc)
            {
                result = false;
                Terminal.Gui.Application.RequestStop();
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == (Key.CtrlMask | Key.X))
            {
                result = true;
                Terminal.Gui.Application.RequestStop();
                args.Handled = true;
            }
        };

        // Bottom hint
        var controlInfo = new Label("Ctrl+X (Confirm) • ESC (Cancel)")
        {
            X = 1,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill() - 2,
            Height = 1,
            ColorScheme = scheme,
            CanFocus = false
        };

        dialog.Add(scrollBar, controlInfo);
        Terminal.Gui.Application.Run(dialog);

        return result;
    }

    private void RenderResult(Result result)
    {
        ColorScheme scheme = _generalTheme.ToColorScheme();
        if (result.IsSuccess)
            scheme = _successFrameTheme.ToColorScheme();
        if (result.IsFailure)
            scheme = _errorFrameTheme.ToColorScheme();
        if (result.IsWarning)
            scheme = _warningFrameTheme.ToColorScheme();

        int dialogWidth = 80;
        int dialogHeight = 15;
        int scrollbarMargin = 1;

        var dialog = new Window($"ℹ  Response")
        {
            X = margin.Length - 2,
            Y = Pos.Top(prompt) - dialogHeight,
            Width = dialogWidth,
            Height = dialogHeight,
            ColorScheme = scheme
        };

        // TextView directly on the Window
        var textView = new TextView()
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - (scrollbarMargin + 2),
            Height = Dim.Fill() - 2,
            ReadOnly = true,
            WordWrap = true,
            Text = result.Message ?? string.Empty,
            ColorScheme = scheme,
            DesiredCursorVisibility = CursorVisibility.Invisible
        };
        textView.Text = result.Message?.Trim() ?? string.Empty;
        // Add TextView first so ScrollBarView has a SuperView
        dialog.Add(textView);

        // Now create ScrollBarView
        var scrollBar = new ScrollBarView(textView, true)
        {
            X = Pos.Right(textView) + scrollbarMargin
        };

        scrollBar.ChangedPosition += () =>
        {
            textView.TopRow = scrollBar.Position;
            textView.SetNeedsDisplay();
        };

        textView.DrawContent += (_) =>
        {
            scrollBar.Size = textView.Lines;
            scrollBar.Position = textView.TopRow;
            scrollBar.ColorScheme = scheme;
            scrollBar.Refresh();
        };

        BindScrollToArrowKeys(textView);

        textView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                Terminal.Gui.Application.RequestStop();
                args.Handled = true;
            }
        };

        // Bottom hint
        var controlInfo = new Label("Enter (Close)")
        {
            X = 1,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill() - 2,
            Height = 1,
            ColorScheme = scheme
        };

        dialog.Add(scrollBar, controlInfo);
        Terminal.Gui.Application.Run(dialog);
    }

    private void UpdateThemes(string theme)
    {
        _generalTheme = Themes.Get($"{theme}");
        _helpTheme = Themes.Get($"{theme}-help");
        _selectedFrameTheme = Themes.Get($"selected-{theme}-frame");
        _selectedPromptTheme = Themes.Get($"selected-{theme}-prompt");
        _errorFrameTheme = Themes.Get($"error-{theme}-frame");
        _warningFrameTheme = Themes.Get($"warning-{theme}-frame");
        _successFrameTheme = Themes.Get($"success-{theme}-frame");
    }
    private void UpdateSchemes(Window win)
    {
        win.ColorScheme             = _generalTheme .ToColorScheme();
        topLeftLabel.ColorScheme    = _generalTheme .ToColorScheme();
        topRightLabel.ColorScheme   = _generalTheme .ToColorScheme();
        outputFrame.ColorScheme     = _generalTheme .ToColorScheme();
        bodyPane.ColorScheme        = _generalTheme .ToColorScheme();
        helpFrame.ColorScheme       = _helpTheme    .ToColorScheme();
        helpPane.ColorScheme        = _helpTheme    .ToColorScheme();
        cmdLinePane.ColorScheme      = _generalTheme .ToColorScheme();
        prompt.ColorScheme     = _generalTheme .ToColorScheme();
        footerPane.ColorScheme      = _generalTheme .ToColorScheme();
        authLabel.ColorScheme       = _generalTheme .ToColorScheme();

        bodyPane.DrawContent += (_) =>
        {
            outputScrollBar.Size = bodyPane.Lines;
            outputScrollBar.ColorScheme = _generalTheme.ToColorScheme();
            outputScrollBar.Position = bodyPane.TopRow;
            outputScrollBar.Refresh();
        };
        helpPane.DrawContent += (_) =>
        {
            helpScrollBar.Size = helpPane.Lines;
            helpScrollBar.ColorScheme = _helpTheme.ToColorScheme();
            helpScrollBar.Position = helpPane.TopRow;
            helpScrollBar.Refresh();
        };

        string meta = $"{_generalTheme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
        topRightLabel.Text = meta;
    }

    public class TUIReader : IReader
    {
        private readonly RenderTextDelegate _renderer;

        public TUIReader(RenderTextDelegate renderer)
        {
            _renderer = renderer;
        }

        public string? ReadText(string text)
        {
            var result = _renderer(_generalTheme.ToColorScheme(),
             "Authentication",
            text,
            "     ",
            0,
            0,
            15
            );
            return result?.Payload as string;
        }

        public string? ReadText() => ReadText("");

        public string? ReadPassword() => "";
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
    private void ActivateHelpFrame(string outputStream)
    {
        int rows = outputStream.RowCount();

        // Clamp rows to max 15
        int visibleRows = Math.Min(rows, 15);


        helpPane.Text = outputStream; // Set text on the TextView
        helpFrame.Height = visibleRows + 2; // Border + padding
        helpFrame.Y = Pos.Top(cmdLinePane) - (visibleRows + 2);
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

}