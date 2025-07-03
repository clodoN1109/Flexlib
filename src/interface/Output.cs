namespace Flexlib.Interface;

public static class Output
{
    public static void ExplainUsage(string? usageInstructions = null)
    {

        if (usageInstructions != null)
            Console.WriteLine($"\n{usageInstructions}\n");
        else
            Console.WriteLine("\n Usage: flexlib {command} [options] \n");

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




