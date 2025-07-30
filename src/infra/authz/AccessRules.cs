public static class ActionAccessRules
{
    
    public static Dictionary<string, AccessLevel> Rules => _rules;

    private static readonly Dictionary<string, AccessLevel> _rules = new()
    {
        ["help"]           = AccessLevel.User,
        ["new-user"]       = AccessLevel.User,
        ["new-lib"]        = AccessLevel.User,
        ["list-libs"]      = AccessLevel.User,
        ["remove-lib"]     = AccessLevel.User,
        ["set-layout"]     = AccessLevel.User,
        ["get-layout"]     = AccessLevel.User,
        ["new-item"]       = AccessLevel.User,
        ["list-items"]     = AccessLevel.User,
        ["remove-item"]    = AccessLevel.User,
        ["view-item"]      = AccessLevel.User,
        ["new-comment"]    = AccessLevel.User,
        ["list-comments"]  = AccessLevel.User,
        ["edit-comment"]   = AccessLevel.User,
        ["remove-comment"] = AccessLevel.User,
        ["new-prop"]       = AccessLevel.User,
        ["list-props"]     = AccessLevel.User,
        ["set-prop"]       = AccessLevel.User,
        ["remove-prop"]    = AccessLevel.User,
        ["fetch-files"]    = AccessLevel.User,
        ["gui"]            = AccessLevel.User
    };

    public static AccessLevel GetRequiredLevel(string actionName)
        => _rules.TryGetValue(actionName, out var level) ? level : AccessLevel.Root;
}

