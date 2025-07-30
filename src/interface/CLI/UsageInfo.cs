namespace Flexlib.Interface.CLI;

public class UsageInfo
{
    public List<string> Meta { get; set; } = new();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public CommandGroup Group { get; set; } = new("", "");
    public string Syntax { get; set; } = "";
    public List<string> Examples { get; set; } = new();
    public List<CommandOption> Options { get; set; } = new();
}

