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

    public override string ToString()
    {
        if(Data.id.HasValue)
            return $"Sector #{Data.id}";
        else 
            return base.ToString() ?? "";
    }
}

public class SideDef(Sector Sector, sidedef Data) : IElementWrapper<sidedef>
{
    public sidedef Data { get; set; } = Data;

    public Sector Sector { get; set; } = Sector;

    public SideDef Copy()
    {
        return new SideDef(Sector, Data);
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

    public IEnumerable<Sector> Sectors
    {
        get
        {
            yield return Front.Sector;
            if (Back != null)
                yield return Back.Sector;
        }
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

    public double Angle => V1.AngleTo(V2);

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

    public LineDef ApplyTexture(Room room)
    {
        var side = SideOfRoom(room);
        var texture = room.TextureForSide(side);
        texture.ApplyTo(this);
        return this;
    }

    public LineDef ApplyTexture(TextureInfo texture)
    {
        texture.ApplyTo(this);
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