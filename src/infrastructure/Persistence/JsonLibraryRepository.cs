using Flexlib.Application.Ports;
using Flexlib.Domain;
using Flexlib.Common;
using Flexlib.Infrastructure.Persistence.Common;
using System.IO;

namespace Flexlib.Infrastructure.Persistence;

public class JsonLibraryRepository : ILibraryRepository
{
    private readonly string _dataDirectory;
    private readonly string _metaFile;
    private List<Library> _cache;
    private string? ExeFolder;
    private string? AppDataFolder;

    public JsonLibraryRepository()
    {
        ExeFolder = Env.GetExecutingAssemblyLocation();
        if (ExeFolder == null || !Directory.Exists(ExeFolder))
            throw new DirectoryNotFoundException($"Executable file directory not found: {ExeFolder}");

        AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (AppDataFolder == null || !Directory.Exists(AppDataFolder))
            throw new DirectoryNotFoundException($"AppData file directory not found: {AppDataFolder}");
        
        _dataDirectory = EnsureDataDirectory();

        _metaFile = EnsureMetaFile();

        _cache = LoadCache(_metaFile);

    }

    private List<Library> LoadCache(string metaFile) {
       
        List<Library> cache;

        if (File.Exists(metaFile))
        {
            var json = File.ReadAllText(metaFile);
            cache = JsonHelpers.ReadJson(json) ?? new List<Library>();
        }
        else
        {
            cache = new List<Library>();
        }

        return cache;
    }

    private string EnsureDataDirectory()
    {
#if DEBUG
        string dataDirectory = Path.Combine(ExeFolder!, "data");
        Directory.CreateDirectory(dataDirectory);
#else
        string flexlibDir = Path.Combine(AppDataFolder!, "Flexlib");
        string dataDirectory =  Path.Combine(flexlibDir, "data");
        Directory.CreateDirectory(flexlibDir);
#endif 
        Directory.CreateDirectory(dataDirectory);
    
        return dataDirectory;
    }

    public string GetDataDirectory()
    {
        return _dataDirectory;
    }
    
    private string EnsureMetaFile()
    {
        string? exeFolder = Env.GetExecutingAssemblyLocation();
        
        if (!Directory.Exists(exeFolder))
            throw new DirectoryNotFoundException($"Directory not found: {exeFolder}");

        string metaFile = Path.Combine(_dataDirectory, "libraries.json");

        return metaFile;
    }

    public void Save(Library lib)
    {
        if ( string.IsNullOrWhiteSpace(lib.Path) )
        {
            lib.Path = _dataDirectory;
        }

        _cache.RemoveAll(l => l.Name == lib.Name && l.Path == lib.Path);
        
        _cache.Add(lib);

        JsonHelpers.WriteJson(_metaFile, _cache);
        UpdateLibFileStructure(lib);
        UpdateLibMetaFile(lib);
        UpdateItems(lib);
        UpdateLocalStorage(lib);
       
    }

    public Result RemoveLibraryByName(string name)
    {
        var selectedLibrary = _cache.FirstOrDefault(l => l.Name == name);

        if (selectedLibrary == null || string.IsNullOrWhiteSpace(selectedLibrary.Name))
        {
            return Result.Fail("Library can't be removed because it doesn't exist — which, honestly, is already kind of a win.");
        }

        string libraryPath = Path.Combine(selectedLibrary.Path ?? "", selectedLibrary.Name);

        Console.WriteLine($"\nAre you sure you want to delete the library '{name}' at path:\n\n  {libraryPath} ?\n");
        Console.Write("(y/N) > ");
        string? input = Console.ReadLine();

        if (!string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail("Deletion cancelled by user.");
        }

        if (string.IsNullOrWhiteSpace(libraryPath) || !Directory.Exists(libraryPath))
        {
            return Result.Fail($"Path '{libraryPath}' does not exist.");
        }

        // Safety: avoid deleting suspiciously short root-level paths
        if (Path.GetFullPath(libraryPath).Length < 10)
        {
            return Result.Fail("Library path is suspiciously short. Aborting deletion to protect your filesystem.");
        }

        try
        {
            foreach (var file in Directory.GetFiles(libraryPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(libraryPath, recursive: true);
           
            _cache.RemoveAll(l => l.Name == name);
            JsonHelpers.WriteJson(_metaFile, _cache);
            
            return Result.Success($"Library '{name}' was successfully removed.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete library local files in '{libraryPath}': {ex.Message}");
        }

    }

    public Result RemoveItem(LibraryItem item, Library lib)
    {
        if (item == null || lib == null)
            return Result.Fail("Invalid library or item.");

        if (string.IsNullOrWhiteSpace(lib.Path) || string.IsNullOrWhiteSpace(lib.Name))
            return Result.Fail("Library path or name is missing.");

        string libraryFolder = Path.Combine(lib.Path, lib.Name);
        if (!Directory.Exists(libraryFolder))
            return Result.Fail($"Library folder '{libraryFolder}' does not exist.");

        string itemsFolder = Path.Combine(libraryFolder, "items");
        string localItemsFolder = Path.Combine(itemsFolder, "local");

        string itemJsonPath = Path.Combine(itemsFolder, $"{item.Name}.json");
        string itemDataPath = Path.Combine(localItemsFolder, $"{item.Name}{item.FileExtension}");

        Console.WriteLine($"\nAre you sure you want to delete the item '{item.Name}' from library '{lib.Name}'?\n\n");
        Console.Write("(y/N) > ");
        string? input = Console.ReadLine();
        if (!string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
            return Result.Fail("Deletion cancelled.");

        try
        {

            string libFilePath = Path.Combine(libraryFolder, $"{lib.Name}.json");
            JsonHelpers.WriteJson(libFilePath, lib);

            JsonHelpers.WriteJson(_metaFile, _cache); // Assuming `lib` is a reference within `_cache`

            if (File.Exists(itemJsonPath))
                File.Delete(itemJsonPath);

            if (File.Exists(itemDataPath))
                File.Delete(itemDataPath);

            lib.Items.RemoveAll(i => i.Id == item.Id);

            Save(lib);
            
            return Result.Success($"Item '{item.Name}' removed from library '{lib.Name}'.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to remove item '{item.Name}': {ex.Message}");
        }
    }

    private void UpdateItems(Library lib)
    {
        string itemsDir = Path.Combine(lib.Path, "items/");

        foreach (LibraryItem item in lib.Items)
        {
            UpdateItemMetaFile(item, lib);
        }
    }
    
    private void UpdateLocalStorage(Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string itemsDir = Path.Combine(libDir, "items");
        string localDir = Path.Combine(itemsDir, "local");

        Directory.CreateDirectory(localDir);

        var localFiles = new HashSet<string?>(
            Directory.GetFiles(localDir).Select(Path.GetFileName),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (LibraryItem item in lib.Items)
        {
            string sourcePath = item.Origin!;
            
            string itemId = item.Id!.ToString();
            string itemName = item.Name!;
            string fileExtension = Path.GetExtension( Path.GetFileName(sourcePath) );
            string fileName = $"{itemId}-{itemName}{fileExtension}";

            string targetPath = Path.Combine(localDir, fileName);

            if (!localFiles.Contains(fileName))
            {
                AddressType type = AddressAnalysis.GetAddressType(sourcePath);
                CopyHelpers.TryCopyToLocal(type, sourcePath, targetPath);
            }
        }
    }

    private void UpdateLibFileStructure(Library lib)
    {   
        string libDir = Path.Combine(lib.Path, $"{lib.Name}");
        string itemsDir = Path.Combine(libDir, "items/");
        string localDir = Path.Combine(itemsDir, "local/");

        Directory.CreateDirectory(libDir);
        Directory.CreateDirectory(itemsDir);
        Directory.CreateDirectory(localDir);
 
    }

    private void UpdateLibMetaFile(Library lib)
    {

        string libDir = Path.Combine(lib.Path, $"{lib.Name}");

        string libMetaFile = Path.Combine(libDir, $"{lib.Name}.json");
        JsonHelpers.WriteJson(libMetaFile, lib.Items);
    
    }

    private void UpdateItemMetaFile(LibraryItem item, Library lib)
    {            
            string sourcePath = item.Origin!;
            
            string itemId = item.Id!.ToString();
            string itemName = item.Name!;
            string fileName = $"{itemId}-{itemName}";

        string itemMetaFile = Path.Combine(lib.Path, lib.Name!, "items", $"{fileName}.json");
        JsonHelpers.WriteJson(itemMetaFile, item);
    }

    private void DeleteLibMetaFile(Library lib)
    {
        string libDir = Path.Combine(lib.Path, lib.Name!);
        string libMetaFile = Path.Combine(libDir, $"{lib.Name}.json");

        if (File.Exists(libMetaFile))
            File.Delete(libMetaFile);
    }

    private void DeleteItemMetaFile(LibraryItem item, Library lib)
    {
        string sourcePath = item.Origin!;
        
        string itemId = item.Id!.ToString();
        string itemName = item.Name!;
        string fileName = $"{itemId}-{itemName}";

        string itemMetaFile = Path.Combine(lib.Path, lib.Name!, "items", $"{fileName}.json");
        if (File.Exists(itemMetaFile))
            File.Delete(itemMetaFile);
    }


    private void DeleteAllLocalMetaFiles()
    {
        foreach (var lib in _cache)
        {
            if (lib == null) continue;

            DeleteLibMetaFile(lib);

            if (lib.Items == null) continue;

            foreach (var item in lib.Items)
            {
                DeleteItemMetaFile(item, lib);
            }
        }
    }

    public double GetLocalItemFileSizes(List<LibraryItem> items, Library library)
    {
        if (items == null || library == null || string.IsNullOrEmpty(library.Path) || string.IsNullOrEmpty(library.Name))
            return 0;

        string localFolderPath = Path.Combine(library.Path, library.Name, "items", "local");

        if (!Directory.Exists(localFolderPath))
            return 0;

        double totalBytes = 0;

        foreach (var item in items)
        {
            if (item?.Id == null)
                continue;

            // Match any file starting with "{id}-"
            string searchPattern = $"{item.Id}-*";
            var matchingFiles = Directory.GetFiles(localFolderPath, searchPattern);

            foreach (var file in matchingFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    totalBytes += fileInfo.Length;
                }
                catch
                {
                }
            }
        }

        return totalBytes;
    }

    public bool Exists(string name) => _cache.Any( l => l.Name == name);

    public Library? GetByName(string name) => _cache.FirstOrDefault(l => l.Name == name);

    public IEnumerable<Library> GetAll() => _cache;

}
