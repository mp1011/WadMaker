namespace WadMaker.Extensions;

public static class SizeExtensions
{
    /// <summary>
    /// Scales the width and height of a Size by the given factor.
    /// </summary>
    public static Size Scale(this Size size, double factor)
    {
        return new Size(
            (int)(size.Width * factor),
            (int)(size.Height * factor)
        );
    }
}