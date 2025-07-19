using WadMaker.Models;

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

    [Test]
    public void CanCreateHallWithDoor()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(512, 0),
            BottomRight = new Point(768, -256)
        });
        var hallGenerator = new HallGenerator();
        map.Rooms.Add(hallGenerator.GenerateHall(
            new Hall(128,
            map.Rooms[0],
            map.Rooms[1],
            Door: new Door(16, Texture.BIGDOOR2, 64))));

        var mapElements = MapBuilder.Build(map);
        
        var doorSector = mapElements.Sectors.Last();

        var doorLines = mapElements.LineDefs.Where(p => p.BelongsTo(doorSector)).ToArray();
        var doorTracks = doorLines.Where(p => p.Length == 16).ToArray();
        var doorFaces = doorLines.Where(p => p.Length == 128).ToList();

        Assert.That(doorTracks[0].Back, Is.Null);
        Assert.That(doorTracks[1].Back, Is.Null);

        Assert.That(doorFaces[0].Back!.Sector, Is.EqualTo(doorSector));
        Assert.That(doorFaces[1].Back!.Sector, Is.EqualTo(doorSector));
    }
}
