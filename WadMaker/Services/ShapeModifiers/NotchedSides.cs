
namespace WadMaker.Services.ShapeModifiers;

/// <summary>
/// Removes a chunk from each side
/// </summary>
public class NotchedSides : IShapeModifier
{
    public int Width { get; set; }  
    public int Depth { get; set; }

    public Point[] AlterPoints(Point[] points, IShape room)
    {
        var center = points.CentralPoint();
        var newPoints = new List<Point>();

        foreach (var point in points.WithNeighbors())
        {
            newPoints.Add(point.Item2);

            var angle = point.Item2.AngleTo(point.Item3);
            if((angle % 90) != 0)            
                continue;
            
            var lineSide = center.LineSide(point.Item2, point.Item3);
            if(lineSide == Side.None)
                continue;

            var lineLength = point.Item2.DistanceTo(point.Item3);
            if (lineLength <= Width)
                continue;

            var notchPoint1 = point.Item2.MoveToward(point.Item3, (lineLength - Width) / 2);
            var notchPoint2 = notchPoint1.Move(lineSide.Opposite(), Depth);
            var notchPoint4 = notchPoint1.MoveToward(point.Item3, Width);
            var notchPoint3 = notchPoint4.Move(lineSide.Opposite(), Depth);

            newPoints.Add(notchPoint1);
            newPoints.Add(notchPoint2);
            newPoints.Add(notchPoint3);
            newPoints.Add(notchPoint4);
        }

        return newPoints.ToArray();
    }
}
