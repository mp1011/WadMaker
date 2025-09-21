namespace WadMaker.Models;

public class Room : IShape, IThemed
{    
    public IThemed Parent { get; }

    public List<Thing> Things{ get; } = new List<Thing>();

    public List<IShapeModifier> ShapeModifiers { get; } = new List<IShapeModifier>();

    public Point UpperLeft { get; set; } = Point.Empty;
    public Point BottomRight { get; set; } = Point.Empty;

    public Point Center => Bounds.Center;

    public DRectangle Bounds => this.Bounds();
    public int VerticalHeight => Ceiling - Floor;

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

    // Added property
    public ZDoomSectorSpecial SectorSpecial { get; set; } = ZDoomSectorSpecial.Normal;

    public Room() : this(NoTheme.Instance) { }
    public Room(IThemed parent) 
    {
        Parent = parent;
    }

    public Room(IThemed parent, Point? center = null, Size? size = null) : this(parent)
    {
        center ??= Point.Empty;
        size ??= new Size(128, 128);

        UpperLeft = new Point(center.Value.X - size.Value.Width / 2, center.Value.Y + size.Value.Height / 2);
        BottomRight = new Point(center.Value.X + size.Value.Width / 2, center.Value.Y - size.Value.Height / 2);
    }

    public Room(IThemed parent, IEnumerable<Point> points)
    {
        Parent = parent;
        SetFromVertices(points);
    }

    public void SetFromVertices(IEnumerable<Point> points)
    {
        UpperLeft = new Point(points.Min(p => p.X), points.Max(p => p.Y));
        BottomRight = new Point(points.Max(p => p.X), points.Min(p => p.Y));
    }

    public Room AddInnerStructure(Room room)
    {
        InnerStructures.Add(room);
        return room;
    }

    public Room Copy(IThemed newParent)
    {
        var copy = new Room(newParent)
        {
            UpperLeft = UpperLeft,
            BottomRight = BottomRight,
            Floor = Floor,
            Ceiling = Ceiling,
            CeilingTexture = CeilingTexture,
            FloorTexture = FloorTexture,
            WallTexture = WallTexture,
            Tag = Tag,
            Theme = Theme,
            SectorSpecial = SectorSpecial
        };
        copy.Things.AddRange(Things.Select(t => t.Copy(this, copy)));
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
        copy.Things.Clear();

        copy.UpperLeft = parent.UpperLeft.Add(UpperLeft);
        copy.BottomRight = parent.UpperLeft.Add(BottomRight);
        copy.Things.AddRange(Things.Select(t => t.Copy(this, copy)));  
        copy.Floor = parent.Floor + Floor;
        copy.Ceiling = parent.Ceiling + Ceiling;            
        return copy;
    }
}
