namespace Flexlib.Interface.CLI;

public class CommandGroup
{
    public string Name { get; }
    public string Icon { get; }

    public CommandGroup(string name, string icon)
    {
        Name = name;
        Icon = icon;
    }
}

public static class CommandGroups
{

    public static readonly CommandGroup Help            = new("help"            , "?");
    public static readonly CommandGroup Authentication  = new("authentication"  , "!");
    public static readonly CommandGroup Libraries       = new("libraries"       , "🏛 ");
    public static readonly CommandGroup Items           = new("items"           , "🕮 ");
    public static readonly CommandGroup Comments        = new("comments"        , "𝒜");
    public static readonly CommandGroup Properties      = new("properties"      , "📐");
    public static readonly CommandGroup Storage         = new("storage"         , "🢃");
    public static readonly CommandGroup Interfaces      = new("interfaces"      , "🗔");

    public static IEnumerable<CommandGroup> All => new[]
    {
        Authentication,
        Libraries,
        Items,
        Comments,
        Properties,
        Storage,
        Interfaces
    };

    public static CommandGroup? FromName(string name)
    {
        return All.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

