using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Persistence;


namespace Flexlib.Interface;

public class Profile
{
    public string Name { get; set; } = string.Empty;

    // A profile is considered default if it's the "Library" one
    [JsonIgnore]
    public bool IsDefault => string.Equals(Name, "library", StringComparison.OrdinalIgnoreCase);

    // Map from source word → target word
    public Dictionary<string, string> Translator { get; set; } = new();

    public Profile(string name = "")
    {
        Name = name;
    }
}

public static class ProfileExtensions
{
    /// <summary>
    /// Translates all occurrences of profile-specific terms in a string.
    /// </summary>
    public static string TranslateToProfile(this string input)
    {
        Profile profile = new JsonLibraryRepository().Config.SelectedProfile;
        if (string.IsNullOrEmpty(input) || profile?.Translator == null || profile.IsDefault)
            return input;

        string result = input;

        foreach (var kvp in profile.Translator)
        {
            // Replace whole words only, case-insensitive
            string pattern = $@"{Regex.Escape(kvp.Key)}";
            result = Regex.Replace(result, pattern, match =>
            {
                string replacement = kvp.Value;
                string original = match.Value;

                // Preserve casing rules
                if (string.Equals(original, original.ToUpper(), StringComparison.Ordinal))
                    return replacement.ToUpper(); // ALL CAPS
                if (string.Equals(original, original.ToLower(), StringComparison.Ordinal))
                    return replacement.ToLower(); // all lower
                if (char.IsUpper(original[0]) && original.Skip(1).All(char.IsLower))
                    return char.ToUpper(replacement[0]) + replacement.Substring(1).ToLower(); // Capitalized
                return replacement; // mixed or default -> just return as-is
            }, RegexOptions.IgnoreCase);

        }

        return result;
    }

    public static List<string> TranslateToProfile(this List<string> input)
    {
        return [.. input.Select(x => x.TranslateToProfile())]; 
    }

    public static string TranslateToDefault(this string input)
    {
        Profile profile = new JsonLibraryRepository().Config.SelectedProfile;
        if (string.IsNullOrEmpty(input) || profile?.Translator == null || profile.IsDefault)
            return input;

        string result = input;

        // Invert the dictionary: profile term → default term
        foreach (var kvp in profile.Translator)
        {
            string profileWord = kvp.Value;
            string defaultWord = kvp.Key;

            // Match substring, not just whole words
            string pattern = Regex.Escape(profileWord);

            result = Regex.Replace(result, pattern, match =>
            {
                string replacement = defaultWord;
                string original = match.Value;

                if (string.Equals(original, original.ToUpper(), StringComparison.Ordinal))
                    return replacement.ToUpper();
                if (string.Equals(original, original.ToLower(), StringComparison.Ordinal))
                    return replacement.ToLower();
                if (char.IsUpper(original[0]) && original.Skip(1).All(char.IsLower))
                    return char.ToUpper(replacement[0]) + replacement.Substring(1).ToLower();

                return replacement;
            }, RegexOptions.IgnoreCase);
        }

        return result;
    }
}

public static class Profiles
{

    public static List<Profile> AllProfileOptions => new List<Profile>()
    {
        LibraryProfile,
        ProjectProfile
    };

    // Default "Library" profile: no translations
    public static readonly Profile LibraryProfile = new Profile("library")
    {
        Translator = new Dictionary<string, string>()
    };

    // Example alternative profile
    public static readonly Profile ProjectProfile = new Profile("project")
    {
        Translator = new Dictionary<string, string>
        {
            { "libraries", "projects" },
            { "library", "project" },
            { "lib", "proj" },
            { "item", "task" },
            { "desk", "plan" },
            { "borrow", "select"}
        }
    };
}
