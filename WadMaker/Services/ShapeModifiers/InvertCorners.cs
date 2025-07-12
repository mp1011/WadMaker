
namespace WadMaker.Services.ShapeModifiers;


/// <summary>
/// Takes each corner, creates two new points along each edge, and brings the original corner inward
/// </summary>
public class InvertCorners : IShapeModifier
{
    public int Width { get; set; } = 16;

    public Point[] AlterPoints(Point[] points, Room room)
    {
        var center = points.CentralPoint();
        List<Point> newPoints = new List<Point>();
        foreach (var point in points.WithNeighbors())
        {
            var len1 = point.Item1.DistanceTo(point.Item2);
            var len2 = point.Item2.DistanceTo(point.Item3);
            if (len1 < Width * 2 || len2 < Width * 2)
                continue;

            var extra1 = point.Item2.MoveToward(point.Item1, Width);
            var extra2 = point.Item2.MoveToward(point.Item3, Width);

            var adjustedCorner = new Point(extra1.X, extra2.Y);
            if(adjustedCorner.Equals(point.Item2))
                adjustedCorner = new Point(extra2.X, extra1.Y);

            newPoints.Add(extra1);
            newPoints.Add(adjustedCorner);
            newPoints.Add(extra2);
        }

        return newPoints.ToArray();
    }
}
