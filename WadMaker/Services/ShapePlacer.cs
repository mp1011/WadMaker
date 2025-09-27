namespace WadMaker.Services;

public class ShapePlacer<T> where T : IShape
{
    public T Shape { get; }

    public ShapePlacer(T shape)
    {
        Shape = shape;
    }

    public T ToSideOf(IShape other, Side side, int gap = 0)
    {
        return side switch
        {
            Side.Left => WestOf(other, gap),
            Side.Right => EastOf(other, gap),
            Side.Top => NorthOf(other, gap),
            Side.Bottom => SouthOf(other, gap),
            _ => throw new Exception("Invalid side")
        };
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
        var x = other.BottomRight.X + gap;
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

    public T SouthOf(IShape other, int gap = 0)
    {
        var x = other.Bounds().Center.X - Shape.Bounds().Width / 2;
        int y = other.UpperLeft.Y - other.Bounds().Height - gap;

        var upperLeft = new Point(x, y);
        var bottomRight = new Point(upperLeft.X + Shape.Bounds().Width,
                                    upperLeft.Y - Shape.Bounds().Height);

        Shape.UpperLeft = upperLeft;
        Shape.BottomRight = bottomRight;
        return Shape;
    }
}
