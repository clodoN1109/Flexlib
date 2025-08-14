using Flexlib.Infrastructure.Interop;
using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using Flexlib.Infrastructure.Modelling;
using Terminal.Gui;
using Flexlib.Infrastructure.Environment;
using Flexlib.Application.Ports;

namespace Flexlib.Interface.TUI;


public partial class TUIApp : ITUIApp
{
    private static Theme _generalTheme = Themes.Get("basic");
    private static Theme _helpTheme = Themes.Get("basic");
    private static Theme _selectedFrameTheme = Themes.Get("basic");
    private static Theme _errorFrameTheme = Themes.Get("basic");
    private static Theme _warningFrameTheme = Themes.Get("basic");
    private static Theme _successFrameTheme = Themes.Get("basic");


    private void RenderWindow(Toplevel top, ColorScheme scheme)
    {
        win = new Window()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = scheme,
            Border = new Border() { BorderStyle = BorderStyle.None }
        };
        top.Add(win);
    }

    private void RenderTitleBar(ColorScheme scheme)
    {
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

        string meta = $"{_generalTheme.Icon}       v{(Env.IsDebug() ? Env.BuildId : Env.Version)}{margin}";  // Minimal spaces for cleaner alignment
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


    }

    private void RenderFooter(ColorScheme scheme)
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
        win.Add(footerPane);
    }

    private List<string> commandHistory = new();
    private int historyIndex = -1;

    private void RenderPrompt(ColorScheme scheme, IUser user)
    {
        // Prompt Label
        promptLabel = new Label(">")
        {
            X = margin.Length,
            Y = Pos.Top(footerPane) - 1,
            Width = 2,
            Height = 1,
            ColorScheme = scheme,
            CanFocus = false
        };

        // Prompt Text Field
        promptPane = new TextField("")
        {
            X = Pos.Right(promptLabel),
            Y = Pos.Top(footerPane) - 1,
            Width = Dim.Fill() - user.Id.Length - 10,
            Height = 1,
            ColorScheme = scheme
        };

        // Change prompt symbol on focus/blur
        promptPane.Enter += (_) => promptLabel.Text = ">";
        promptPane.Leave += (_) => promptLabel.Text = "˄";

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

        // Key handling for history navigation
        promptPane.KeyPress += (e) =>
        {
            if (e.KeyEvent.Key == Key.CursorUp)
            {
                if (commandHistory.Count > 0 && historyIndex > 0)
                {
                    historyIndex--;
                    promptPane.Text = commandHistory[historyIndex];
                }
                e.Handled = true;
            }
            else if (e.KeyEvent.Key == Key.CursorDown)
            {
                if (commandHistory.Count > 0 && historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    promptPane.Text = commandHistory[historyIndex];
                }
                else
                {
                    historyIndex = commandHistory.Count;
                    promptPane.Text = "";
                }
                e.Handled = true;
            }
            if (e.KeyEvent.Key == Key.CursorUp || e.KeyEvent.Key == Key.CursorDown)
            {
                promptPane.CursorPosition = promptPane.Text.Length;
            }
        };

        win.Add(promptLabel, promptPane);
    }

    private void RenderOutputPane(ColorScheme scheme)
    {
        // --- Output Pane ---
        int outputScrollbarMargin = 1;
        outputFrame = new FrameView()
        {
            X = margin.Length,
            Y = Pos.Bottom(rightTopLabel),
            Width = Dim.Fill() - margin.Length,
            Height = Dim.Fill() - 3,
            ColorScheme = scheme,
            CanFocus = false,
        };

        outputPane = new TextView()
        {
            X = margin.Length,
            Y = 1,
            Width = Dim.Fill() - (margin.Length + outputScrollbarMargin + 1),
            Height = Dim.Fill() - 1,
            ReadOnly = false,
            ColorScheme = scheme,
            DesiredCursorVisibility = CursorVisibility.Invisible
        };
        outputFrame.Add(outputPane);

        outputPane.Enter += (_) =>
        {
            outputPane.ReadOnly = true;
            outputFrame.ColorScheme = _selectedFrameTheme.ToColorScheme();
            outputPane.ColorScheme = _selectedFrameTheme.ToColorScheme();
        };
        outputPane.Leave += (_) =>
        {
            outputPane.ReadOnly = false;
            outputFrame.ColorScheme = _generalTheme.ToColorScheme();
            outputPane.ColorScheme = _generalTheme.ToColorScheme();
        };
        BindScrollToArrowKeys(outputPane);
        win.Add(outputFrame);

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

    }

    private void RenderHelpPane(ColorScheme helpScheme)
    {
        // --- Help Frame & Help Pane ---
        helpFrame = new FrameView("? TUI Help")
        {
            X = margin.Length - 2,
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
            X = 2,
            Y = 1,
            Width = Dim.Fill() - (2 + scrollbarMargin), // space for frame border + margin + scrollbar
            Height = Dim.Fill() - 1,
            ReadOnly = true,
            ColorScheme = helpScheme,
            DesiredCursorVisibility = CursorVisibility.Invisible,
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
            helpScrollBar.ColorScheme = helpScheme;
            helpScrollBar.Position = helpPane.TopRow;
            helpScrollBar.Refresh();
        };

    }

    private void RenderAuthLabel(ColorScheme scheme, IUser user)
    {
        authLabel = new Label($"{user.Id}")
        {
            X = Pos.AnchorEnd(user.Id.Length) - margin.Length,
            Y = Pos.Top(footerPane) - 1,
            Width = 2,
            Height = 1,
            ColorScheme = scheme
        };

        win.Add(authLabel);

    }
    private string ReadText(ColorScheme scheme, string prompt = "Enter text:", string initialText = "")
    {
        string? result = null;

        // Dialog box
        int dialogWidth = 80;
        int dialogHeight = 15;

        var dialog = new Window($"📓 {prompt}")
        {
            X = margin.Length - 2,
            Y = Pos.Top(promptLabel) - dialogHeight,
            Width = dialogWidth,
            Height = dialogHeight,
            ColorScheme = scheme
        };

        // Bottom hint (1 line reserved)
        var controlInfo = new Label("Ctrl+X (Save) | ESC (Cancel)")
        {
            X = 1,
            Y = Pos.AnchorEnd(1),          // stick to last content row
            Width = Dim.Fill() - 2,        // leave a 1-col margin on each side
            Height = 1,
            ColorScheme = scheme,
            CanFocus = false
        };

        // Multiline editor fills everything above the hint line
        var textView = new TextView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2,       // 1 for top margin + 1 for controlInfo
            ColorScheme = scheme,
            Text = initialText,
            WordWrap = true,
            CanFocus = true
        };

        textView.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Esc)
            {
                result = initialText;                 // canceled
                Terminal.Gui.Application.RequestStop();
                args.Handled = true;
            }
            else if (args.KeyEvent.Key == (Key.CtrlMask | Key.X)) // confirm
            {
                result = textView.Text.ToString();
                Terminal.Gui.Application.RequestStop();
                args.Handled = true;
            }
        };

        dialog.Add(textView, controlInfo);
        Terminal.Gui.Application.Run(dialog);

        return result ?? string.Empty;
    }
    private bool ConfirmationPrompt(ColorScheme scheme, string prompt)
    {
        bool result = false;

        int dialogWidth = 80;
        int dialogHeight = 15;
        int scrollbarMargin = 1;

        var dialog = new Window($"⚠  Confirmation")
        {
            X = margin.Length - 2,
            Y = Pos.Top(promptLabel) - dialogHeight,
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
            Text = prompt,
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
            Y = Pos.Top(promptLabel) - dialogHeight,
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
        _errorFrameTheme = Themes.Get($"error-{theme}-frame");
        _warningFrameTheme = Themes.Get($"warning-{theme}-frame");
        _successFrameTheme = Themes.Get($"success-{theme}-frame");
    }
    private void UpdateSchemes()
    {
        win.ColorScheme = _generalTheme.ToColorScheme();
        leftTopLabel.ColorScheme = _generalTheme.ToColorScheme();
        rightTopLabel.ColorScheme = _generalTheme.ToColorScheme();
        outputFrame.ColorScheme = _generalTheme.ToColorScheme();
        outputPane.ColorScheme = _generalTheme.ToColorScheme();
        helpFrame.ColorScheme = _helpTheme.ToColorScheme();
        helpPane.ColorScheme = _helpTheme.ToColorScheme();
        promptPane.ColorScheme = _generalTheme.ToColorScheme();
        promptLabel.ColorScheme = _generalTheme.ToColorScheme();
        footerPane.ColorScheme = _generalTheme.ToColorScheme();
        authLabel.ColorScheme = _generalTheme.ToColorScheme();

        outputPane.DrawContent += (_) =>
        {
            outputScrollBar.Size = outputPane.Lines;
            outputScrollBar.ColorScheme = _generalTheme.ToColorScheme();
            outputScrollBar.Position = outputPane.TopRow;
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
        rightTopLabel.Text = meta;
    }


}