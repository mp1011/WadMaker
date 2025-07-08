namespace WadMaker.Models;

public interface IElementWrapper<T> where T:IMapElement
{
    T Data { get; }
}

public class Sector(sector Data) : IElementWrapper<sector>
{
    public sector Data { get; private set; } = Data;
}

public class SideDef(Sector Sector, sidedef Data) : IElementWrapper<sidedef>
{
    public sidedef Data { get; private set; } = Data;

    public Sector Sector { get; private set; } = Sector;

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
    public linedef Data { get; private set; } = Data;
    public SideDef Front { get; private set; } = Front;
    public SideDef? Back { get; private set; } = Back;
    public vertex[] Vertices => new[] { V1, V2 };
    public vertex V1 { get; private set; } = V1;
    public vertex V2 { get; private set; } = V2;

    public IEnumerable<SideDef> SideDefs     {
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

    public MapElements Merge(MapElements other)
    {
        Vertices.AddRange(other.Vertices);
        Sectors.AddRange(other.Sectors);
        SideDefs.AddRange(other.SideDefs);
        LineDefs.AddRange(other.LineDefs);

        return this;
    }
}