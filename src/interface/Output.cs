using Flexlib.Application.Ports;
using Flexlib.Domain;

namespace Flexlib.Interface;

public class Output : IOutput
{
    public void ExplainUsage(string? usageInstructions = null)
    {

        if (usageInstructions != null)
            Console.WriteLine($"\n{usageInstructions}\n");
        else
            Console.WriteLine("\n Usage: flexlib {command} [options] \n");

    }

    public void Success(string? message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✅ {message}\n");
        Console.ResetColor();
    }

    public void Failure(string? message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n❌ {message}\n");
        Console.ResetColor();
    }

    public void ShowError(string message) 
    { 
        Console.WriteLine($"\nError: {message}\n");
    }


    public void ListComments(List<Comment> comments)
    {
        if (comments == null || comments.Count == 0)
        {
            Console.WriteLine("No comments found.");
            return;
        }

        int consoleWidth = Console.WindowWidth;

        // Determine column widths with minimum spacing and max cap to avoid overflow
        int idWidth = Math.Min(Math.Max("ID".Length, comments.Max(c => c.Id.Length)) + 2, consoleWidth / 3);
        int textWidth = consoleWidth - idWidth;

        string divider = new string('-', consoleWidth);

        // Header
        Console.WriteLine(divider);
        Console.WriteLine($"{"ID".PadRight(idWidth)}{"Text".PadRight(textWidth)}");
        Console.WriteLine(divider);

        // Rows
        foreach (var comment in comments)
        {
            string id = comment.Id.PadRight(idWidth);
            string text = comment.Text.Length > textWidth
                ? comment.Text.Substring(0, textWidth - 3) + "..."
                : comment.Text.PadRight(textWidth);

            Console.WriteLine($"{id}{text}\n");
        }

        Console.WriteLine(divider);
    }


}




