namespace Flexlib.Infrastructure.Processing;

public static class MathUtil
{
    public static int IfZeroFallbackTo(this int number, int fallback, out int replaced)
    {
        if (number == 0)
            replaced = fallback;
        else
            replaced = number;

        return replaced;
    }
}
