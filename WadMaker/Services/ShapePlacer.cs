namespace WadMaker.Services;

public class ShapePlacer<T> where T : IShape
{
    public T Shape { get; }

    public ShapePlacer(T shape)
    {
        Shape = shape;
    }

    public T ToSideOf(IShape other, Side side, int gap = 0, Anchor? sourceAnchor = null, Anchor? targetAnchor = null)
    {
        var anchorPoint = (sourceAnchor ?? Anchor.MidPoint).GetPoint(Shape, side.Opposite());
        var otherAnchorPoint = (targetAnchor ?? Anchor.MidPoint).GetPoint(other, side);

        anchorPoint.Position = otherAnchorPoint.Position.Move(side, gap);

        return Shape;
    }

    public T WestOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Left, gap, sourceAnchor: anchor);
    public T EastOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Right, gap, sourceAnchor: anchor);
    public T NorthOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Top, gap, sourceAnchor: anchor);
    public T SouthOf(IShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Bottom, gap, sourceAnchor: anchor);


}
