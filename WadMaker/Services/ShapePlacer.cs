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
        var anchorPoint = Anchor.MidPoint.GetPoint(Shape, side.Opposite());
        var otherAnchorPoint = Anchor.MidPoint.GetPoint(other, side);

        anchorPoint.Position = otherAnchorPoint.Position.Move(side, gap);

        return Shape;
    }

    public T WestOf(IShape other, int gap = 0) => ToSideOf(other, Side.Left, gap);
    public T EastOf(IShape other, int gap = 0) => ToSideOf(other, Side.Right, gap);
    public T NorthOf(IShape other, int gap = 0) => ToSideOf(other, Side.Top, gap);
    public T SouthOf(IShape other, int gap = 0) => ToSideOf(other, Side.Bottom, gap);


}
