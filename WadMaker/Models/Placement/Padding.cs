namespace WadMaker.Models.Placement;

public record Padding(int Left, int Top, int Right, int Bottom)
{
    public Padding(int Horizontal, int Vertical) : this(Horizontal, Vertical, Horizontal, Vertical) { }

    public Padding(int All) : this(All, All, All, All) { }

    public Point OnSide2(Side side) => side switch
    {
        Side.Left => new Point(Left, 0),
        Side.Right => new Point(Right, 0),
        Side.Top => new Point(0, -Top),
        Side.Bottom => new Point(0, -Bottom),
        _ => Point.Empty,
    };

    public int OnSide(Side side) => side switch
    {
        Side.Left => Left,
        Side.Right => Right,
        Side.Top => Top,
        Side.Bottom => Bottom,
        _ => 0,
    };
}
