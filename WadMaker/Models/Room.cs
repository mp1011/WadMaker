namespace WadMaker.Models;

public class Room : IShape
{    
    public List<IShapeModifier> ShapeModifiers { get; } = new List<IShapeModifier>();

    public Point UpperLeft { get; set; } = Point.Empty;
    public Point BottomRight { get; set; } = Point.Empty;

    public Point Center => new Point(UpperLeft.X + (BottomRight.X - UpperLeft.X) / 2, 
                                     UpperLeft.Y + (BottomRight.Y - UpperLeft.Y) / 2);

    public DRectangle Bounds => new DRectangle(
        location: UpperLeft,
        size: new Size(BottomRight.X - UpperLeft.X,
            Math.Abs(BottomRight.Y - UpperLeft.Y)));
            
    public int Height => Ceiling - Floor;

    public int Ceiling { get; set; } = 128;
    public int Floor { get; set; } = 0;
    public Flat FloorTexture { get; set; } = Flat.Default;
    public Flat CeilingTexture { get; set; } = Flat.Default;
    public Texture WallTexture { get; set; } = Texture.Default;

    public Dictionary<Side, Texture> SideTextures { get; private set; } = new Dictionary<Side, Texture>();

    public int? Tag { get; set; } = null;

    public Dictionary<Side, LineSpecial> LineSpecials { get; private set; } = new Dictionary<Side, LineSpecial>();

    public List<Hall> Halls { get; } = new List<Hall>();
    public List<Cutout> Pillars { get; } = new List<Cutout>();

    public List<Room> InnerStructures { get; } = new List<Room>();

    public Room() { }

    public Room(IEnumerable<Point> points)
    {        
        UpperLeft = new Point(points.Min(p => p.X), points.Max(p => p.Y));
        BottomRight = new Point(points.Max(p => p.X), points.Min(p => p.Y));
    }

    public Texture TextureForSide(Side? side)
    {
        if (side == null)
            return WallTexture;

        var sideTexture = SideTextures.Try(side.Value);
        if (sideTexture == Texture.MISSING)
            return WallTexture;
        else
            return sideTexture;
    }

    public Room RelativeTo(Room parent)
    {
        var copy = new Room
        {
            UpperLeft = parent.UpperLeft.Add(UpperLeft),
            BottomRight = parent.UpperLeft.Add(BottomRight),
            Floor = parent.Floor + Floor,
            Ceiling = parent.Ceiling + Ceiling,
            CeilingTexture = CeilingTexture,
            FloorTexture = FloorTexture,
            WallTexture = WallTexture,
            LineSpecials = LineSpecials,
            SideTextures = SideTextures,
        };
        copy.ShapeModifiers.AddRange(ShapeModifiers);
        copy.InnerStructures.AddRange(InnerStructures);
        copy.Pillars.AddRange(Pillars);
        return copy;
    }
}
