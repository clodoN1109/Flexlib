using Flexlib.Infrastructure.Interop;

namespace Flexlib.Services.Media;

public interface IMediaService
{
    Result TryOpenFile(string filePath);
}

  
