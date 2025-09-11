using System.Text;

namespace Flexlib.Interface.TUI;
public static class TUIHelp
{
    public static string PromptUSage()
    {
        var commandGroups = new Dictionary<string, string[]>
        {
            ["Library Management"] = new[]
            {
                "list-libs", "new-lib", "remove-lib", "set-layout", "get-layout", "rebalance"
            },
            ["Item Management"] = new[]
            {
                "list-items", "new-item", "rename-item", "remove-item", "view-item", "set-prop", "update-origin", "get-origin"
            },
            ["Desk Management"] = new[]
            {
                "list-desks", "new-desk", "view-desk", "set-appetite", "set-priority", "define-progress", "set-progress"
            },
            ["Borrowing & Loans"] = new[]
            {
                "borrow-item", "return-item", "list-loans"
            },
            ["Notes"] = new[]
            {
                "list-notes", "new-note", "edit-note", "remove-note"
            },
            ["Properties"] = new[]
            {
                "list-props", "new-prop", "remove-prop", "rename-prop"
            },
            ["Miscellaneous"] = new[]
            {
                "fetch-files", "lib-report"
            },
            ["TUI"] = new[]
            {
                "exit", "dark", "light", "clear", "cls", "help"
            },
            ["Configurations"] = new[]
            {
                "set-profile"
            } ,
            ["Authentication"] = new[]
            {
                "login", "signup", "logout"
            }

        };

        var sb = new StringBuilder();

        sb.AppendLine("<command> [option ...]\n");
        sb.AppendLine("commands:\n");

        foreach (var group in commandGroups)
        {
            sb.AppendLine($"  {group.Key}:");
            sb.AppendLine($"    {string.Join(" | ", group.Value)}");
            sb.AppendLine();
        }

        string helpText = sb.ToString().TrimEnd();

        return helpText;
    }
}
