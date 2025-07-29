namespace WadMaker.Extensions;

public static class IShapeExtensions
{
    public static Point[] GetPoints(this IShape room)
    {
        var initialPoints = new[]
        {
            new Point(room.UpperLeft.X, room.UpperLeft.Y),
            new Point(room.BottomRight.X, room.UpperLeft.Y),
            new Point(room.BottomRight.X, room.BottomRight.Y),
            new Point(room.UpperLeft.X, room.BottomRight.Y),
        };

        return room.ShapeModifiers
            .Aggregate(initialPoints, (p, s) => s.AlterPoints(p, room))
            .ToArray();
    }
}
