namespace Flexlib.Interface;

public static class Output
{
    public static void ExplainUsage(ParsedInput input)
    {
        switch (input) 
        {
            case NewLibraryCommand newlib:
                Console.WriteLine("\n Usage: flexlib new {name} {path}\n");
                break;

            default: 
                Console.WriteLine("\n Usage: flexlib [options] \n");
                break;
        }

    }

    public static void Success(string? message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✅ {message}\n");
        Console.ResetColor();
    }

    public static void Failure(string? message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n❌ {message}\n");
        Console.ResetColor();
    }

    public static void ShowError(string msg) 
    { 
        Console.WriteLine("\nError: {error message}\n");
    }
}




