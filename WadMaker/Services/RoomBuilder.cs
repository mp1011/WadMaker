using WadMaker.Models.BuildingBlocks;

namespace WadMaker.Services;
public class RoomBuilder
{
    private readonly IDProvider _idProvider;
    private readonly IsPointInSector _isPointInSector;

    public RoomBuilder(IDProvider idProvider, IsPointInSector isPointInSector)
    {
        _idProvider = idProvider;
        _isPointInSector = isPointInSector;
    }

    public MapElements Build(Room room)
    {
        var roomElements = BuildRoomSector(room);
        var roomSector = roomElements.Sectors[0];

        foreach (var pillar in room.Pillars)
        {
            roomElements.Merge(BuildPillar(room, roomSector, pillar));
        }

        foreach (var innerElement in room.InnerStructures)
        {
            roomElements.Merge(BuildInternalElement(room, roomSector, innerElement, roomElements));
        }

        AddLineSpecials(room, roomElements);

        foreach (var thing in room.Things)
        {
          roomElements.Things.Add(thing);
        } 
        return roomElements;
    }

    private MapElements BuildRoomSector(Room room)
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

    public MapElements BuildPillar(Room room, Sector roomSector, Cutout pillar)
    {
        pillar = pillar.RelativeTo(room);
        var elements = new MapElements();
        var vertices = Vertices(pillar).ToArray();
        elements.Vertices.AddRange(vertices);

        var sidedefs = SideDefs(pillar, vertices, roomSector).ToArray();
        elements.SideDefs.AddRange(sidedefs);

        elements.LineDefs.AddRange(LineDefsOutward(vertices, sidedefs));

        return elements;
    }

    public MapElements BuildInternalElement(Room room, Sector roomSector, Room innerElement, MapElements elements)
    {
        innerElement = innerElement.RelativeTo(room);
        var innerElements = Build(innerElement);
        
        var lines = innerElements.LineDefs.Where(p => p.Front.Sector == innerElements.Sectors[0] && p.Back == null).ToArray();
        innerElements.LineDefs.RemoveMany(lines);
        innerElements.SideDefs.RemoveMany(lines.SelectMany(p => p.SideDefs));

        var sectors = new[] { roomSector, innerElements.Sectors[0] };

        foreach (var line in lines)
        {
            var backBelongsToRoom = _isPointInSector.Execute(line.BackTestPoint, roomSector, elements);
            if (backBelongsToRoom || StaticFlags.InnerSectorLinesAlwaysStartTwoSided)
            {
                var lineSide = line.SideOfRoom(innerElement);
                var backSide = new SideDef(roomSector, new sidedef(sector: -1));
                var frontSide = new SideDef(innerElements.Sectors.First(), new sidedef(sector: -1));

                var newLine = new LineDef(line.V1, line.V2,
                        frontSide,
                        backSide,
                        line.Data with { twosided = true, blocking = false });

                innerElement.TextureForSide(lineSide).ApplyTo(newLine);
                newLine.LineSpecial = line.LineSpecial;

                innerElements.LineDefs.Add(newLine);

                innerElements.SideDefs.Add(frontSide);
                innerElements.SideDefs.Add(backSide);
            }
            else
            {
                var lineSide = line.SideOfRoom(innerElement);

                var frontSide = new SideDef(innerElements.Sectors.First(), new sidedef(sector: -1));

                var newLine = new LineDef(line.V1, line.V2,
                        frontSide,
                        null,
                        line.Data with { twosided = false, blocking = true });
                newLine.LineSpecial = line.LineSpecial;

                innerElement.TextureForSide(lineSide).ApplyTo(newLine);
                innerElements.LineDefs.Add(newLine);
                innerElements.SideDefs.Add(frontSide);
            }
        }

        return innerElements;
    }

    private void AddLineSpecials(Room room, MapElements elements)
    {
        foreach (var lineSpecial in room.LineSpecials)
        {
            AddLineSpecials(lineSpecial.Key, lineSpecial.Value, room, elements);
        }           
    }

    private void AddLineSpecials(Side side, LineSpecial lineSpecial, Room room, MapElements elements)
    {
        var sectorLines = elements.LineDefs.Where(l => l.BelongsTo(elements.Sectors[0])).ToArray();

        var sideLine = sectorLines.FirstOrDefault(l => l.SideOfRoom(room) == side);
        if (sideLine == null)
            return;

        sideLine.LineSpecial = lineSpecial;   
    }

    private Sector Sector(Room room) => new Sector(room, new sector(
        special: room.SectorSpecial == ZDoomSectorSpecial.Normal ? null : (int?)room.SectorSpecial,
        texturefloor: room.FloorTexture.ToString(),
        textureceiling: room.CeilingTexture.ToString(),
        heightfloor: room.Floor,
        id: room.Tag,
        heightceiling: room.Floor + room.VerticalHeight,
        lightlevel: 127));

    private IEnumerable<vertex> Vertices(IShape room)
    {
        return room.GetPoints().Select(p => new vertex(p.X, p.Y));
    }

    public IEnumerable<SideDef> SideDefs(Room room, vertex[] vertices, Sector sector)
    {
        return vertices.Select(v => new SideDef(Sector: sector, Data: new sidedef(sector: -1)));
    }

    public IEnumerable<SideDef> SideDefs(Cutout cutout, vertex[] vertices, Sector sector)
    {
        return vertices.Select(v => new SideDef(Sector: sector, Data: new sidedef(sector: -1, texturemiddle: cutout.WallTexture.ToString())));
    }

    public IEnumerable<LineDef> LineDefs(Room room, vertex[] vertices, SideDef[] sidedefs)
    {
        return vertices.WithNeighbors().Select((v, index) =>
        {
            var line = new LineDef(V1: v.Item2, V2: v.Item3, Front: sidedefs[index], Back: null, Data: new linedef(blocking: true));
            var lineTexture = room.TextureForSide(line.SideOfRoom(room));
            lineTexture?.ApplyTo(line);
            return line;
        });        
    }

    public IEnumerable<LineDef> LineDefsOutward(vertex[] vertices, SideDef[] sidedefs)
    {
        return vertices.Reverse().ToArray().WithNeighbors().Select((v, index) =>
            new LineDef(V1: v.Item2, V2: v.Item3, Front: sidedefs[index], Back: null, Data: new linedef(blocking: true)));
    }
}

