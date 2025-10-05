namespace WadMaker.Models.BuildingBlocks;

public class Cutout : IShape
{
    public List<IShapeModifier> ShapeModifiers { get; } = new List<IShapeModifier>();

    public Point UpperLeft { get; set; } = Point.Empty;
    public Point BottomRight { get; set; } = Point.Empty;
    public Texture WallTexture { get; set; } = Texture.STONE;

    public Point Center
    {
        get => this.Bounds().Center;
        set
        {
            Point delta = new Point(value.X - this.Bounds().Center.X, value.Y - this.Bounds().Center.Y);
            UpperLeft = UpperLeft.Add(delta);
            BottomRight = BottomRight.Add(delta);
        }
    }

    public Cutout(Point? upperLeft = null, Point? center = null, Size ? size = null)
    {
        size ??= new Size(64, 64);
        UpperLeft = upperLeft ?? Point.Empty;
        BottomRight = new Point(UpperLeft.X + size.Value.Width, UpperLeft.Y - size.Value.Height);
    }

    public Cutout Copy()
    {
        var copy = new Cutout
        {
            UpperLeft = UpperLeft,
            BottomRight = BottomRight,
            WallTexture = WallTexture
        };
        copy.ShapeModifiers.AddRange(ShapeModifiers);
        return copy;
    }

    public Cutout RelativeTo(IShape parent)
    {
         var copy = new Cutout
         {
             UpperLeft = parent.UpperLeft.Add(UpperLeft),
             BottomRight = parent.UpperLeft.Add(BottomRight),
             WallTexture = WallTexture,             
         };
        copy.ShapeModifiers.AddRange(ShapeModifiers);
        return copy;
    }

    bool IShape.Owns(IShape other) => false;
}
