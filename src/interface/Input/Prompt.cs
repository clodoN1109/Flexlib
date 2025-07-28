using Flexlib.Domain;
using Flexlib.Interface.Output;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Authentication;

namespace Flexlib.Interface.Input;

public static class AuthPrompt
{
    private static readonly IReader _reader = new Reader();

    public static UntrustedAccessInfo PromptUserIdentification()
    {
        Console.Write("Enter User ID: ");
        string id = Console.ReadLine()?.Trim() ?? "";

        string password = _reader.ReadPassword("Enter Password: ") ?? "";

        return new UntrustedAccessInfo(id, password);
    }
}
