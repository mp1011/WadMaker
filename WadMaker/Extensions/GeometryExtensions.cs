using WadMaker.Models.Geometry;

namespace WadMaker.Extensions;

public static class GeometryExtensions
{
    public static bool Intersects(this vertex v, LineDef line)
    {
        var colinear = (line.V2.x - line.V1.x) * (v.y - line.V1.y) == (line.V2.y - line.V1.y) * (v.x - line.V1.x);
        if (!colinear)
            return false;

        // Bounds check
        bool withinX = v.x >= Math.Min(line.V1.x, line.V2.x) && v.x <= Math.Max(line.V1.x, line.V2.x);
        bool withinY = v.y >= Math.Min(line.V1.y, line.V2.y) && v.y <= Math.Max(line.V1.y, line.V2.y);

        return withinX && withinY;
    }

    public static double SquaredDistanceTo(this vertex v, vertex other)
    {
        double dx = v.x - other.x;
        double dy = v.y - other.y;
        return dx * dx + dy * dy;
    }

    public static double DistanceTo(this vertex v, vertex other) => Math.Sqrt(v.SquaredDistanceTo(other));

    public static double SquaredDistanceTo(this Point v, Point other)
    {
        double dx = v.X - other.X;
        double dy = v.Y - other.Y;
        return dx * dx + dy * dy;
    }

    public static double DistanceTo(this Point p, Point other) =>
        Math.Sqrt(p.SquaredDistanceTo(other));

    public static DRectangle ExtendOnSide(this DRectangle rectangle, Side side, int distance)
    {
        return side switch
        {
            Side.Left => new DRectangle(rectangle.X - distance, rectangle.Y, rectangle.Width + distance, rectangle.Height),
            Side.Right => new DRectangle(rectangle.X, rectangle.Y, rectangle.Width + distance, rectangle.Height),
            Side.Top => new DRectangle(rectangle.X, rectangle.Y + distance, rectangle.Width, rectangle.Height + distance),
            Side.Bottom => new DRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height + distance),
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }

    public static IEnumerable<Point> SidePoints(this DRectangle rectangle, Side side)
    {
        return side switch
        {
            Side.Left   => new[] { new Point(rectangle.X, rectangle.Y), new Point(rectangle.X, rectangle.Bottom) },
            Side.Right  => new[] { new Point(rectangle.Right, rectangle.Y), new Point(rectangle.Right, rectangle.Bottom) },
            Side.Top    => new[] { new Point(rectangle.X, rectangle.Y), new Point(rectangle.Right, rectangle.Y) },
            Side.Bottom => new[] { new Point(rectangle.X, rectangle.Bottom), new Point(rectangle.Right, rectangle.Bottom) },
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }

    public static Side Opposite(this Side side)
    {
        return side switch
        {
            Side.Left => Side.Right,
            Side.Right => Side.Left,
            Side.Top => Side.Bottom,
            Side.Bottom => Side.Top,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }

    public static Point Add(this Point pt, Point other)
    {
        return new Point(pt.X + other.X, pt.Y + other.Y);
    }

    public static Point Add(this Point pt, int dx, int dy)
    {
        return new Point(pt.X + dx, pt.Y + dy);
    }

    public static Point CentralPoint(this IEnumerable<Point> points)
    {
        if (!points.Any())
            return Point.Empty;
        int sumX = points.Sum(p => p.X);
        int sumY = points.Sum(p => p.Y);
        int count = points.Count();
        return new Point(sumX / count, sumY / count);
    }

    public static Point CentralPoint(this IEnumerable<vertex> points)
    {
        if (!points.Any())
            return Point.Empty;
        int sumX = points.Sum(p => (int)p.x);
        int sumY = points.Sum(p => (int)p.y);
        int count = points.Count();
        return new Point(sumX / count, sumY / count);
    }

    /// <summary>
    /// Moves each point toward their common center until their distance apart is the specified value
    /// </summary>
    /// <param name="points"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static IEnumerable<Point> MoveToDistance(this IEnumerable<Point> points, int distance = 32)
    {
        var center = points.CentralPoint();

        // assuming two points, not sure if I'll need to handle more
        var currentDistance = points.First().DistanceTo(points.Last());

        var delta = (currentDistance - distance) / 2;

        return points.Select(p => p.MoveToward(center, delta));
    }

    public static Point MoveToward(this Point point, Point target, double delta)
    {
        if (point == target || delta <= 0)
            return point;
        double angle = Math.Atan2(target.Y - point.Y, target.X - point.X);
        return new Point(
            (int)(point.X + delta * Math.Cos(angle)),
            (int)(point.Y + delta * Math.Sin(angle))
        );
    }

    public static Point Move(this Point point, Side side, int distance)
    {
        return side switch
        {
            Side.Right => new Point(point.X + distance, point.Y),
            Side.Left => new Point(point.X - distance, point.Y),
            Side.Top => new Point(point.X, point.Y + distance),
            Side.Bottom => new Point(point.X, point.Y - distance),
            _ => point
        };
    }

    public static double AsAngle(this double angle) => angle.NMod(360.0);

    public static double AngleTo(this vertex v1, vertex v2)
    {
        double deltaY = v2.y - v1.y;
        double deltaX = v2.x - v1.x;
        double radians = Math.Atan2(deltaY, deltaX);
        double degrees = radians * (180.0 / Math.PI);

        return degrees.AsAngle();
    }
    public static double AngleTo(this Point p1, Point p2)
    {
        double deltaY = p2.Y - p1.Y;
        double deltaX = p2.X - p1.X;
        double radians = Math.Atan2(deltaY, deltaX);
        double degrees = radians * (180.0 / Math.PI);

        return degrees.AsAngle();
    }

    /// <summary>
    /// Gets the angle of front sidedef
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static double FrontSidedefAngle(this vertex v1, vertex v2)
    {
        return (v1.AngleTo(v2) - 90.0).AsAngle();
    }

    public static Point ToPoint(this Side s, int length) =>
        s switch
        {
            Side.Left => new Point(-length, 0),
            Side.Right => new Point(length, 0),
            Side.Top => new Point(0, length),
            Side.Bottom => new Point(0, -length),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

    public static double AngleDifference(this double angle1, double angle2)
    {
        var d1 = (angle1 - angle2).NMod(360.0);
        var d2 = (angle2 - angle1).NMod(360.0);
        return Math.Min(d1, d2);
    }

    public static Point AngleToPoint(this double angle, double length)
    {
        double angleRadians = angle * Math.PI / 180.0;

        var x = length * Math.Cos(angleRadians);
        var y = length * Math.Sin(angleRadians);

        return new Point((int)x, (int)y);
    }

    public static Side ClockwiseTurn(this Side s) =>
        s switch
        {
            Side.Left => Side.Top,
            Side.Top => Side.Right,
            Side.Right => Side.Bottom,
            Side.Bottom => Side.Left,
            _ => Side.None
        };

    public static Side CounterClockwiseTurn(this Side s) =>
        s switch
        {
            Side.Left => Side.Bottom,
            Side.Top => Side.Left,
            Side.Right => Side.Top,
            Side.Bottom => Side.Right,
            _ => Side.None
        };
}
