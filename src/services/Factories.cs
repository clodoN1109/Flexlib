namespace Flexlib.Services.Media;

public class MediaServiceFactory
{
    public static IMediaService CreateDefault()
    {
        return new PowerExplorer();
    }
}

