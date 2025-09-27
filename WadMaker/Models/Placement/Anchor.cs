namespace WadMaker.Models.Placement;

public abstract record Anchor(double Value)
{
    public static Anchor MidPoint => new RelativeAnchor(0.5);
}

public record AbsoluteAnchor(double Value) : Anchor(Value)
{

}

public record RelativeAnchor(double Value) : Anchor(Value)
{

}
