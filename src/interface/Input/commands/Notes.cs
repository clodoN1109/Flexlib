using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.Input.Commands;


public abstract class NoteCommand : Command
{
    public object ItemId { get; } 
    public string LibName { get; set; }

    protected NoteCommand(string[] options)
    {
        Options = options;
        LibName = options.Length > 0 ? options[0] : "";
        ItemId  = options.Length > 1 ? options[1] : "";
    }

}

public class NewNoteCommand : NoteCommand
{
    public string? Note { get; set; }
    public NewNoteCommand(string[] options) : base(options) 
    {
        Note = options.Length > 2 ? options[2] : "";
    }
    
    public override string Type => "new-note";
    
    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length <= 3;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "new-note",
            Description = "Creates a new note for the selected library item.",
            Group = CommandGroups.Notes,
            Syntax = "new-note <library name> <item id> [note]" ,
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option{
                    Name = "note",
                    OptionDomain = new VariableDomain()
                }
            }
        };
    }


}

public class ListNotesCommand : NoteCommand
{
    
    public ListNotesCommand(string[] options) : base(options) 
    {
    }

    public override string Type => "list-notes";

    public override bool IsValid()
    {
        return Options.Length == 2;
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "list-notes",
            Description = "List all notes from a selected library item.",
            Group = CommandGroups.Notes,
            Syntax = "list-notes <library name> <item id>",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }

            }
        };
    }


}

public class EditNoteCommand : NoteCommand
{
    public string NoteId;
    public EditNoteCommand(string[] options) : base(options) { 
        
        NoteId = options.Length > 2 ? options[2] : "";   
    }
    
    public override string Type => "edit-note";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "edit-note",
            Description = "Edit a selected commment.",
            Group = CommandGroups.Notes,
            Syntax = "edit-note <library name> <item id> <note id>",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option{
                    Name = "note id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }

            }
        };
    }

}

public class RemoveNoteCommand : NoteCommand
{
    public string NoteId;
    public RemoveNoteCommand(string[] options) : base(options) { 
        
        NoteId = options.Length > 2 ? options[2] : "";
   
    }
    
    public override string Type => "remove-note";

    public override bool IsValid()
    {
        return Options.Length == 3;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-note",
            Description = "Remove a note from a selected item.",
            Group = CommandGroups.Notes,
            Syntax = "remove-note <library name> <item id> <note id>",
            Options = new List<Option>
            {
                new Option{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option{
                    Name = "note id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new Option{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                }

            }
        };
    }

}

