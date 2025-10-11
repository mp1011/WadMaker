
namespace WadMaker.Services.ShapeModifiers;

public class Bend(Side Sides, int HallWidth) : IShapeModifier
{
    public Point[] AlterPoints(Point[] points)
    {
        if (points.Length != 4)
            throw new NotSupportedException("Currentlt only supported on rectangles");

        var bounds = new DRectangle(points);

        var cornerToAdjust = bounds.GetCorner(Sides);

        var center = points.CentralPoint();
        List<Point> newPoints = new List<Point>();
        foreach (var point in points.WithNeighbors())
        {
            if(point.Item2 != cornerToAdjust)
            {
                newPoints.Add(point.Item2);
                continue;
            }

            var len1 = point.Item1.DistanceTo(point.Item2) - HallWidth;
            var len2 = point.Item2.DistanceTo(point.Item3) - HallWidth;

            var newPt1 = point.Item2.MoveToward(point.Item1, len1);
            var newPt2 = point.Item2.MoveToward(point.Item3, len2);
            var adjustedCorner = new Point(newPt1.X, newPt2.Y);
            if (adjustedCorner.Equals(point.Item2))
                adjustedCorner = new Point(newPt2.X, newPt1.Y);

            newPoints.Add(newPt1);
            newPoints.Add(adjustedCorner);
            newPoints.Add(newPt2);
        }

        return newPoints.ToArray();
    }

   
}
