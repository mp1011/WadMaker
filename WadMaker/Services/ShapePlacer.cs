namespace WadMaker.Services;

public class ShapePlacer<T> where T : IShape
{
    public T Shape { get; }

    public ShapePlacer(T shape)
    {
        Shape = shape;
    }

    public T WestOf(IShape other, int gap = 0)
    {
        var x = other.UpperLeft.X - Shape.Bounds().Width - gap;
        int y = other.Bounds().Center.Y + Shape.Bounds().Height / 2;

        var upperLeft = new Point(x, y);    
        var bottomRight = new Point(upperLeft.X + Shape.Bounds().Width,
                                    upperLeft.Y - Shape.Bounds().Height);

        Shape.UpperLeft = upperLeft;
        Shape.BottomRight = bottomRight;
        return Shape;
    }

    public T EastOf(IShape other, int gap = 0)
    {
        var x = other.BottomRight.X + Shape.Bounds().Width + gap;
        int y = other.Bounds().Center.Y + Shape.Bounds().Height / 2;

        var upperLeft = new Point(x, y);
        var bottomRight = new Point(upperLeft.X + Shape.Bounds().Width,
                                    upperLeft.Y - Shape.Bounds().Height);

        Shape.UpperLeft = upperLeft;
        Shape.BottomRight = bottomRight;
        return Shape;
    }

    public T NorthOf(IShape other, int gap = 0)
    {
        var x = other.Bounds().Center.X - Shape.Bounds().Width / 2;
        int y = other.UpperLeft.Y + Shape.Bounds().Height + gap;

        var upperLeft = new Point(x, y);
        var bottomRight = new Point(upperLeft.X + Shape.Bounds().Width,
                                    upperLeft.Y - Shape.Bounds().Height);

        Shape.UpperLeft = upperLeft;
        Shape.BottomRight = bottomRight;
        return Shape;
    }
}
