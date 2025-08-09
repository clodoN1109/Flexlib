using Flexlib.Domain;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Interface.Output;

namespace Flexlib.Interface.Input;

public static class AuthPrompt
{
    private static readonly IReader _reader = new Reader();
    private static readonly IPresenter _presenter = new ConsolePresenter();

    public static UntrustedAccessInfo PromptUserIdentification()
    {
        _presenter.AuthPrompt(out AuthPromptScreen screen);

        Console.SetCursorPosition(screen.IDPosition.X, screen.IDPosition.Y);
        
        string id = Console.ReadLine()?.Trim() ?? "";

        Console.SetCursorPosition(screen.PasswordPosition.X, screen.PasswordPosition.Y);

        string password = _reader.ReadPassword("") ?? "";

        return new UntrustedAccessInfo(id, password);
    }

    public static UntrustedAccessInfo PromptUserRegistration()
    {
        _presenter.RegistrationPrompt(out RegistrationPromptScreen screen);

        Console.SetCursorPosition(screen.NamePosition.X, screen.NamePosition.Y);
        
        string name = Console.ReadLine()?.Trim() ?? "";
        
        Console.SetCursorPosition(screen.IDPosition.X, screen.IDPosition.Y);
        
        string id = Console.ReadLine()?.Trim() ?? "";

        Console.SetCursorPosition(screen.PasswordPosition.X, screen.PasswordPosition.Y);

        string password = _reader.ReadPassword("") ?? "";

        return new UntrustedAccessInfo(id, password, name);
    }

}

public class AuthPromptScreen
{
    public List<Components.ColoredLine> Lines { get; set; } = new();
    public (int X, int Y) IDPosition { get; set; }
    public (int X, int Y) PasswordPosition { get; set; }
}

public class RegistrationPromptScreen
{
    public List<Components.ColoredLine> Lines { get; set; } = new();
    public (int X, int Y) NamePosition { get; set; }
    public (int X, int Y) IDPosition { get; set; }
    public (int X, int Y) PasswordPosition { get; set; }
}

