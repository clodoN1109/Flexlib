using Flexlib.Application.Ports; 
using Flexlib.Domain;

namespace Flexlib.Interface.Output;

public class ConsolePresenter : IPresenter
{
    private readonly Renderer _renderer = new();
    private readonly ConsoleEmitter _emitter = new();

    public void Message(string? message = null)
    {
        _emitter.Print(_renderer.Message(message));
    }

    public void ExplainUsage(string? usageInstructions = null)
    {
        _emitter.Print(_renderer.ExplainUsage(usageInstructions));
    }
    
    public void ExhibitUserInfo(IUser user)
    {
        _emitter.Print(_renderer.UserInfo(user));
    }

    public void Success(string message)
    {
        _emitter.Print(_renderer.Success(message), ConsoleColor.Green);
    }

    public void Failure(string message)
    {
        _emitter.Print(_renderer.Failure(message), ConsoleColor.Red);
    }

    public void ShowError(string message)
    {
        _emitter.Print(_renderer.Error(message));
    }

    public void ListComments(List<Comment> comments, string? itemName, string? libName)
    {
        var lines = _renderer.FormatCommentTable(comments, itemName ?? " ", libName ?? " ", Console.WindowWidth);
        _emitter.PrintLines(lines);
    }
    
    public void ListItems(List<LibraryItem> items, Library lib, string filterSequence, string sortSequence, double localSizeInBytes)
    {
        var lines = _renderer.FormatItemTable(items, lib, filterSequence, sortSequence, localSizeInBytes, Console.WindowWidth);
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

