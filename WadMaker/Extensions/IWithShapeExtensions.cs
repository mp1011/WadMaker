namespace WadMaker.Extensions;

public static class IWithShapeExtensions
{
    public static void MoveTo(this IWithShape shape, Point center)
    {
        var size = shape.Bounds().Size;
        shape.UpperLeft = new Point(center.X - size.Width / 2, center.Y + size.Height / 2);
        shape.BottomRight = new Point(shape.UpperLeft.X + size.Width, shape.UpperLeft.Y - size.Height);
    }

    public static DRectangle Bounds(this IWithShape obj) => obj.Shape.Bounds;

    public static bool IsRelativeTo(this IWithShape item, IWithShape other)
    {
        return item.Shape.RelativeTo == other.Shape;
    }

    public static ShapePlacer<T> Place<T>(this T shape) where T : IWithShape
    {
        return new ShapePlacer<T>(shape);
    }

    public static MultiShapePlacer<T> Place<T>(this IEnumerable<T> shapes) where T : IWithShape
    {
        return new MultiShapePlacer<T>(shapes);
    }
}
