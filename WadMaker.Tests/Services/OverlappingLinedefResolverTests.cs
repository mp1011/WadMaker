using System.Net;

namespace WadMaker.Tests.Services;


class OverlappingLinedefResolverTests : StandardTest
{
    [Test]
    public void CanResolveOverlappingLinedefs()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = new TextureInfo(Main: Texture.STONE),
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });

        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = new TextureInfo(Main: Texture.STONE),
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(128, -32),
            BottomRight = new Point(256, -200)
        });

        var roomBuilder = new RoomBuilder(new IDProvider());
        var mapElements = roomBuilder.Build(map.Rooms[0]).Merge(roomBuilder.Build(map.Rooms[1]));

        var overlappingLinedefResolver = new OverlappingLinedefResolver(new TestAnnotator(), new IsPointInSector());
        var results = overlappingLinedefResolver.Execute(mapElements).ToArray();

        Assert.That(results.Count(), Is.EqualTo(3));
        Assert.That(results[0].Front.Sector, Is.EqualTo(mapElements.Sectors[0]));

        Assert.That(results[1].Front.Sector, Is.EqualTo(mapElements.Sectors[0]));
        Assert.That(results[1].Back!.Sector, Is.EqualTo(mapElements.Sectors[1]));
        Assert.That(results[1].Data.twosided, Is.True);

        Assert.That(results[1].Front.Data.texturemiddle, Is.Null);
        Assert.That(results[1].Back!.Data.texturemiddle, Is.Null);
        Assert.That(results[1].Front.Data.texturebottom, Is.Not.Empty);
        Assert.That(results[1].Back!.Data.texturebottom, Is.Not.Empty);
        Assert.That(results[1].Front.Data.texturetop, Is.Not.Empty);
        Assert.That(results[1].Back!.Data.texturetop, Is.Not.Empty);

        Assert.That(results[2].Front.Sector, Is.EqualTo(mapElements.Sectors[0]));
    }

    [Test]
    public void SingleSideMerge()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = new TextureInfo(Main: Texture.STONE),
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -128)
        });

        map.Rooms[0].InnerStructures.Add(
            new Room
            {
                Floor = 0,
                Ceiling = 256,
                WallTexture = new TextureInfo(Main: Texture.STONE),
                FloorTexture = Flat.FLOOR0_1,
                CeilingTexture = Flat.FLOOR0_3,
                UpperLeft = new Point(80, 0),
                BottomRight = new Point(100, -32)
            });

        var mapElements = MapBuilder.Build(map);

        Assert.That(mapElements.LineDefs.Count(p=>p.Back == null), Is.EqualTo(6));
    }

    [Test]
    public void CanResolveAdjacentInnerSectors()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(300, -128)
        });

        map.Rooms[0].InnerStructures.Add(
            new Room
            {
                UpperLeft = new Point(50, 0),
                BottomRight = new Point(100, -128),
            });

        map.Rooms[0].InnerStructures.Add(
           new Room
           {
               UpperLeft = new Point(100, 0),
               BottomRight = new Point(150, -128),
           });

        var mapElements = MapBuilder.Build(map);

        Assert.That(mapElements.LineDefs.Count(p => p.Back != null), Is.EqualTo(3));
        Assert.That(mapElements.LineDefs.Count(p => p.Back == null), Is.EqualTo(10));

    }

    [Test]
    public void RoomSplitInTwo()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(200, -128)
        });

        map.Rooms[0].InnerStructures.Add(
            new Room
            {
                UpperLeft = new Point(100, 0),
                BottomRight = new Point(200, -128),
            });

        var mapElements = MapBuilder.Build(map);

        Assert.That(mapElements.LineDefs.Count(p => p.Back != null), Is.EqualTo(1));
        Assert.That(mapElements.LineDefs.Count(p => p.Back == null), Is.EqualTo(6));

    }
}
