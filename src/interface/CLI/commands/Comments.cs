using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Modelling;
using Flexlib.Infrastructure.Environment;
using Flexlib.Interface.Input.Heuristics;
using System.IO;
using Flexlib.Interface.Input;
using System.Collections.Generic;

namespace Flexlib.Interface.CLI;



public abstract class CommentCommand : Command
{
    public object ItemId { get; } 

    protected CommentCommand(string[] options)
    {
        Options = options;
        ItemId = options.Length > 0 ? options[0] : "";
    }

}

public class NewCommentCommand : CommentCommand
{
    public string? Comment { get; set; }
    public string LibName { get; set; }

    public NewCommentCommand(string[] options) : base(options) 
    {
        LibName = options.Length > 1 ? options[1] : "Default Library";
        Comment = options.Length > 2 ? options[2] : "";
    }
    
    public override string Type => "new-comment";
    
    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length <= 3;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "new-comment",
            Description = "Creates a new comment for the selected library item.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib new-comment <item id> [library name] [comment]" ,
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                },
                new CommandOption{
                    Name = "comment",
                    OptionDomain = new VariableDomain()
                }
            }
        };
    }


}

public class ListCommentsCommand : CommentCommand
{
    public string LibName { get; set; }
    
    public ListCommentsCommand(string[] options) : base(options) 
    {
        LibName = options.Length > 1 ? options[1] : "Default Library";
    }

    public override string Type => "list-comments";

    public override bool IsValid()
    {
        return Options.Length > 0 && Options.Length < 3;
    }
    
    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "list-comments",
            Description = "List all comments from a selected library item.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib list-comments <item id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },

                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
    }


}

public class EditCommentCommand : CommentCommand
{
    public string CommentId;
    public string LibName { get; set; }

    public EditCommentCommand(string[] options) : base(options) { 
        
        CommentId = options.Length > 1 ? options[1] : "";
        LibName = options.Length > 2 ? options[2] : "Default Library"; 
   
    }
    
    public override string Type => "edit-comment";

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "edit-comment",
            Description = "Edit a selected commment.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib edit-comment <item id> <comment id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "comment id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
    }

}

public class RemoveCommentCommand : CommentCommand
{
    public string CommentId;
    public string LibName { get; set; }

    public RemoveCommentCommand(string[] options) : base(options) { 
        
        CommentId = options.Length > 1 ? options[1] : "";
        LibName = options.Length > 2 ? options[2] : "Default Library"; 
   
    }
    
    public override string Type => "remove-comment";

    public override bool IsValid()
    {
        return Options.Length > 1 && Options.Length < 4;
    }

    public override UsageInfo GetUsageInfo()
    {
        return new UsageInfo
        {
            Meta = new List<string> {},
            Title = "remove-comment",
            Description = "Remove a comment from a selected item.",
            Group = CommandGroups.Comments,
            Syntax = "flexlib remove-comment <item id> <comment id> [library name]",
            Options = new List<CommandOption>
            {
                new CommandOption{
                    Name = "item id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "comment id",
                    OptionDomain = new VariableDomain(),
                    Mandatory = true
                },
                new CommandOption{
                    Name = "library name",
                    OptionDomain = new VariableDomain(),
                    DefaultValue = "Default Library" 
                }

            }
        };
    }

}

