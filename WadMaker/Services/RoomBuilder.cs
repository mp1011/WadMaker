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

        var sidedefs = SideDefs(room, sector).ToArray();
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
        yield return new vertex(room.UpperLeft.X, room.UpperLeft.Y);
        yield return new vertex(room.BottomRight.X, room.UpperLeft.Y);
        yield return new vertex(room.BottomRight.X, room.BottomRight.Y);
        yield return new vertex(room.UpperLeft.X, room.BottomRight.Y);
    }

    public IEnumerable<SideDef> SideDefs(Room room, Sector sector)
    {
        yield return new SideDef(Sector: sector, Data: new sidedef(sector: -1, texturemiddle: room.WallTexture.ToString()));
        yield return new SideDef(Sector: sector, Data: new sidedef(sector: -1, texturemiddle: room.WallTexture.ToString()));
        yield return new SideDef(Sector: sector, Data: new sidedef(sector: -1, texturemiddle: room.WallTexture.ToString()));
        yield return new SideDef(Sector: sector, Data: new sidedef(sector: -1, texturemiddle: room.WallTexture.ToString()));
    }

    public IEnumerable<LineDef> LineDefs(Room room, vertex[] vertices, SideDef[] sidedefs)
    {
        yield return new LineDef(V1: vertices[0], V2: vertices[1], Front: sidedefs[0], Back: null, Data: new linedef(blocking: true));
        yield return new LineDef(V1: vertices[1], V2: vertices[2], Front: sidedefs[1], Back: null, Data: new linedef(blocking: true));
        yield return new LineDef(V1: vertices[2], V2: vertices[3], Front: sidedefs[2], Back: null, Data: new linedef(blocking: true));
        yield return new LineDef(V1: vertices[3], V2: vertices[0], Front: sidedefs[3], Back: null, Data: new linedef(blocking: true));
    }
}

