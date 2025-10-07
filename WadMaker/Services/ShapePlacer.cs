namespace WadMaker.Services;

public class ShapePlacer<T> where T : IWithShape
{
    public T Item { get; }

    public ShapePlacer(T shape)
    {
        Item = shape;
    }

    private void MakePositionRelative(IWithShape relativeTo)
    {
        var size = Item.Bounds().Size;
        Item.UpperLeft = new Point(Item.UpperLeft.X - relativeTo.UpperLeft.X, Item.UpperLeft.Y - relativeTo.UpperLeft.Y);
        Item.BottomRight = Item.UpperLeft.Add(size.Width, -size.Height);
    }

    public T ToSideOf(IWithShape other, Side side, int gap = 0, Anchor? sourceAnchor = null, Anchor? targetAnchor = null)
    {
        var anchorPoint = (sourceAnchor ?? Anchor.MidPoint).GetPoint(Item, side.Opposite());
        var otherAnchorPoint = (targetAnchor ?? Anchor.MidPoint).GetPoint(other, side);

        anchorPoint.Position = otherAnchorPoint.Position.Move(side, gap);

        Item.Shape.AdjustToRelative();
            
        return Item;
    }

    public T ToInsideOf(IWithShape other, Side side, int gap = 0, Anchor? sourceAnchor = null, Anchor? targetAnchor = null)
    {
        return ToSideOf(other, side, gap - Item.Bounds().AxisLength(side), sourceAnchor, targetAnchor);
    }

    public T WestOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Left, gap, sourceAnchor: anchor);
    public T EastOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Right, gap, sourceAnchor: anchor);
    public T NorthOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Top, gap, sourceAnchor: anchor);
    public T SouthOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToSideOf(other, Side.Bottom, gap, sourceAnchor: anchor);

    public T InsideWestOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Left, gap, sourceAnchor: anchor);
    public T InsideEastOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Right, gap, sourceAnchor: anchor);
    public T InsideNorthOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Top, gap, sourceAnchor: anchor);
    public T InsideSouthOf(IWithShape other, int gap = 0, Anchor? anchor = null) => ToInsideOf(other, Side.Bottom, gap, sourceAnchor: anchor);


    public T InCenterOf(IWithShape other)
    {
        Item.Center = other.Center;
        Item.Shape.AdjustToRelative();

        return Item;
    }

}
