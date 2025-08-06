public static class ActionAccessRules
{
    
    public static Dictionary<string, AccessLevel> Rules => _rules;

    private static readonly Dictionary<string, AccessLevel> _rules = new()
    {
        ["help"]            = AccessLevel.Public,
        ["signup"]          = AccessLevel.Public,
        ["login"]           = AccessLevel.Public,
        ["logout"]          = AccessLevel.Public,
        ["new-lib"]         = AccessLevel.User,
        ["list-libs"]       = AccessLevel.User,
        ["remove-lib"]      = AccessLevel.User,
        ["set-layout"]      = AccessLevel.User,
        ["get-layout"]      = AccessLevel.User,
        ["new-item"]        = AccessLevel.User,
        ["rename-item"]     = AccessLevel.User,
        ["update-origin"]   = AccessLevel.User,
        ["list-items"]      = AccessLevel.User,
        ["remove-item"]     = AccessLevel.User,
        ["view-item"]       = AccessLevel.User,
        ["new-note"]     = AccessLevel.User,
        ["list-notes"]   = AccessLevel.User,
        ["edit-note"]    = AccessLevel.User,
        ["remove-note"]  = AccessLevel.User,
        ["new-prop"]        = AccessLevel.User,
        ["rename-prop"]     = AccessLevel.User,
        ["list-props"]      = AccessLevel.User,
        ["set-prop"]        = AccessLevel.User,
        ["unset-prop"]      = AccessLevel.User,
        ["remove-prop"]     = AccessLevel.User,
        ["fetch-files"]     = AccessLevel.User,
        ["rebalance"]       = AccessLevel.User,
        ["gui"]             = AccessLevel.Public
    };

    public static AccessLevel GetRequiredLevel(string actionName)
        => _rules.TryGetValue(actionName, out var level) ? level : AccessLevel.Dev;
}

