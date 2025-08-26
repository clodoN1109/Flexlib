
using Flexlib.Interface.Input;

namespace Flexlib.Interface.Output;


public partial class ConsoleRenderer
{

    public AuthPromptScreen AuthPromptRender(int consoleWidth)
    {
        var lines = new List<Components.ColoredRow>();
        string title = "🟦 Welcome to Flexlib";
        string subtitle = "Please identify yourself to continue";

        // Layout sizes
        int innerWidth = Math.Min(consoleWidth - 8, 60);
        int paddingX = (consoleWidth - innerWidth) / 2;
        string padX = new(' ', paddingX);
        string horizontal = new('─', innerWidth - 2);

        // Content lines (excluding vertical centering)
        var boxLines = new List<Components.ColoredRow>();

        boxLines.Add(new Components.ColoredRow($"{padX}┌{horizontal}┐", ConsoleColor.DarkGray));
        boxLines.Add(new Components.ColoredRow($"{padX}│ {title.PadRight(innerWidth - 3)}│", ConsoleColor.Cyan));
        boxLines.Add(new Components.ColoredRow($"{padX}│ {subtitle.PadRight(innerWidth - 3)}│", ConsoleColor.Gray));
        boxLines.Add(new Components.ColoredRow($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));

        string idLabel = "🪪 ID:";
        string passLabel = "🔒 Password:";
        int labelPad = 4;
        string idLine = $"{new string(' ', labelPad)}{idLabel}";
        string passLine = $"{new string(' ', labelPad)}{passLabel}";

        int idX = paddingX + 1 + labelPad + idLabel.Length + 2; // +1 because of box border +2 to get distance from prompt text
        int idY = boxLines.Count;
        
        int passX = paddingX + 1 + labelPad + passLabel.Length + 2; // +1 because of box border +2 to get distance from prompt text
        boxLines.Add(new Components.ColoredRow($"{padX}│ {idLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));
        int passY = boxLines.Count;
        boxLines.Add(new Components.ColoredRow($"{padX}│ {passLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        boxLines.Add(new Components.ColoredRow($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));
        boxLines.Add(new Components.ColoredRow($"{padX}└{horizontal}┘", ConsoleColor.DarkGray));

        // Center vertically
        int consoleHeight = Console.WindowHeight;
        int verticalPad = Math.Max(0, (consoleHeight - boxLines.Count) / 2);

        for (int i = 0; i < verticalPad; i++)
            lines.Add(new Components.ColoredRow("", ConsoleColor.Gray)); // empty line for spacing

        lines.AddRange(boxLines);

        // Adjust Y positions after vertical padding
        return new AuthPromptScreen
        {
            Lines = lines,
            IDPosition = (idX, verticalPad + idY),
            PasswordPosition = (passX, verticalPad + passY)
        };
    }

    public RegistrationPromptScreen RegistrationPromptRender(int consoleWidth, int consoleHeight)
    {
        var lines = new List<Components.ColoredRow>();
        string title = "🟦 Welcome to Flexlib";
        string subtitle = "Please create your account";

        // Layout sizes
        int innerWidth = Math.Min(consoleWidth - 8, 60);
        int paddingX = (consoleWidth - innerWidth) / 2;
        string padX = new(' ', paddingX);
        string horizontal = new('─', innerWidth - 2);

        // Box content lines
        var boxLines = new List<Components.ColoredRow>();

        boxLines.Add(new($"{padX}┌{horizontal}┐", ConsoleColor.DarkGray));
        boxLines.Add(new($"{padX}│ {title.PadRight(innerWidth - 3)}│", ConsoleColor.Cyan));
        boxLines.Add(new($"{padX}│ {subtitle.PadRight(innerWidth - 3)}│", ConsoleColor.Gray));
        boxLines.Add(new($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));

        // Field labels
        int labelPad = 4;
        string nameLabel = "👤 Name:";
        string idLabel   = "🪪 ID:";
        string passLabel = "🔒 Password:";

        string nameLine = $"{new string(' ', labelPad)}{nameLabel}";
        string idLine   = $"{new string(' ', labelPad)}{idLabel}";
        string passLine = $"{new string(' ', labelPad)}{passLabel}";

        int nameX = paddingX + 1 + labelPad + nameLabel.Length + 2;
        int idX   = paddingX + 1 + labelPad + idLabel.Length + 2;
        int passX = paddingX + 1 + labelPad + passLabel.Length + 2;

        int nameY = boxLines.Count;
        boxLines.Add(new($"{padX}│ {nameLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        int idY = boxLines.Count;
        boxLines.Add(new($"{padX}│ {idLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        int passY = boxLines.Count;
        boxLines.Add(new($"{padX}│ {passLine.PadRight(innerWidth - 3)}│", ConsoleColor.White));

        boxLines.Add(new($"{padX}│{"".PadRight(innerWidth - 2)}│", ConsoleColor.Gray));
        boxLines.Add(new($"{padX}└{horizontal}┘", ConsoleColor.DarkGray));

        // Center vertically
        int verticalPad = Math.Max(0, (consoleHeight - boxLines.Count) / 2);

        for (int i = 0; i < verticalPad; i++)
            lines.Add(new("", ConsoleColor.Gray)); // blank padding

        lines.AddRange(boxLines);

        return new RegistrationPromptScreen
        {
            Lines = lines,
            NamePosition     = (nameX, verticalPad + nameY),
            IDPosition       = (idX, verticalPad + idY),
            PasswordPosition = (passX, verticalPad + passY)
        };
    }
}
