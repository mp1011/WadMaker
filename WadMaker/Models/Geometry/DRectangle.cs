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

    public DRectangle(Point upperLeft, Point bottomRight)
    {
        X = upperLeft.X;
        Y = upperLeft.Y;
        Width = bottomRight.X - upperLeft.X;
        Height = Math.Abs(upperLeft.Y - bottomRight.Y);
    }

    public DRectangle(IEnumerable<Point> points)
    {
        if (!points.Any())
            throw new ArgumentException("Points collection cannot be empty", nameof(points));
        X = points.Min(p => p.X);
        Y = points.Max(p => p.Y);
        Width = points.Max(p => p.X) - X;
        Height = Math.Abs(Y - points.Min(p => p.Y));
    }

    public Point GetCorner(Side corner)
    {
        return corner switch
        {
            Side.Top | Side.Left => new Point(X, Y),
            Side.Top | Side.Right => new Point(Right, Y),
            Side.Bottom | Side.Left => new Point(X, Bottom),
            Side.Bottom | Side.Right => new Point(Right, Bottom),
            _ => throw new Exception("Invalid corner"),
        };
    }

    public Point RelativePoint(double relX, double relY)
    {
        return new Point((int)(Size.Width * relX), (int)(-Size.Height * relY));
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

    public (Point, Point) GetSegment(Side side, int position, int width)
    {
        Point upperLeft, bottomRight;
        switch (side)
        {
            case Side.Right:
                upperLeft = side.ToPoint(position);
                bottomRight = upperLeft.Add(new Point(width, -Height));
                break;
            case Side.Left:
                upperLeft = side.ToPoint(position).Add(new Point(Width - width, 0));
                bottomRight = upperLeft.Add(new Point(width, -Height));
                break;
            case Side.Bottom:
                upperLeft = side.ToPoint(position);
                bottomRight = upperLeft.Add(new Point(Width, -width));
                break;
            case Side.Top:
                upperLeft = side.ToPoint(position).Add(new Point(0, -(Height - width)));
                bottomRight = upperLeft.Add(new Point(Width, -width));
                break;
            default:
                throw new Exception("Invalid Side");

        }

        return (upperLeft, bottomRight);
    }
}
