namespace WadMaker.Services;

public class ShapePlacer<T> where T : IShape
{
    public T Shape { get; }

    public ShapePlacer(T shape)
    {
        Shape = shape;
    }

    private void MakePositionRelative(IShape relativeTo)
    {
        var size = Shape.Bounds().Size;
        Shape.UpperLeft = new Point(Shape.UpperLeft.X - relativeTo.UpperLeft.X, Shape.UpperLeft.Y - relativeTo.UpperLeft.Y);
        Shape.BottomRight = Shape.UpperLeft.Add(size.Width, -size.Height);
    }

    public T ToSideOf(IShape other, Side side, int gap = 0, Anchor? sourceAnchor = null, Anchor? targetAnchor = null)
    {
        var anchorPoint = (sourceAnchor ?? Anchor.MidPoint).GetPoint(Shape, side.Opposite());
        var otherAnchorPoint = (targetAnchor ?? Anchor.MidPoint).GetPoint(other, side);

        anchorPoint.Position = otherAnchorPoint.Position.Move(side, gap);

        if (other.Owns(Shape))
            MakePositionRelative(other);
            
        return Shape;
    }

    public T ToInsideOf(IShape other, Side side, int gap = 0, Anchor? sourceAnchor = null, Anchor? targetAnchor = null)
    {
        return ToSideOf(other, side, gap - Shape.Bounds().AxisLength(side), sourceAnchor, targetAnchor);
    }

    public T WestOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Left, gap, sourceAnchor: anchor);
    public T EastOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Right, gap, sourceAnchor: anchor);
    public T NorthOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Top, gap, sourceAnchor: anchor);
    public T SouthOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Bottom, gap, sourceAnchor: anchor);

    public T InsideWestOf(IShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Left, gap, sourceAnchor: anchor);
    public T InsideEastOf(IShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Right, gap, sourceAnchor: anchor);
    public T InsideNorthOf(IShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Top, gap, sourceAnchor: anchor);
    public T InsideSouthOf(IShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Bottom, gap, sourceAnchor: anchor);


    public T InCenterOf(IShape other)
    {
        Shape.Center = other.Center;
        
        if (other.Owns(Shape))
            MakePositionRelative(other);

        return Shape;
    }

}
