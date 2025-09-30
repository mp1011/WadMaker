using System;
using System.Drawing;

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

    public Point Center => new Point(X + (Width/2),
                                     Y - (Height/2));

    public Size Size => new Size(Width, Height);

    public int Area => Width * Height;

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

    public Point AxisLength2(Side side)
    {
        switch (side)
        {
            case Side.Left:
            case Side.Right:
                return new Point(Width, 0);
            default:
                return new Point(0, Height);
        }
    }

    public Point GetRelativeSidePoint(Side side, int position)
    {
        return side switch
        {
            Side.Left => new Point(0, -position),
            Side.Right => new Point(Width, -position),
            Side.Bottom => new Point(position, -Height),
            Side.Top => new Point(position, 0),
            _ => new Point(0, 0),
        };
    }

    public Point SideCenter(Side side) => side switch
    {
        Side.Left => new Point(X, Y - Height / 2),
        Side.Right => new Point(Right, Y - Height / 2),
        Side.Top => new Point(X + Width / 2, Y),
        Side.Bottom => new Point(X + Width / 2, Bottom),
        _ => throw new Exception("Invalid Side"),
    };

    public int SidePosition(Side side) => side switch
    {
        Side.Left => X,
        Side.Right => Right,
        Side.Top => Y,
        Side.Bottom => Bottom,
        _ => throw new Exception("Invalid Side")
    };

    public int SideLength(Side side)
    {
        switch (side)
        {
            case Side.Left:
            case Side.Right:
                return Height;
            default:
                return Width;
        }
    }

    public Side SideRelativeTo(DRectangle other)
    {
        DRectangle dis = this;
        return Enum.GetValues<Side>().Where(s => s != Side.None).FirstOrDefault(side =>
        {
            var extendedBounds = other.ExtendOnSide(side, 5000);
            return dis.IntersectsWith(extendedBounds);
        });        
    }
}
