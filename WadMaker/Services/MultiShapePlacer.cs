namespace WadMaker.Services;

public class MultiShapePlacer<T> where T : IShape
{
    public T[] Shapes { get; }

    public MultiShapePlacer(IEnumerable<T> shapes)
    {
        Shapes = shapes.ToArray();
    }

    public T[] InLine(IShape container, Side direction, double position, Padding padding)
    {
        if (Shapes.Length <= 1)
            return Shapes;

        Point spacing = Point.Empty;

        var totalShapeSize = Shapes.Sum(p=>p.Bounds().AxisLength(direction));
        var availableSize = container.Bounds().AxisLength(direction) - padding.OnSide(direction) - padding.OnSide(direction.Opposite());
        var totalSpacing = availableSize - totalShapeSize;

        var spacingEach = totalSpacing / (Shapes.Length - 1);
        spacing = direction.ToPoint(spacingEach);

        var cursor = container.Bounds().GetRelativeSidePoint(direction.Opposite(),
            (int)(container.Bounds().AxisLength(direction.ClockwiseTurn()) * position));

        cursor = cursor.Add(padding.OnSide2(direction.Opposite()));

        foreach(var shape in Shapes)
        {
            cursor = cursor.Add(shape.Bounds().AxisLength2(direction).Scale(0.5));
            shape.MoveTo(cursor);
            cursor = cursor.Add(shape.Bounds().AxisLength2(direction).Scale(0.5));
            cursor = cursor.Add(spacing);
        }

        return Shapes;
    }
}
