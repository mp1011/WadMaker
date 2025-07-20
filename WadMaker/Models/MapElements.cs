namespace WadMaker.Models;

public interface IElementWrapper<T> where T:IMapElement
{
    T Data { get; }
}

public class Thing(thing Data) : IElementWrapper<thing>
{
    public thing Data { get; private set; } = Data;
}

public class Sector(sector Data) : IElementWrapper<sector>
{
    public sector Data { get; private set; } = Data;
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
}

public class LineDef(vertex V1, vertex V2, SideDef Front, SideDef? Back, linedef Data) : IElementWrapper<linedef>
{
    public linedef Data { get; set; } = Data;
    public SideDef Front { get; private set; } = Front;
    public SideDef? Back { get; private set; } = Back;
    public vertex[] Vertices => new[] { V1, V2 };
    public vertex V1 { get; private set; } = V1;
    public vertex V2 { get; private set; } = V2;

    private LineSpecial? _lineSpecial;  
    public LineSpecial?  LineSpecial
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

        if(Vertices.All(v=> v.x < center.X))
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
        Data = Data with {
            v1 = mapElements.Vertices.IndexOf(V1),
            v2 = mapElements.Vertices.IndexOf(V2),
            sidefront = mapElements.SideDefs.IndexOf(Front),
            sideback = Back != null ? mapElements.SideDefs.IndexOf(Back) : null,
        };
    }

    public vertex[]  OverlappingVertices(LineDef other)
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

        return this;
    }
}