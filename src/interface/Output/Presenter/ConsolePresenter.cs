using Flexlib.Application.Ports; 
using Flexlib.Domain;
using Flexlib.Interface.CLI;
using Flexlib.Interface.Input;
using Flexlib.Infrastructure.Interop;
using Flexlib.Services.Media;
using Flexlib.Infrastructure.Config;
using Flexlib.Infrastructure.Environment;

namespace Flexlib.Interface.Output;

public class ConsolePresenter : IPresenter
{

    private readonly int WindowWidth = Env.GetSafeWindowWidth();

    private readonly ConsoleRenderer _renderer = new();
    private readonly ConsoleEmitter _emitter = new();
    private readonly IMediaService _mediaService = MediaServiceFactory.CreateDefault();

    public void Message(string message = "")
    {
        _emitter.PrintLines(_renderer.Message(message).Lines);
    }

    public void Result(Result result)
    {
        _emitter.PrintLines(_renderer.RenderResult(result).Lines, false);
    }

    public void ExplainUsage(UsageInfo info)
    {
        _emitter.PrintLines(_renderer.UsageInfo(info, WindowWidth));
    }

    public Result File(string filePath)
    {
        return _mediaService.TryOpenFile(filePath);
    }

    public void PresentLoanHistory(LoanHistory history, LibraryItem item, string libName)
    {
        _emitter.PrintLines(_renderer.FormatLoanHistoryTable(history, item, libName, WindowWidth));
    }

    public void UserInfo(string info)
    {
        _emitter.PrintLines(_renderer.UserInfo(info).Lines, false);
    }

    public void AvailableActions(List<string> actions)
    {
        _emitter.PrintLines(_renderer.AvailableActions(actions, WindowWidth), false);
    }

    public void ItemProperties(LibraryItem item, Library lib)
    {
        _emitter.PrintLines(_renderer.FormatItemPropertiesTable(item, lib, WindowWidth));
    }

    public void LibraryProperties(Library lib)
    {
        _emitter.PrintLines(_renderer.FormatPropertyDefinitionsTable(lib, WindowWidth));
    }

    public void AuthStatus(string message)
    {
        _emitter.PrintLines(_renderer.AuthStatus(message).Lines);
    }

    public void AuthPrompt(out AuthPromptScreen screen)
    {
        screen = _renderer.AuthPromptRender(WindowWidth);

        _emitter.PrintLines(screen.Lines);

        Console.SetCursorPosition(screen.IDPosition.X, screen.IDPosition.Y);
    }

    public void RegistrationPrompt(out RegistrationPromptScreen screen)
    {
        screen = _renderer.RegistrationPromptRender(WindowWidth);

        _emitter.PrintLines(screen.Lines);

        Console.SetCursorPosition(screen.IDPosition.X, screen.IDPosition.Y);
    }

    public void ShowError(string message)
    {
        _emitter.PrintLines(_renderer.Error(message).Lines);
    }

    public void ListNotes(List<Note> notes, string? itemName, string? libName)
    {
        var lines = _renderer.FormatNoteTable(notes, itemName ?? " ", libName ?? " ", WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ListDesks(List<Desk> desks, string? libName)
    {
        var lines = _renderer.FormatDesksTable(desks, libName ?? " ", WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ViewDesk(Desk desk, string? libName)
    {
        var lines = _renderer.FormatDeskTable(desk, libName ?? " ", WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ListItems(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, double localSizeInBytes, List<string> itemNameFilter)
    {
        var lines = _renderer.FormatItemTable(items, lib, filterSequence, sortSequence, localSizeInBytes, itemNameFilter, WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ListLibs(List<Library> libs)
    {
        var lines = _renderer.FormatLibraryTable(libs, WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ListLayoutSequence(List<string> layoutSequence)
    {
        if (layoutSequence.Count == 0)
        {
            Console.WriteLine("\nLayout sequence is empty.");
            return;
        }

        Console.WriteLine("\nlayout structure:");
        Console.WriteLine("\n📂");

        string indentUnit = "  ";
        for (int i = 0; i < layoutSequence.Count; i++)
        {
            string indent = string.Concat(Enumerable.Repeat(indentUnit, i));
            string symbol = i == layoutSequence.Count - 1 ? "└─" : "├─";
            Console.WriteLine($"{indent}{symbol} {layoutSequence[i]}");
        }

        Console.WriteLine();
    }

}

