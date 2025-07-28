
namespace WadMaker.Services.ShapeModifiers;


/// <summary>
/// Replaces all right angled corners with 45 degree slants
/// </summary>
public class AngledCorners : IShapeModifier
{
    public int Width { get; set; } = 16;

    public Point[] AlterPoints(Point[] points, IShape room)
    {
        List<Point> newPoints = new List<Point>();
        foreach (var point in points.WithNeighbors())
        {
            var angle1 = point.Item1.AngleTo(point.Item2);
            var angle2 = point.Item2.AngleTo(point.Item3);

            var side1 = point.Item1.DistanceTo(point.Item2);   
            var side2 = point.Item2.DistanceTo(point.Item3);

            if (angle1.AngleDifference(angle2) != 90.0)
            {
                newPoints.Add(point.Item2);
                continue;
            }

            var legWidth = (Width * Math.Sqrt(2)) / 2.0;

            if(legWidth < side1 && legWidth < side2)
            {
                var newCorner1 = point.Item2.MoveToward(point.Item1, legWidth);
                var newCorner2 = point.Item2.MoveToward(point.Item3, legWidth);
                newPoints.Add(newCorner1);  
                newPoints.Add(newCorner2);
            }
            else
            {
                newPoints.Add(point.Item2);
            }            
        }

        return newPoints.ToArray();
    }
}
