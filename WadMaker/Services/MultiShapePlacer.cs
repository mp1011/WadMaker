namespace WadMaker.Services;

public class MultiShapePlacer<T> where T : IWithShape
{
    public T[] Shapes { get; }

    public MultiShapePlacer(IEnumerable<T> shapes)
    {
        Shapes = shapes.ToArray();
    }

    public T[] InLine(IWithShape container, Side direction, double position, Padding padding)
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

    public T[] InGrid(IWithShape container, int columns, Padding padding)
    {
        int rows = Shapes.Length / columns;
        if (Shapes.Length < 4)
            return Shapes;

        // place corners first

        // upper left
        Shapes[0].MoveTo(new Point(
            padding.Left + Shapes[0].Bounds().Width / 2, 
            -padding.Top - Shapes[0].Bounds().Height / 2));

        // upper right
        Shapes[columns-1].MoveTo(new Point(
            container.Bounds().Width - padding.Right - Shapes[columns - 1].Bounds().Width / 2,
            -padding.Top - Shapes[columns - 1].Bounds().Height / 2));

        // lower left 
        Shapes[Shapes.Length - columns].MoveTo(new Point(
            padding.Left + Shapes[Shapes.Length - columns].Bounds().Width / 2,
            -(container.Bounds().Height - padding.Bottom - Shapes[Shapes.Length - columns].Bounds().Height / 2)));

        // lower right
        Shapes[Shapes.Length - 1].MoveTo(new Point(
             container.Bounds().Width - padding.Right - Shapes[Shapes.Length - 1].Bounds().Width / 2,
             -(container.Bounds().Height - padding.Bottom - Shapes[Shapes.Length - 1].Bounds().Height / 2)));

        Point cursor = Point.Empty;
        int height = Shapes.First().Bounds().Center.Y - Shapes.Last().Bounds().Center.Y;
        int dy = height / (rows - 1);
            
        // now place each shape equally distant
        for (int row = 0; row < rows; row++)
        {
            var rowShapes = Shapes.Skip(row * columns).Take(columns).ToArray();

            if (row > 0 && row < rows - 1)
            {
                rowShapes.First().MoveTo(new Point(
                    padding.Left + rowShapes.First().Bounds().Width / 2,
                    cursor.Y - dy));

                rowShapes.Last().MoveTo(new Point(
                    container.Bounds().Width - padding.Right - rowShapes.Last().Bounds().Width / 2,
                    cursor.Y - dy));
            }

            cursor = rowShapes.First().Bounds().Center;

            int width = rowShapes.Last().Bounds().Center.X - rowShapes.First().Bounds().Center.X;
            int dx = width / (columns - 1);

            for(int col = 0; col < columns; col++)
            {
                rowShapes[col].MoveTo(cursor);
                cursor = cursor.Add(dx, 0);
            }
        }

        return Shapes;
    }
}
