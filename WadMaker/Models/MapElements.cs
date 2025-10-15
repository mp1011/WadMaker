namespace WadMaker.Models;

public interface IElementWrapper<T> where T : IMapElement
{
    T Data { get; }
}

public class Thing(thing Data) : IElementWrapper<thing>
{
    public thing Data { get; private set; } = Data;
    public ThingType ThingType => (ThingType)Data.type;

    public ThingCategory Category => ThingType.Category();

    public Angle Angle
    {
        get => new Angle(Data.angle);
        set => Data = Data with { angle = (int)value.Degrees };
    }

    public Point Position
    {
        get => new Point((int)Data.x, (int)Data.y);
        set
        {
            Data = Data with { x = value.X, y = value.Y };
        }
    }

    public DoomThingInfo ThingInfo => DoomConfig.DoomThingInfo.Try(Data.type) ?? DoomThingInfo.None;

    public int Radius => ThingInfo.Radius;

    public bool Overlaps(Thing other)
    {
        var distance = Position.DistanceTo(other.Position);
        return distance < (Radius + other.Radius) * .75; // still not sure exactly how close is too close
    }

    public override string ToString() => ThingInfo.Description;

    public Thing Copy(Room oldRoom, Room newRoom)
    {
        var relX = Data.x - oldRoom.UpperLeft.X;
        var relY = Data.y - oldRoom.UpperLeft.Y;

        return new Thing(Data with { x = newRoom.UpperLeft.X + relX, y = newRoom.UpperLeft.Y + relY });
    }
}

public class Sector(Room Room, sector Data) : IElementWrapper<sector>
{
    public Room Room { get; } = Room;

    public sector Data { get; private set; } = Data;

    public int Height => Data.heightceiling - Data.heightfloor;

    public int? Tag => Data.id;

    public LineDef[] Lines { get; set; } = Array.Empty<LineDef>();

    public LineDef[] Activators { get; set; } = Array.Empty<LineDef>();

    public ZDoomSectorSpecial SectorSpecial
    {
        get => (ZDoomSectorSpecial)(Data.special ?? 0);
        set => Data = Data with { special = (int)value };
    }

    public Flat Floor
    {
        get => Data.texturefloor.ParseAs<Flat>() ?? Flat.Default;
        set => Data = Data with { texturefloor = value.ToString() };
    }

    public Flat Ceiling
    {
        get => Data.textureceiling.ParseAs<Flat>() ?? Flat.Default;
        set => Data = Data with { textureceiling = value.ToString() };
    }

    public int FloorHeight
    {
        get => Data.heightfloor;
        set => Data = Data with { heightfloor = value };
    }

    public int CeilingHeight
    {
        get => Data.heightceiling;
        set => Data = Data with { heightceiling = value };
    }

    public override string ToString()
    {
        if (Data.id.HasValue)
            return $"Sector #{Data.id}";
        else
            return base.ToString() ?? "";
    }
}

public class SideDef(Sector Sector, sidedef Data) : IElementWrapper<sidedef>
{
    public sidedef Data { get; set; } = Data;

    public Sector Sector { get; set; } = Sector;

    public TextureInfo? TextureInfo { get; set; }

    public SideDef Copy()
    {
        return new SideDef(Sector, Data) { TextureInfo = TextureInfo };
    }

    public void Resolve(MapElements mapElements)
    {
        Data = Data with { sector = mapElements.Sectors.IndexOf(Sector) };
    }

    public string Texture => Data.texturemiddle ?? Data.texturebottom ?? Data.texturetop ?? "";
}

public class LineDef(vertex V1, vertex V2, SideDef Front, SideDef? Back, linedef Data) : IElementWrapper<linedef>
{
    public linedef Data { get; set; } = Data;
    public SideDef Front { get; private set; } = Front;
    public SideDef? Back { get; private set; } = Back;
    public vertex[] Vertices => new[] { V1, V2 };
    public vertex V1 { get; private set; } = V1;
    public vertex V2 { get; private set; } = V2;

    private TextureInfo? _texture;
    public TextureInfo TextureInfo
    {
        get =>  _texture ?? Front.TextureInfo ?? Back?.TextureInfo ?? new TextureInfo(Texture.STONE);
        set
        {
            _texture = value;
        }
    }

    public bool BlocksSounds
    {
        get => Data.blocksound.GetValueOrDefault();
        set
        {
            Data = Data with { blocksound = value ? true : null };
        }
    }

    public bool SingleSided => Back == null;
    public IEnumerable<Sector> Sectors
    {
        get
        {
            yield return Front.Sector;
            if (Back != null)
                yield return Back.Sector;
        }
    }

    public vertex OtherVertex(Point p) => OtherVertex(new vertex(p.X, p.Y));

    public vertex OtherVertex(vertex v)
    {
        if (v == V1)
            return V2;
        else if (v == V2)
            return V1;
        else
            throw new Exception($"Line does not contain vertex {v}");
    }

    public Point MidPoint => new Point((int)(V1.x + (V2.x - V1.x) / 2), (int)(V1.y + (V2.y - V1.y) / 2));

    /// <summary>
    /// Point slight in front of the midpoint
    /// </summary>
    public Point FrontTestPoint => MidPoint.Add(FrontAngle.AngleToPoint(4.0));


    /// <summary>
    /// Point slightly behind the midpoint
    /// </summary>
    public Point BackTestPoint => MidPoint.Add(FrontAngle.AngleToPoint(-4.0));

    private LineSpecial? _lineSpecial;
    public LineSpecial? LineSpecial
    {
        get => _lineSpecial;
        set
        {
            _lineSpecial = value;
            if (_lineSpecial != null)
                Data = _lineSpecial.ApplyTo(Data);
        }
    }

    public void FlipSides()
    {
        var v1 = V1;
        V1 = V2;
        V2 = v1;

        var backSector = Back!.Sector;
        Back.Sector = Front.Sector;
        Front.Sector = backSector;
    }

    public void FlipDirection()
    {
        var v1 = V1;
        V1 = V2;
        V2 = v1;
    }

    public void RemoveBack()
    {
        Back = null;
        Data = Data with { blocking = true, twosided = null };
    }

    public double Length => V1.DistanceTo(V2);

    public double FrontAngle => V1.FrontSidedefAngle(V2);

    public Angle Angle => new Angle(V1.AngleTo(V2));

    public bool BelongsTo(Sector sector)
    {
        return Front.Sector == sector || (Back != null && Back.Sector == sector);
    }

    /// <summary>
    /// Determines which side this line represents for this room
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public Side? SideOfRoom(Room room)
    {
        var center = room.Center;

        if (Vertices.All(v => v.x < center.X))
            return Side.Left;
        if (Vertices.All(v => v.x > center.X))
            return Side.Right;
        if (Vertices.All(v => v.y < center.Y))
            return Side.Bottom;
        if (Vertices.All(v => v.y > center.Y))
            return Side.Top;

        return null;
    }

    public LineDef SetTexture(Room room)
    {
        var side = SideOfRoom(room);
        var texture = room.TextureForSide(side);
        TextureInfo = texture;
        return this;
    }


    public IEnumerable<SideDef> SideDefs
    {
        get
        {
            yield return Front;
            if (Back != null)
                yield return Back;
        }
    }

    public override string ToString() => $"#{V1} - #{V2}";

    public void Resolve(MapElements mapElements)
    {
        Data = Data with
        {
            v1 = mapElements.Vertices.IndexOf(V1),
            v2 = mapElements.Vertices.IndexOf(V2),
            sidefront = mapElements.SideDefs.IndexOf(Front),
            sideback = Back != null ? mapElements.SideDefs.IndexOf(Back) : null,
        };
    }

    public vertex[] OverlappingVertices(LineDef other)
    {
        return Vertices.Where(v => v.Intersects(other))
                                       .Union(other.Vertices
                                       .Where(v => v.Intersects(this)))
                                       .ToArray();
    }

    public bool Contains(vertex vertex)
    {
        return V1.Equals(vertex) || V2.Equals(vertex);
    }

    public bool Contains(Point point)
    {
        return ((int)V1.x == point.X && (int)V1.y == point.Y)
            || ((int)V2.x == point.X && (int)V2.y == point.Y);
    }

    public bool Overlaps(LineDef other)
    {
        return OverlappingVertices(other).Length > 1;
    }

    public bool Crosses(LineDef other) => IntersectionPoint(other) != null;

    public Point? IntersectionPoint(LineDef other)
    {
        //copilot authored

        // Line 1: (x1, y1) to (x2, y2)
        // Line 2: (x3, y3) to (x4, y4)
        double x1 = V1.x, y1 = V1.y;
        double x2 = V2.x, y2 = V2.y;
        double x3 = other.V1.x, y3 = other.V1.y;
        double x4 = other.V2.x, y4 = other.V2.y;

        double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (Math.Abs(denom) < double.Epsilon)
            return null; // Lines are parallel or coincident

        double pre = (x1 * y2 - y1 * x2);
        double post = (x3 * y4 - y3 * x4);
        double x = (pre * (x3 - x4) - (x1 - x2) * post) / denom;
        double y = (pre * (y3 - y4) - (y1 - y2) * post) / denom;

        // Check if intersection is within both segments
        bool onThis =
            Math.Min(x1, x2) - double.Epsilon <= x && x <= Math.Max(x1, x2) + double.Epsilon &&
            Math.Min(y1, y2) - double.Epsilon <= y && y <= Math.Max(y1, y2) + double.Epsilon;
        bool onOther =
            Math.Min(x3, x4) - double.Epsilon <= x && x <= Math.Max(x3, x4) + double.Epsilon &&
            Math.Min(y3, y4) - double.Epsilon <= y && y <= Math.Max(y3, y4) + double.Epsilon;

        if (onThis && onOther)
        {
            var pt = new Point((int)Math.Round(x), (int)Math.Round(y));
            if (pt == V1 || pt == V2 || pt == other.V1 || pt == other.V2)
                return null;
            else
                return pt;
        }
        else
            return null;
    }

    public bool InSamePositionAs(LineDef other)
    {
        return (V1.Equals(other.V1) && V2.Equals(other.V2)) || (V2.Equals(other.V1) && V1.Equals(other.V2));
    }
}

public class MapElements
{
    public List<vertex> Vertices { get; set; } = new List<vertex>();
    public List<Sector> Sectors { get; set; } = new List<Sector>();
    public List<SideDef> SideDefs { get; set; } = new List<SideDef>();
    public List<LineDef> LineDefs { get; set; } = new List<LineDef>();
    public List<Thing> Things { get; set; } = new List<Thing>();

    public MapElements Merge(MapElements other)
    {
        Vertices.AddRange(other.Vertices);
        Sectors.AddRange(other.Sectors);
        SideDefs.AddRange(other.SideDefs);
        LineDefs.AddRange(other.LineDefs);
        Things.AddRange(other.Things);

        return this;
    }
}