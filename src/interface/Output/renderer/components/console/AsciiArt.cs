
using Figgle.Fonts;

namespace Flexlib.Interface.Output;

public static partial class Components
{

    public static string AsciiArt(string text)
    {
        // For demo, uppercase + spaces = pseudo-block style
        // Replace with a real Figlet renderer if needed
        try
        {
            return _AsciiArt(text);
        }
        catch
        {
            string ascii = "";

            foreach (var c in text.ToUpperInvariant())
            {
                ascii += $"{c} ";
            }

            return ascii.TrimEnd();
        }

    }

    private static string _AsciiArt(string text) => FiggleFonts.Twisted.Render(text);

}