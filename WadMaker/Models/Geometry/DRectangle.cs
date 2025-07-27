namespace WadMaker.Models.Geometry;

/// Rectangle in Doom coordinates where decreasing Y means going down
public struct DRectangle
{
    public int X { get; init; }
    public int Y { get; init; } 
    public int Width { get; init; }
    public int Height { get; init; }

    public int Bottom => Y - Height;
    public int Right => X + Width;

    public DRectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public DRectangle(Point location, Size size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public bool IntersectsWith(DRectangle other)
    {
        return X < other.Right && Right > other.X &&
               Y > other.Bottom && Bottom < other.Y;
    }

    public int AxisLength(Side side)
    {
        switch (side)
        {
            case Side.Left:
            case Side.Right:
                return Width;
            default:
                return Height;
        }
    }
}
