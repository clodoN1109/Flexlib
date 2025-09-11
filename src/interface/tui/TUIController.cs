using Terminal.Gui;
using Flexlib.Infrastructure.Processing;
using Flexlib.Application.UseCases;
using Flexlib.Application.Common;
using Flexlib.Infrastructure.Persistence;
using Flexlib.Infrastructure.Interop;
using Command = Flexlib.Interface.Input.Command;
using Flexlib.Interface.Input.Commands;
using Flexlib.Interface.Output;
using Flexlib.Infrastructure.Environment;
using Flexlib.Domain;

namespace Flexlib.Interface.TUI;

public partial class TUIApp : ITUIApp
{
    private void TUIController(Command cmd, bool isNotRecursiveCall)
    {
        _libRepo = new JsonLibraryRepository();

        if (cmd.IsSpecificHelp())
        {
            string outputStream = RunFlexlibExe(cmd);
            ActivateHelpFrame(outputStream.TranslateToProfile());
            return;
        }

        if (!cmd.IsValid())
        {
            RenderResult(Result.Fail("Invalid command usage. For details, run: <command name> help."));
            return;
        }

        DeactivateHelpFrame();

        string confirmationMessage;
        bodyPane.GetCurrentWidth(out int pageWidth);
        pageWidth.IfZeroFallbackTo(Env.GetSafeWindowWidth(), out pageWidth);
        switch (cmd)
        {
            // Special TUI commands
            case ExitCommand:
                ExitTUI();
                return;

            case HelpCommand:
                ActivateHelpFrame(TUIHelp.PromptUSage().TranslateToProfile());
                return;

            case ClearCommand:
                bodyPane.Text = "";
                helpPane.Text = "";
                DeactivateHelpFrame();
                return;

            case DarkModeCommand:
                UpdateThemes("dark");
                UpdateSchemes(_tui);
                return;

            case LightModeCommand:
                UpdateThemes("light");
                UpdateSchemes(_tui);
                return;

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
            case ListLibrariesCommand c:
                var libraryList = ListLibs.Execute(_libRepo);
                if (libraryList.Payload is List<Library> libs)
                    _page?.Update(
                        c,
                        _renderer.RenderLibrariesTable(libs, pageWidth, false).ToMultiRowString(),
                        "LIBRARIES",
                        $"",
                        $"",
                        $"",
                        $"{libs.Count.ToString()} libraries"
                    );
                else
                    RenderResult(Result.Fail($"Could not retrieve the requested list of {"libraries".TranslateToProfile()}."));
                break;

            case NewLibraryReportCommand c:
                _result = NewLibraryReport.Execute(c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case ListItemsCommand c:
                _result = ListItems.Execute(c.LibraryName, c.FilterSequence, c.SortSequence, c.ItemName, _libRepo);
                if (_result.Payload is ListItemsPayload payload)
                    _page?.Update(
                        c,
                        _renderer.RenderItemsTable(payload.Items,
                            payload.Library,
                            pageWidth, false).ToMultiRowString(),
                            "ITEMS",
                            $"{payload.Library.Name}/{payload.FilterSequence}/{string.Join('|', payload.ItemNameFilter.Where(n => n != "*").Select(n => n.IsCompound() ? $"'{n}'" : n))}",
                            $"{payload.SortSequence}",
                            string.Join("/", payload.Library.LayoutSequence.Select(p => p.Name)),
                            $"{payload.Items.Count} items {payload.LocalSizeInBytes:N2} bytes"
                    );
                break;
            case ListPropertiesCommand c:
                _result = ListProperties.Execute(c.LibName, c.ItemId, _libRepo);

                List<Components.ColoredRow> table = new();
                if (_result.Payload is Library lib)
                {
                    table = _renderer.RenderPropertyDefinitionsTable(lib, pageWidth, false);
                    _page?.Update(
                        c,
                        table.ToMultiRowString(),
                        "PROPERTIES",
                        $"{c.LibName}",
                        "",
                        "",
                        $"{lib.PropertyDefinitions.Count()} properties"
                    );
                }
                else if (_result.Payload is (Library library, LibraryItem item))
                {
                    table = _renderer.RenderItemPropertiesTable(item, library, pageWidth, false);
                    _page?.Update(
                        c,
                        table.ToMultiRowString(),
                        "PROPERTIES",
                        $"{c.LibName}/{item.Name}",
                        "",
                        $"Item ID {item.Id}",
                        $"{library.PropertyDefinitions.Count()} properties {item.PropertyValues.Count()} values"
                    );
                }
                else RenderResult(_result);
                break;

            case ListDesksCommand c:
                _result = ListDesks.Execute(c.LibraryName, _libRepo);
                if (_result.Payload is List<Desk> desks)
                    _page?.Update(
                        c,
                        _renderer.RenderDesksTable(desks, pageWidth, false).ToMultiRowString(),
                        "DESKS",
                        $"{c.LibraryName}",
                        "",
                        "",
                        $"{desks.Count()} desks"
                    );
                else RenderResult(_result);
                break;

            case ViewDeskCommand c:
                _result = ViewDesk.Execute(c.DeskId, c.LibraryName, c.SortSequence, _libRepo);
                if (_result.Payload is Desk desk)
                    _page?.Update(
                        c,
                        _renderer.RenderDeskItemsTable(desk, pageWidth, false).ToMultiRowString(),
                        "DESK ITEMS",
                        $"{c.LibraryName}/{desk.Name}",
                        $"{c.SortSequence}",
                        $"Desk ID: {c.DeskId}",
                        $"{desk.BorrowedItems.Count()} items"
                    );
                else RenderResult(_result);
                break;

            case ListLoansCommand c:
                _result = ListLoans.Execute(c.ItemId, c.LibraryName, _libRepo);
                if (_result.Payload is (LoanHistory loans, LibraryItem libItem))
                    _page?.Update(
                        c,
                        _renderer.RenderLoanHistoryTable(loans, libItem, c.LibraryName, pageWidth, false).ToMultiRowString(),
                        "LOAN HISTORY",
                        $"{c.LibraryName}/{libItem.Name}",
                        $"",
                        $"",
                        $"{loans.Entries.Count()} entries"
                    );
                else RenderResult(_result);
                break;

            case ListNotesCommand c:
                _result = ListNotes.Execute(c.ItemId, c.LibName, _libRepo);
                if (_result.Payload is (List<Note> notes, LibraryItem i))
                    _page?.Update(
                        c,
                        _renderer.RenderNoteTable(notes, i!.Name ?? "", i!.Id, c.LibName, pageWidth, false).ToMultiRowString(),
                        "NOTES",
                        $"{c.LibName}/{i.Name}",
                        $"",
                        $"",
                        $"{notes.Count()} notes"
                    );
                else RenderResult(_result);
                break;

            case NewLibraryCommand c:
                _result = NewLibrary.Execute(c.Name, c.Path, _libRepo);
                RenderResult(_result);
                break;

            case GetLibraryLayoutCommand c:
                _result = GetLibraryLayout.Execute(c.LibraryName, _libRepo);
                if (_result.Payload is List<string> layout)
                {
                    RenderResult(Result.Success($"{_renderer.RenderLayoutSequence(layout).ToMultilineString()}"));
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
                    _result = Result.Fail($"{"Library".TranslateToProfile()} named {c.Name} not found.");
                    RenderResult(_result);
                    break;
                }

                confirmationMessage = $"\nAre you sure you want to delete the {"library".TranslateToProfile()} '{c.Name}' at path:\n\n  {_selectedLibrary.Path} ?\n";
                if (!ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), confirmationMessage))
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
                    _result = Result.Fail($"{"Item".TranslateToProfile()} with ID {c.ItemId} not found in {"library".TranslateToProfile()} {c.LibraryName}.");
                    RenderResult(_result);
                    break;
                }

                confirmationMessage = $"\nAre you sure you want to delete the {"item".TranslateToProfile()} '{_selectedItem?.Name ?? ""}' from {"library".TranslateToProfile()} '{_selectedLibrary?.Name ?? ""}'?\n\n";
                if (!ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), confirmationMessage))
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
                                        Pos.Top(prompt) - _dialogHeight, _dialogHeight
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
                    Pos.Top(prompt) - _dialogHeight, _dialogHeight
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
                    confirmationMessage = $"\nAre you sure you want to delete note of ID {c.NoteId} from {"item".TranslateToProfile()} '{itemName ?? ""}' from {"library".TranslateToProfile()} '{c.LibName ?? ""}'?\n\n";
                    if (!ConfirmationPrompt(_selectedFrameTheme.ToColorScheme(), confirmationMessage))
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

            // Desks    
            case NewDeskCommand c:
                _result = NewDesk.Execute(c.DeskName, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case SetAppetiteCommand c:
                _result = SetAppetite.Execute(c.ItemID, c.DeskID, c.LibraryName, c.Date, _libRepo);
                RenderResult(_result);
                break;

            case SetProgressCommand c:
                _result = SetProgress.Execute(c.NewValue, c.ItemID, c.DeskID, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case DefineProgressCommand c:
                _result = DefineProgress.Execute(c.Unit, c.CompletionValue, c.ItemID, c.DeskID, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case SetPriorityCommand c:
                _result = SetPriority.Execute(c.NewPriority, c.ItemID, c.DeskID, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case RenameDeskCommand c:
                _result = RenameDesk.Execute(c.NewName, c.DeskID, c.LibraryName, _libRepo);
                RenderResult(_result);
                break;

            case BorrowItemCommand c:
                _result = BorrowItem.Execute(c.ItemId, c.DeskId, c.LibraryName, _user!.Id, _libRepo);
                RenderResult(_result);
                break;

            case ReturnItemCommand c:
                _result = ReturnItem.Execute(c.ItemId, c.DeskId, c.LibraryName, _user!.Id, _libRepo);
                RenderResult(_result);
                break;

            // Configurations
            case SetProfileCommand c:
                _result = SelectProfile.Execute(c.Name, _libRepo);
                RenderResult(_result);
                break;

            // Fallback to the CLI route
            default:
                string outputStream = RunFlexlibExe(cmd);
                _page?.Update(cmd, outputStream);
                break;
        }

        if (isNotRecursiveCall)
            RefreshBodyPane();

        _libRepo = new JsonLibraryRepository();

    }

}