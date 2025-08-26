using Flexlib.Infrastructure.Interop;
using Flexlib.Application.Ports;
using Flexlib.Infrastructure.Config;
using Flexlib.Interface;
using System.Management;

namespace Flexlib.Application.UseCases;

    public static class SelectProfile
    {
        
        public static Result Execute(string name, ILibraryRepository repo)
        {

            Result validation = IsOperationAllowed(name, repo);

            if (validation.IsSuccess)
            {
                return _SelectProfile(name, repo);
            }
            else 
            {
                return validation;
            }
                    
        }
        private static Result _SelectProfile(string name, ILibraryRepository repo)
        {
            FlexlibConfig config = repo.Config;
            Profile? selectedProfile = config.ProfileOptions.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (selectedProfile != null)
            {
                string currentProfile = config.SelectedProfile.Name;
                config.SelectedProfile = selectedProfile;
                if (repo.Save(config).IsSuccess)
                    return Result.Success($"Profile changed from {currentProfile} to {selectedProfile.Name}.");
            }
            return Result.Fail("Could not select the profile ");
                    
        }

        private static Result IsOperationAllowed(string name, ILibraryRepository repo)
        {
            if (!repo.Config.ProfileOptions.Any(p =>  p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return Result.Fail($"Profile named {name} not defined.");
            else if (repo.Config.SelectedProfile.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return Result.Warn($"Profile {name} already selected.");
            else
                return Result.Success("Operation allowed.");

        }

    }
