using Terminal.Gui;
using Flexlib.Infrastructure.Processing;
using Flexlib.Interface.Input;
using Flexlib.Application.UseCases;
using Flexlib.Application.Common;
using Flexlib.Interface.CLI;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Interop;


namespace Flexlib.Interface.TUI;


public partial class TUIApp : ITUIApp
{
    private void TUIController(string input, bool isNotRecursiveCall)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        if (isNotRecursiveCall)
            AddToCommandHistory(input);

        var args = input.ToArrayOfStrings();
        string commandName = args[0].ToLowerInvariant();

        // Forbidden Commands
        switch (commandName)
        {
            case "gui":
            case "tui":
                pagePane.Text = "Cannot create a nested interface. \nUse the CLI commands or available shortcuts instead.";
                return;
        }

        //Special TUI commands
        switch (commandName)
        {
            case "exit":
                ExitTUI();
                return;

            case "help":
                ActivateHelpFrame(TUIHelp.PromptUSage());
                return;

            case "cls":
            case "clear":
                pagePane.Text = "";
                helpPane.Text = "";
                DeactivateHelpFrame();
                return;

            case "dark":
                UpdateThemes("dark");
                UpdateSchemes();
                return;

            case "light":
                UpdateThemes("light");
                UpdateSchemes();
                return;

        }

        // CLI commands
        InputPreProcessing.Execute(input.ToArrayOfStrings(), out PreProcessingResult processed);

        if (!processed.IsValid || processed.Value is not Input.Command cmd || !Input.Command.IsKnownCommandName(commandName))
        {
            RenderResult(Result.Fail($"Invalid command '{input.Trim()}'. \n\nFor the list of available commands, use 'help'.")); return;
        }

        if (cmd.IsSpecificHelp())
        {
            string outputStream = RunFlexlib(args);
            ActivateHelpFrame(outputStream);
            return;
        }

        if (!cmd.IsValid())
        {
            RenderResult(Result.Fail("Invalid command usage. For details, run: <command name> help."));
            return;
        }

        DeactivateHelpFrame();

        string promptMessage;
        switch (cmd)
        {
            // Auth
            case LoginCommand:
                _finalAction = "login";
                ExitTUI();
                return;

            case SignUpCommand:
                _finalAction = "signup";
                ExitTUI();
                return;

            case LogoutCommand:
                _auth.Logout();
                ExitTUI();
                return;
            
            // Libraries
            case NewLibraryCommand c:
                _result = NewLibrary.Execute(c.Name, c.Path, _libRepo);
                RenderResult(_result);
                break;

            case GetLibraryLayoutCommand c:
                _result = GetLibraryLayout.Execute(c.LibraryName, _libRepo);
                if (_result.Payload is List<string> layout)
                {
                    RenderResult( Result.Success($"{_renderer.RenderLayoutSequence(layout).ToMultilineString()}" ));
                }
                else
                {
                    RenderResult(_result);
                }
                return;

            case SetLibraryLayoutCommand c:
                _result = SetLibraryLayout.Execute(c.LibraryName, c.LayoutString, _libRepo);
                RenderResult(_result);
                break;

            case RemoveLibraryCommand c:
                _selectedLibrary = _libRepo.GetByName(c.Name)!;
                if (_selectedLibrary == null)
                {
                    _result = Result.Fail($"Library named {c.Name} not found.");
                    RenderResult(_result);
                    break;
                }

                promptMessage = $"\nAre you sure you want to delete the library '{c.Name}' at path:\n\n  {_selectedLibrary.Path} ?\n";
                if (!ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), promptMessage))
                    break;
                _result = RemoveLibrary.Execute(c.Name, _libRepo);
                RenderResult(_result);
                break;

            // Items
            case NewItemCommand c:
                _result = NewItem.Execute(c.LibraryName, c.ItemOrigin, c.ItemName, _libRepo);
                RenderResult(_result);
                break;

            case RenameItemCommand c:
                _result = RenameItem.Execute(c.ItemId, c.NewName, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case ViewItemCommand c:
                _result = GetItemLocalCopy.Execute(c.ItemId, c.LibraryName, c.Application, _libRepo);
                if (_result.Payload is string filePath)
                    _result = _mediaService.TryOpenFile(filePath);
                RenderResult(_result);
                break;

            case GetItemOriginCommand c:
                _result = GetItemOrigin.Execute(c.ItemId, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case UpdateItemOriginCommand c:
                _result = UpdateItemOrigin.Execute(c.ItemId, c.NewOrigin, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case RemoveItemCommand c:
                _selectedLibrary = _libRepo.GetByName(c.LibraryName)!;
                _selectedItem = _selectedLibrary.GetItemById(c.ItemId);
                if (_selectedItem == null)
                {
                    _result = Result.Fail($"Item with ID {c.ItemId} not found in library {c.LibraryName}.");
                    RenderResult(_result);
                    break;
                }

                promptMessage = $"\nAre you sure you want to delete the item '{_selectedItem?.Name ?? ""}' from library '{_selectedLibrary?.Name ?? ""}'?\n\n";
                if (!ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), promptMessage))
                    break;
                _result = RemoveItem.Execute(c.ItemId, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            // Notes
            case EditNoteCommand c:

                _result = SelectItemNote.Execute(c.ItemId, c.NoteId, c.LibName, _libRepo);

                if ((_result.Payload is Domain.Note currentNote) && !string.IsNullOrWhiteSpace(currentNote.Text))
                {
                    itemName = _libRepo.GetByName(c.LibName)?.GetItemById(c.ItemId)?.Name;
                    _result = RenderTUITextReader(_selectedFrameTheme.ToColorScheme(),
                                        $"{c.LibName}/{(itemName?.IsCompound() ?? false ? $"\'{itemName}\'" : itemName) ?? c.ItemId} | Note {c.NoteId}",
                                        currentNote.Text,
                                        margin,
                                        margin.Length - 2,
                                        Pos.Top(promptLabel) - _dialogHeight, _dialogHeight
                                        );
                }
                if (_result?.IsSuccess ?? false)
                    _result = EditNote.Execute(c.ItemId, c.NoteId, c.LibName, (string)(_result?.Payload ?? ""), _libRepo);
                else
                {
                    _result = Result.Warn("Note editing canceled.");
                }
                RenderResult(_result);
                break;

            case NewNoteCommand c:
                itemName = _libRepo.GetByName(c.LibName)?.GetItemById(c.ItemId)?.Name;
                _result = RenderTUITextReader(_selectedFrameTheme.ToColorScheme(),
                    $"{c.LibName}/{(itemName?.IsCompound() ?? false ? $"\'{itemName}\'" : itemName) ?? c.ItemId}",
                    "",
                    margin,
                    margin.Length - 2,
                    Pos.Top(promptLabel) - _dialogHeight, _dialogHeight
                    );
                if (_result?.IsSuccess ?? false)
                    _result = NewNote.Execute(c.ItemId, c.LibName, (string)(_result?.Payload ?? ""), _user!, _libRepo);
                else
                {
                    _result = Result.Warn("Note creation canceled.");
                }
                RenderResult(_result);
                break;

            case RemoveNoteCommand c:
                _result = SelectItemNote.Execute(c.ItemId, c.NoteId, c.LibName, _libRepo);

                if ((_result.Payload is Domain.Note targetNote) && !string.IsNullOrWhiteSpace(targetNote.Text))
                {
                    itemName = _libRepo.GetByName(c.LibName)?.GetItemById(c.ItemId)?.Name;
                    promptMessage = $"\nAre you sure you want to delete note of ID {c.NoteId} from item '{itemName ?? ""}' from library '{c.LibName ?? ""}'?\n\n";
                    if (!ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), promptMessage))
                        _result = Result.Warn("Note deletion canceled.");
                    else
                    {
                        _result = RemoveNote.Execute(c.ItemId, c.NoteId, c.LibName!, _libRepo);
                    }
                }
                RenderResult(_result);
                break;

            // Properties
            case NewPropertyCommand c:
                _result = NewProperty.Execute(c.LibName, c.PropName, c.PropType, _libRepo);
                RenderResult(_result);
                break;

            case SetPropertyCommand c:
                _result = SetProperty.Execute(c.PropName, c.NewValue, c.LibName, c.ItemId, _libRepo);
                RenderResult(_result);
                break;

            case RenamePropertyCommand c:
                _result = RenameProperty.Execute(c.PropName, c.NewName, c.LibName, _libRepo);
                RenderResult(_result);
                break;

            case UnsetPropertyCommand c:
                _result = UnsetProperty.Execute(c.PropName, c.TargetValue, c.LibName, c.ItemId, _libRepo);
                RenderResult(_result);
                break;

            case RemovePropertyCommand c:
                _result = RemoveProperty.Execute(c.PropName, c.LibName, _libRepo);
                RenderResult(_result);
                break;

            // Local Storage
            case FetchFilesCommand c:
                _result = FetchFiles.Execute(c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case RebalanceLocalStorageCommand c:
                _result = RebalanceLocalStorage.Execute(c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            // Temporary redirect to the CLI controller
            default:
                string outputStream = RunFlexlib(args);
                _page.Update(outputStream, input);
                DeactivateHelpFrame();
                break;
        }

        if (isNotRecursiveCall)
            UpdatePagePane();

        _libRepo = new JsonLibraryRepository();

    }

    private void TUIController(string input) => TUIController(input, true);
}