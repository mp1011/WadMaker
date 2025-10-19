using WadMaker.Models.Theming;

namespace WadMaker.Models;

public class Room : IWithShape, IThemed
{    
    public IThemed Parent { get; }

    public Shape Shape { get; private set; }

    public RoomBuildingBlock? BuildingBlock { get; set; }

    public List<Thing> Things{ get; } = new List<Thing>();

    public Point UpperLeft
    {
        get => Shape.UpperLeft;
        set => Shape.UpperLeft = value;
    }

    public Point BottomRight
    {
        get => Shape.BottomRight;
        set => Shape.BottomRight = value;
    }

    public Point Center
    {
        get => Shape.Center;
        set => Shape.Center = value;        
    }

    /// <summary>
    /// Note, center point is preserved on size change
    /// </summary>
    public Size Size
    {
        get => Bounds.Size;
        set
        {
            var center = Center;
            UpperLeft = new Point(center.X - value.Width / 2, center.Y + value.Height / 2);
            BottomRight = new Point(center.X + value.Width / 2, center.Y - value.Height / 2);
        }
    }

    public DRectangle Bounds => this.Bounds();
    public int VerticalHeight => Ceiling - Floor;

    public int Ceiling { get; set; } = 128;
    public int Floor { get; set; } = 0;    
    public bool BlocksSound { get; set; }

    public FlatsQuery FloorTexture { get; set; } = FlatsQuery.Default;
    public FlatsQuery CeilingTexture { get; set; } = FlatsQuery.Default;

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

    private List<Cutout> _pillars = new List<Cutout>();
    public IEnumerable<Cutout> Pillars => _pillars;

    public Cutout AddPillar(Cutout cutout)
    {
        _pillars.Add(cutout);
        cutout.Shape.RelativeTo = this.Shape;
        return cutout;
    }

    public Cutout AddPillar(Size? size = null) => AddPillar(new Cutout(size: size));

    private List<Room> _innerStructures = new List<Room>();
    public IEnumerable<Room> InnerStructures => _innerStructures;

    public ZDoomSectorSpecial SectorSpecial { get; set; } = ZDoomSectorSpecial.Normal;

    public int LightLevel { get; set; } = 127;

    public Room() : this(NoTheme.Instance) { }

    public Room(IThemed parent, Point? center = null, Size? size = null)
    {
        Parent = parent;
        Shape = new Shape(center: center, size: size);
    }

    public Room(IThemed parent, IEnumerable<Point> points)
    {
        Parent = parent;
        Shape = new Shape();
        SetFromVertices(points);
    }

    public Room AddTo(Map map)
    {
        map.Rooms.Add(this);
        return this;
    }

    public void SetFromVertices(IEnumerable<Point> points)
    {
        UpperLeft = new Point(points.Min(p => p.X), points.Max(p => p.Y));
        BottomRight = new Point(points.Max(p => p.X), points.Min(p => p.Y));
    }

    public IEnumerable<Room> AddInnerStructures(IEnumerable<Room> rooms)
    {
        foreach (var room in rooms)
            AddInnerStructure(room);
        return rooms;
    }

    public Room AddInnerStructure(Room room)
    {
        if (_innerStructures.Contains(room))
            throw new Exception("Attempted to add same structure twice");
        room.Shape.RelativeTo = this.Shape;
        _innerStructures.Add(room);
        return room;
    }

    public Room AddInnerStructure(Point? center = null, Size? size = null) => AddInnerStructure(new Room(this, center, size));

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
            SectorSpecial = SectorSpecial,
            BuildingBlock = BuildingBlock,
            BlocksSound = BlocksSound,
            LightLevel = LightLevel,
        };
        copy.Shape = Shape.Copy();

        copy.Things.AddRange(Things.Select(t => t.Copy(this, copy)));
        copy._innerStructures.AddRange(InnerStructures.Select(p => p.Copy(copy)));
        copy._pillars.AddRange(Pillars.Select(p => p.Copy()));
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

    public void MatchFloorAndCeilingTo(Room other, int floorAdjust=0, int ceilingAdjust=0)
    {
        Floor = other.Floor + floorAdjust;
        Ceiling = other.Ceiling + ceilingAdjust;
    }
}
