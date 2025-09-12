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

    public static DRectangle Bounds(this IShape shape) => new DRectangle(
         location: shape.UpperLeft,
         size: new Size(shape.BottomRight.X - shape.UpperLeft.X,
             Math.Abs(shape.BottomRight.Y - shape.UpperLeft.Y)));

    public static void MoveTo(this IShape shape, Point center)
    {
        var size = shape.Bounds().Size;
        shape.UpperLeft = new Point(center.X - size.Width / 2, center.Y + size.Height / 2);
        shape.BottomRight = new Point(shape.UpperLeft.X + size.Width, shape.UpperLeft.Y - size.Height);
    }

    public static ShapePlacer<T> Place<T>(this T shape) where T : IShape
    {
        return new ShapePlacer<T>(shape);
    }

    public static MultiShapePlacer<T> Place<T>(this IEnumerable<T> shapes) where T : IShape
    {
        return new MultiShapePlacer<T>(shapes);
    }
}
