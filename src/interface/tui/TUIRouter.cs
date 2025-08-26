using Flexlib.Infrastructure.Processing;
using Flexlib.Interface.Input;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Interop;


namespace Flexlib.Interface.TUI;


public partial class TUIApp : ITUIApp
{
    private void TUIRouter(string input, bool isNotRecursiveCall)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        if (isNotRecursiveCall)
            AddToCommandHistory(input);

        var args = input.ToArrayOfStrings();
        string firstArg = args[0].ToLowerInvariant();

        // Forbidden input
        switch (firstArg)
        {
            case "gui":
            case "tui":
                bodyPane.Text = "Cannot create a nested interface. \nUse the CLI commands or available shortcuts instead.";
                return;
        }

        InputPreProcessing.Execute(args, out PreProcessingResult processed);
        if (!processed.IsValid || processed.Value is not Command cmd || !Command.IsKnownCommandName(cmd.Type))
        {
            RenderResult(Result.Fail($"Invalid command '{input.Trim()}'. \n\nFor the list of available commands, use 'help'."));
            return;
        }

        TUIController(cmd, isNotRecursiveCall);

        _libRepo = new JsonLibraryRepository();

    }

    private void TUIRouter(string input) => TUIRouter(input, true);
}