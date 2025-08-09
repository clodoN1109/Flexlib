using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;



public abstract class NoteCommand : Command
{
    public object ItemId { get; } 

    protected NoteCommand(string[] options)
    {
        Options = options;
        ItemId = options.Length > 0 ? options[0] : "";
    }

}

public class NewNoteCommand : NoteCommand
{
    public string? Note { get; set; }
    public string LibName { get; set; }

    public NewNoteCommand(string[] options) : base(options) 
    {
        LibName = options.Length > 1 ? options[1] : "Default Library";
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
            Syntax = "flexlib new-note <item id> [library name] [note]" ,
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
                    DefaultValue = "Default Library" 
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
    public string LibName { get; set; }
    
    public ListNotesCommand(string[] options) : base(options) 
    {
        LibName = options.Length > 1 ? options[1] : "Default Library";
    }

    public override string Type => "list-notes";

    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length < 3;
    }
    
    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "list-notes",
            Description = "List all notes from a selected library item.",
            Group = CommandGroups.Notes,
            Syntax = "flexlib list-notes <item id> [library name]",
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
                    DefaultValue = "Default Library" 
                }

            }
        };
    }


}

public class EditNoteCommand : NoteCommand
{
    public string NoteId;
    public string LibName { get; set; }

    public EditNoteCommand(string[] options) : base(options) { 
        
        NoteId = options.Length > 1 ? options[1] : "";
        LibName = options.Length > 2 ? options[2] : "Default Library"; 
   
    }
    
    public override string Type => "edit-note";

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "edit-note",
            Description = "Edit a selected commment.",
            Group = CommandGroups.Notes,
            Syntax = "flexlib edit-note <item id> <note id> [library name]",
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
                    DefaultValue = "Default Library" 
                }

            }
        };
    }

}

public class RemoveNoteCommand : NoteCommand
{
    public string NoteId;
    public string LibName { get; set; }

    public RemoveNoteCommand(string[] options) : base(options) { 
        
        NoteId = options.Length > 1 ? options[1] : "";
        LibName = options.Length > 2 ? options[2] : "Default Library"; 
   
    }
    
    public override string Type => "remove-note";

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override CommandUsageInfo GetUsageInfo()
    {
        return new CommandUsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-note",
            Description = "Remove a note from a selected item.",
            Group = CommandGroups.Notes,
            Syntax = "flexlib remove-note <item id> <note id> [library name]",
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
                    DefaultValue = "Default Library" 
                }

            }
        };
    }

}

