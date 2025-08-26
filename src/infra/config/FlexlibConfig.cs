using Flexlib.Interface;

namespace Flexlib.Infrastructure.Config;

public class FlexlibConfig
{
    public int MaxFilesPerFolder { get; set; } = 100;

    // Start with the default profile by default
    public Profile SelectedProfile { get; set; } = Profiles.LibraryProfile;
    public List<Profile> ProfileOptions { get; set; } = Profiles.AllProfileOptions;

    public void ResetToDefault()
    {
        ProfileOptions = Profiles.AllProfileOptions;
    }
    public FlexlibConfig() { }
}

