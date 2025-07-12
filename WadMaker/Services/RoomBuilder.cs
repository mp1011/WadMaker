namespace WadMaker.Services;
public class RoomBuilder
{
    private readonly IDProvider _idProvider;

    public RoomBuilder(IDProvider idProvider)
    {
        _idProvider = idProvider;
    }

    public MapElements Build(Room room)
    {
        var elements = new MapElements();  
     
        var sector = Sector(room);
        elements.Sectors.Add(sector);

        var vertices = Vertices(room).ToArray();
        elements.Vertices.AddRange(vertices);

        var sidedefs = SideDefs(room, vertices, sector).ToArray();
        elements.SideDefs.AddRange(sidedefs);

        elements.LineDefs.AddRange(LineDefs(room, vertices, sidedefs));

        return elements;
    }

    private Sector Sector(Room room) => new Sector(new sector(
        texturefloor: room.FloorTexture.ToString(),
        textureceiling: room.CeilingTexture.ToString(),
        heightfloor: room.Floor,
        heightceiling: room.Floor + room.Height,
        lightlevel: 127));

    private IEnumerable<vertex> Vertices(Room room)
    {
        var initialPoints = new[]
        {
            new Point(room.UpperLeft.X, room.UpperLeft.Y),
            new Point(room.BottomRight.X, room.UpperLeft.Y),
            new Point(room.BottomRight.X, room.BottomRight.Y),
            new Point(room.UpperLeft.X, room.BottomRight.Y),
        };

        var points = room.ShapeModifiers.Aggregate(initialPoints, 
            (p, s) => s.AlterPoints(p, room));

        return points.Select(p => new vertex(p.X, p.Y));
    }

    public IEnumerable<SideDef> SideDefs(Room room, vertex[] vertices, Sector sector)
    {
        return vertices.Select(v => new SideDef(Sector: sector, Data: new sidedef(sector: -1, texturemiddle: room.WallTexture.ToString())));
    }

    public IEnumerable<LineDef> LineDefs(Room room, vertex[] vertices, SideDef[] sidedefs)
    {
        return vertices.WithNeighbors().Select((v,index) =>
            new LineDef(V1: v.Item2, V2: v.Item3, Front: sidedefs[index], Back: null, Data: new linedef(blocking: true)));
    }
}

