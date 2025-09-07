using WadMaker.Models;
using WadMaker.Models.LineSpecials;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WadMaker.Tests.Services;

internal class MapBuilderTests : StandardTest
{
    [Test]
    public void CanCreatePillarAlignedWithWall()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(300, -300)
        });

        map.Rooms[0].Pillars.Add(new Cutout
        {
            UpperLeft = new Point(100, 0),
            BottomRight = new Point(200, -50),
        });

        var mapElements = MapBuilder.Build(map);
        var topLines = mapElements.LineDefs.Where(p => p.V1.y == 0 && p.V2.y == 0).ToArray();
        Assert.That(topLines, Has.Length.EqualTo(2));
        // probably should test more things....
    }

    [Test]
    public void CanCreateNestedInnerStructures()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400)
        });

        map.Rooms[0].InnerStructures.Add(new Room
        {
            UpperLeft = new Point(100, -100),
            BottomRight = new Point(300, -300),
            Floor = 16,
            Ceiling = 0,
        });

        map.Rooms[0].InnerStructures[0].InnerStructures.Add(new Room
        {
            UpperLeft = new Point(50, -50),
            BottomRight = new Point(100, -100),
            Floor = 16,
            Ceiling = 0,
        });

        var mapElements = MapBuilder.Build(map);
        Assert.That(mapElements.Sectors, Has.Count.EqualTo(3));
        Assert.That(mapElements.LineDefs, Has.Count.EqualTo(12));

        var outerLines = mapElements.LineDefs.Take(4).ToArray();
        Assert.That(outerLines.All(p => p.Back == null));
        Assert.That(outerLines.All(p => p.Front.Sector.Data == mapElements.Sectors[0].Data));


        var firstInnerLines = mapElements.LineDefs.Skip(4).Take(4).ToArray();
        Assert.That(firstInnerLines.All(p => p.Front.Sector.Data == mapElements.Sectors[2].Data));
        Assert.That(firstInnerLines.All(p => p.Back!.Sector.Data == mapElements.Sectors[1].Data));

        var secondInnerLines = mapElements.LineDefs.Skip(8).Take(4).ToArray();
        Assert.That(secondInnerLines.All(p => p.Front.Sector.Data == mapElements.Sectors[1].Data));
        Assert.That(secondInnerLines.All(p => p.Back!.Sector.Data == mapElements.Sectors[0].Data));
    }

    [TestCase(512, 0, 768, -256)] // right
    [TestCase(-512, 0, -312, -256)] // left
    [TestCase(0, 500, 256, 300)] // top
    [TestCase(0, -500, 256, -700)] // bottom
    public void CanCreateHallWithDoor(int ulX, int ulY, int brX, int brY)
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(ulX, ulY),
            BottomRight = new Point(brX, brY)
        });
        var hallGenerator = new HallGenerator();
        map.Rooms.Add(hallGenerator.GenerateHall(
            new Hall(128,
            map.Rooms[0],
            map.Rooms[1],
            Door: new Door(16, Texture.BIGDOOR2, Texture.DOORTRAK, 64))));

        var mapElements = MapBuilder.Build(map);

        Assert.That(mapElements.SideDefs.Count, Is.EqualTo(24));
        
        var doorSector = mapElements.Sectors.Last();

        var doorLines = mapElements.LineDefs.Where(p => p.BelongsTo(doorSector)).ToArray();
        var doorTracks = doorLines.Where(p => p.Length == 16).ToArray();
        var doorFaces = doorLines.Where(p => p.Length == 128).ToList();

        Assert.That(doorTracks[0].Back, Is.Null);
        Assert.That(doorTracks[1].Back, Is.Null);

        Assert.That(doorTracks[0].Data.dontpegbottom, Is.True);
        Assert.That(doorTracks[1].Data.dontpegbottom, Is.True);

        Assert.That(doorTracks[0].Front.Data.texturemiddle, Is.EqualTo(Texture.DOORTRAK.ToString()));
        Assert.That(doorTracks[1].Front.Data.texturemiddle, Is.EqualTo(Texture.DOORTRAK.ToString()));

        Assert.That(doorFaces[0].Back!.Sector, Is.EqualTo(doorSector));
        Assert.That(doorFaces[1].Back!.Sector, Is.EqualTo(doorSector));

        Assert.That(doorFaces[0].Data.special, Is.EqualTo((int)LineSpecialType.DoorRaise));
        Assert.That(doorFaces[1].Data.special, Is.EqualTo((int)LineSpecialType.DoorRaise));
    }

    [Test]
    public void CanCreateRoomWithAlcove()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400),
        };

        var alcove = RoomGenerator.AddStructure(room,
            new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
            Side: Side.Left,
            Width: 100,
            Depth: 32,
            CenterPercent: 0.50));

        var map = new Map();
        map.Rooms.Add(room);

        var mapElements = MapBuilder.Build(map);

        var twoSidedLines = mapElements.LineDefs.Where(p => p.Back != null).ToArray();

        Assert.That(twoSidedLines.Length, Is.EqualTo(1));
    }

    [Test]
    public void CanCreateRoomWithCornerStructure()
    {
        var map = new Map();
        var room = map.AddRoom(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400),
        });
        room.Tag = 1;

        room.AddInnerStructure(new Room { UpperLeft = Point.Empty, BottomRight = new Point(64, -64) });
        room.InnerStructures[0].Tag = 2;

        var mapElements = MapBuilder.Build(map);
        Assert.That(mapElements.Sectors[0].Lines.Count, Is.EqualTo(6));
        Assert.That(mapElements.Sectors[1].Lines.Count, Is.EqualTo(4));

    }
}
