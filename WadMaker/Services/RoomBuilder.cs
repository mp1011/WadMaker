using System.ComponentModel;
using System.Numerics;
using WadMaker.Models;

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
        var roomElements = BuildRoomSector(room);
        var roomSector = roomElements.Sectors[0];

        foreach (var pillar in room.Pillars)
        {
            roomElements.Merge(BuildPillar(room, roomSector, pillar));
        }

        foreach (var innerElement in room.InnerStructures)
        {
            roomElements.Merge(BuildInternalElement(room, roomSector, innerElement));
        }

        AddLineSpecials(room, roomElements);
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

    public MapElements BuildInternalElement(Room room, Sector roomSector, Room innerElement)
    {
        innerElement = innerElement.RelativeTo(room);
        var innerElements = Build(innerElement);
        
        var lines = innerElements.LineDefs.Where(p => p.Front.Sector == innerElements.Sectors[0] && p.Back == null).ToArray();
        innerElements.LineDefs.RemoveMany(lines);
        innerElements.SideDefs.RemoveMany(lines.SelectMany(p => p.SideDefs));

        foreach (var line in lines)
        {
            var lineSide = line.SideOfRoom(innerElement);
            var backSide = new SideDef(roomSector, new sidedef(sector: -1, texturemiddle: null, 
                texturetop: innerElement.TextureForSide(lineSide).ToString(),
                texturebottom: innerElement.TextureForSide(lineSide).ToString()));

            var frontSide = new SideDef(innerElements.Sectors.First(), new sidedef(sector: -1, texturemiddle: null,
                texturetop: innerElement.TextureForSide(lineSide).ToString(),
                texturebottom: innerElement.TextureForSide(lineSide).ToString()));

            var newLine = new LineDef(line.V1, line.V2,
                    frontSide,
                    backSide,
                    line.Data with { twosided = true, blocking = false });
            newLine.LineSpecial = line.LineSpecial;

            innerElements.LineDefs.Add(newLine);

            innerElements.SideDefs.Add(frontSide);
            innerElements.SideDefs.Add(backSide);
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

    private Sector Sector(Room room) => new Sector(new sector(
        texturefloor: room.FloorTexture.ToString(),
        textureceiling: room.CeilingTexture.ToString(),
        heightfloor: room.Floor,
        heightceiling: room.Floor + room.Height,
        lightlevel: 127));

    private IEnumerable<vertex> Vertices(IShape room)
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

