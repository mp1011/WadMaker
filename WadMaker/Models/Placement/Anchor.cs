namespace WadMaker.Models.Placement;

public abstract record Anchor(double Value)
{
    public static Anchor MidPoint => new RelativeAnchor(0.5);

    public static Anchor Percent(double value) => new RelativeAnchor(value);

    public static Anchor Absolute(double value) => new AbsoluteAnchor(value);

    public abstract AnchorPoint GetPoint(IWithShape shape, Side side);
}

public record AbsoluteAnchor(double Value) : Anchor(Value)
{
    public override AnchorPoint GetPoint(IWithShape shape, Side side)
    {
        return new AnchorPoint(shape, shape.Bounds().GetRelativeSidePoint(side, (int)Value));
    }
}

public record RelativeAnchor(double Value) : Anchor(Value)
{
    public override AnchorPoint GetPoint(IWithShape shape, Side side)
    {
        return new AnchorPoint(shape, shape.Bounds().GetRelativeSidePoint(side, (int)(shape.Bounds().SideLength(side) * Value)));
    }
}
