using Flexlib.Application.Ports; 
using Flexlib.Domain;
using Flexlib.Interface.Input;
using Flexlib.Infrastructure.Interop;
using Flexlib.Services.Media;
using Flexlib.Infrastructure.Environment;
using System.Drawing;

namespace Flexlib.Interface.Output;

public class ConsolePresenter : IPresenter
{
    private readonly int WindowWidth = Env.GetSafeWindowWidth();
    private readonly int WindowHeight = Env.GetSafeWindowHeight();

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
        _emitter.PrintLines(_renderer.RenderLoanHistoryTable(history, item, libName, WindowWidth));
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
        _emitter.PrintLines(_renderer.RenderItemPropertiesTable(item, lib, WindowWidth));
    }

    public void LibraryProperties(Library lib)
    {
        _emitter.PrintLines(_renderer.RenderPropertyDefinitionsTable(lib, WindowWidth));
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
        screen = _renderer.RegistrationPromptRender(WindowWidth, WindowHeight);

        _emitter.PrintLines(screen.Lines);

        Console.SetCursorPosition(screen.IDPosition.X, screen.IDPosition.Y);
    }

    public void ShowError(string message)
    {
        _emitter.PrintLines(_renderer.Error(message).Lines);
    }

    public void ListNotes(List<Note> notes, string? itemName, int itemId, string? libName)
    {
        var lines = _renderer.RenderNoteTable(notes, itemName ?? " ", itemId, libName ?? " ", WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ListDesks(List<Desk> desks, string? libName)
    {
        var lines = _renderer.RenderDesksTable(desks, WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ViewDesk(Desk desk, string? libName)
    {
        var lines = _renderer.RenderDeskItemsTable(desk, WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ListItems(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, double localSizeInBytes, List<string> itemNameFilter)
    {
        var lines = _renderer.RenderItemsPage(items, lib, filterSequence, sortSequence, localSizeInBytes, itemNameFilter, WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void ListLibs(List<Library> libs)
    {
        var lines = _renderer.RenderLibrariesPage(libs, WindowWidth);
        _emitter.PrintLines(lines);
    }

    public void PresentLayoutSequence(List<string> layoutSequence)
    {
        var lines = _renderer.RenderLayoutSequence(layoutSequence);
        _emitter.PrintLines(lines, ConsoleColor.Blue, false);
    }

}

