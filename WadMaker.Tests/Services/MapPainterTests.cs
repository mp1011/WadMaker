namespace WadMaker.Tests.Services;

internal class MapPainterTests : StandardTest
{
    [Test]
    public void MapPainterCanCreateBasicRoom()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });

        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//basicmap.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    [Test]
    public void MapPainterCanCreateTwoUnconnectedRooms()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });

        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(256, 0),
            BottomRight = new Point(356, -256)
        });

        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//two_unconnected_rooms.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    [Test]
    public void MapPainterCanCreateTwoRoomsConnectedByHall()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });

        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(256, 0),
            BottomRight = new Point(356, -256)
        });

        var hall = new Hall(Room1: map.Rooms[0], Room2: map.Rooms[1], Width: 192);
        map.Rooms[0].Halls.Add(hall);
        map.Rooms[1].Halls.Add(hall);

        map.Rooms.Add(new HallGenerator().GenerateHall(hall));

        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//two_rooms_with_hall.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    [Test]
    public void CanCreateRoomWithPillar()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });

        map.Rooms[0].Pillars.Add(new Cutout{
            UpperLeft = new Point(148,-128),
            BottomRight = new Point(148+64, -128-64),
            WallTexture = Texture.STONE2});

        map.Rooms[0].Pillars[0].ShapeModifiers.Add(new InvertCorners { Width = 8 });

        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//room_with_pillar.udmf");
        Assert.That(udmf, Is.EqualTo(expected));

    }

    [Test]
    public void CanCreateRoomWithInnerStructure()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Ceiling = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });

        map.Rooms[0].InnerStructures.Add(new Room
        {
            UpperLeft = new Point(148, -128),
            BottomRight = new Point(148 + 64, -128 - 64),
            WallTexture = Texture.STONE2,
            Floor = -16, 
            Ceiling = 32,
        });

        map.Rooms[0].InnerStructures[0].ShapeModifiers.Add(new InvertCorners { Width = 8 });

        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//room_with_inner_structure.udmf");
        Assert.That(udmf, Is.EqualTo(expected));

    }
}

