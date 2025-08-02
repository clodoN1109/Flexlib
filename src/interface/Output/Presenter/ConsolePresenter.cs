using Flexlib.Application.Ports; 
using Flexlib.Domain;
using Flexlib.Interface.CLI;
using Flexlib.Interface.Input;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Interface.Output;

public class ConsolePresenter : IPresenter
{

    private readonly ConsoleRenderer _renderer = new();
    private readonly ConsoleEmitter _emitter = new();

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
        _emitter.PrintLines(_renderer.UsageInfo(info, Console.WindowWidth));
    }

    public void UserInfo(string info)
    {
        _emitter.PrintLines(_renderer.UserInfo(info).Lines, false);
    }

    public void AvailableActions(List<string> actions)
    {
        _emitter.PrintLines(_renderer.AvailableActions(actions, Console.WindowWidth), false);
    }

    public void AuthStatus(string message)
    {
        _emitter.PrintLines(_renderer.AuthStatus(message).Lines);
    }

    public void AuthPrompt(out AuthPromptScreen screen)
    {
        screen = _renderer.AuthPromptRender(Console.WindowWidth);

        _emitter.PrintLines(screen.Lines);

        Console.SetCursorPosition(screen.IDPosition.X, screen.IDPosition.Y);
    }

    public void RegistrationPrompt(out RegistrationPromptScreen screen)
    {
        screen = _renderer.RegistrationPromptRender(Console.WindowWidth);

        _emitter.PrintLines(screen.Lines);

        Console.SetCursorPosition(screen.IDPosition.X, screen.IDPosition.Y);
    }

    public void ShowError(string message)
    {
        _emitter.PrintLines(_renderer.Error(message).Lines);
    }

    public void ListComments(List<Comment> comments, string? itemName, string? libName)
    {
        var lines = _renderer.FormatCommentTable(comments, itemName ?? " ", libName ?? " ", Console.WindowWidth);
        _emitter.PrintLines(lines);
    }
    
    public void ListItems(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, double localSizeInBytes, List<string> itemNameFilter)
    {
        var lines = _renderer.FormatItemTable(items, lib, filterSequence, sortSequence, localSizeInBytes, itemNameFilter, Console.WindowWidth);
        _emitter.PrintLines(lines);
    }
    
    public void ListLibs(List<Library> libs)
    {
        var lines = _renderer.FormatLibraryTable(libs, Console.WindowWidth);
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
