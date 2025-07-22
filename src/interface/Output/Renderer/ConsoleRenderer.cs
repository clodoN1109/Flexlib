using Flexlib.Domain;
using System.Text.Json;

namespace Flexlib.Interface.Output;

public class Renderer
{
    public string Message(string? message)
    {
        return $"\n{message}\n";
    }
    
    public string ExplainUsage(string? usageInstructions)
    {
        return usageInstructions != null
            ? $"\n{usageInstructions}\n"
            : "\n Usage: flexlib {command} [options] \n";
    }

    public string Success(string message) => $"\n✅ {message}\n";

    public string Failure(string message) => $"\n❌ {message}\n";

    public string Error(string message) => $"\nError: {message}\n";

    public List<string> FormatCommentTable(List<Comment> comments, int consoleWidth)
    {
        var output = new List<string>();

        if (comments == null || comments.Count == 0)
        {
            output.Add("No comments found.");
            return output;
        }

        int idWidth = Math.Min(Math.Max("ID".Length, comments.Max(c => c.Id.Length)) + 2, consoleWidth / 3);
        int textWidth = consoleWidth - idWidth;

        string largeDivider = new string('=', consoleWidth);
        string thinDivider = new string('-', consoleWidth);

        output.Add(largeDivider);
        output.Add($"{"ID".PadRight(idWidth)}{"Text".PadRight(textWidth)}");
        output.Add(largeDivider);

        foreach (var comment in comments)
        {
            string id = comment.Id.PadRight(idWidth);
            string text = comment.Text.Length > textWidth
                ? comment.Text[..(textWidth - 3)] + "..."
                : comment.Text.PadRight(textWidth);

            output.Add($"{id}{text}");
            output.Add(thinDivider);
        }

        return output;
    }

    public List<string> FormatItemTable(List<LibraryItem> items, int consoleWidth)
    {
        List<string> formattedTable = new List<string>();

        foreach (var item in items)
        {
            formattedTable.Add(item?.Name ?? "");
        }

        return formattedTable;
    }
    
    public List<string> FormatLibraryTable(List<Library> items, int consoleWidth)
    {
        List<string> formattedTable = new List<string>();

        foreach (var item in items)
        {
            formattedTable.Add(item?.Name ?? "");
        }

        return formattedTable;
    }

}


