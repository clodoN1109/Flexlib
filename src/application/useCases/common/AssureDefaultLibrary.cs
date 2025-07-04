using Flexlib.Common;
using Flexlib.Application.UseCases;
using Flexlib.Application.Ports;

namespace Flexlib.Application.UseCases.Common;

public static class AssureDefaultLibrary
{
        public static Result Execute(ILibraryRepository repo)
        {
            string? assemblyLocation = Env.GetExecutingAssemblyLocation();
            
            if (assemblyLocation == null)
                return Result.Fail("Could not determine Flexlib data folder path.");

            string flexlibDataFolder = Path.Combine(assemblyLocation, "data"); 

            NewLibrary.Execute("Default Library", flexlibDataFolder, repo);

            return Result.Success("Default library existence assured.");
        }
}


