namespace WadMaker.Models;

public class Room : IShape, IThemed
{    
    public IThemed Parent { get; }

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

    private Theme? _theme;
    public Theme? Theme
    {
        get
        {
            return _theme ?? Parent.Theme;
        }
        set
        {
            _theme = value;
        }
    }

    public TextureInfo WallTexture { get; set; } = new TextureInfo();

    public Dictionary<Side, TextureInfo> SideTextures { get; private set; } = new Dictionary<Side, TextureInfo>();

    public int? Tag { get; set; } = null;

    public Dictionary<Side, LineSpecial> LineSpecials { get; private set; } = new Dictionary<Side, LineSpecial>();

    public List<Cutout> Pillars { get; } = new List<Cutout>();

    public List<Room> InnerStructures { get; } = new List<Room>();

    public Room() : this(NoTheme.Instance) { }
    public Room(IThemed parent) 
    {
        Parent = parent;
    }

    public Room(IThemed parent, IEnumerable<Point> points)
    {
        Parent = parent;
        UpperLeft = new Point(points.Min(p => p.X), points.Max(p => p.Y));
        BottomRight = new Point(points.Max(p => p.X), points.Min(p => p.Y));
    }

    public Room Copy(IThemed newParent)
    {
        var copy = new Room(newParent)
        {
            Floor = Floor,
            Ceiling = Ceiling,
            CeilingTexture = CeilingTexture,
            FloorTexture = FloorTexture,
            WallTexture = WallTexture,
            Tag = Tag,
            Theme = Theme,
        };
        copy.ShapeModifiers.AddRange(ShapeModifiers);
        copy.InnerStructures.AddRange(InnerStructures.Select(p => p.Copy(copy)));
        copy.Pillars.AddRange(Pillars.Select(p => p.Copy()));
        copy.LineSpecials = LineSpecials.ToDictionary(k => k.Key, v => v.Value);
        copy.SideTextures = SideTextures.ToDictionary(k => k.Key, v => v.Value);
        return copy;
    }

    public TextureInfo TextureForSide(Side? side)
    {
        if (side == null)
            return WallTexture;

        var sideTexture = SideTextures.Try(side.Value);
        if (sideTexture == null)
            return WallTexture;
        else
            return sideTexture;
    }

    public Room RelativeTo(Room parent)
    {
        var copy = Copy(parent);
        copy.UpperLeft = parent.UpperLeft.Add(UpperLeft);
        copy.BottomRight = parent.UpperLeft.Add(BottomRight);
        copy.Floor = parent.Floor + Floor;
        copy.Ceiling = parent.Ceiling + Ceiling;            
        return copy;
    }
}
