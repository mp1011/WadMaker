namespace WadMaker.Tests.Services;

internal class RoomGeneratorTests : StandardTest
{
    [Test]
    public void CanAddAlcove()
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

        Assert.That(alcove.Bounds.Width, Is.EqualTo(32));
        Assert.That(alcove.Bounds.Height, Is.EqualTo(100));

        Assert.That(alcove.UpperLeft.X, Is.EqualTo(-32));
        Assert.That(alcove.BottomRight.X, Is.EqualTo(0));
    }

    [Test]
    public void CanAddNorthAlcove()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -300),
        };

        var alcove = RoomGenerator.AddStructure(room,
            new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
            Side: Side.Top,
            Width: 100,
            Depth: 32,
            CenterPercent: 0.50));

        Assert.That(alcove.Bounds.Height, Is.EqualTo(32));
        Assert.That(alcove.Bounds.Width, Is.EqualTo(100));

        Assert.That(alcove.UpperLeft.X, Is.EqualTo(150));
   }


    [Test]
    public void CanAddWindow()
    {
        var map = new Map();
        var room1 = map.AddRoom(new Room(parent: map, size: new Size(400, 400)));
        var room2 = map.AddRoom(new Room(parent: map, size: new Size(400, 400)))
                       .Place().EastOf(room1, 16);

        var window = RoomGenerator.AddStructure(room1, new Window(
            Template: new Room { Floor = 32, Ceiling = -32 },
            Width: 128,
            AdjacentRoom: room2,
            CenterPercent: 0.50));

        Assert.That(window.Bounds.Width, Is.EqualTo(16));
        Assert.That(window.Bounds.X, Is.EqualTo(room1.Bounds.Width));
    }

    [Test]
    public void CanAddPoisonPit()
    {
        var map = new TestMaps().TwoConnectedRoomsWithDifferentCeilings();
        
        var pit = RoomGenerator.AddStructure(map.Rooms[1], new HazardPit(
            Depth: 32,
            Flat: AnimatedFlat.NUKAGE1,
            Damage: DamagingSectorSpecial.Damage_10Percent,
            Padding: new Padding(16)));

        var mapElements = MapBuilder.Build(map);

        var pitSector = mapElements.Sectors.First(s => s.Floor == Flat.NUKAGE1);
        var room2Sector = mapElements.Sectors.First(p => p.Room == map.Rooms[1]);


        Assert.That(pitSector.Floor, Is.EqualTo(Flat.NUKAGE1));
        Assert.That(pitSector.Data.heightfloor, Is.EqualTo(room2Sector.Data.heightfloor - 32));
        Assert.That(pitSector.SectorSpecial, Is.EqualTo(ZDoomSectorSpecial.Damage_10Percent));
    }
}
