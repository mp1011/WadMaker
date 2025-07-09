namespace WadMaker.Tests.Services;

internal class MapPainterTests
{
    [Test]
    public void MapPainterCanCreateBasicRoom()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            Floor = 0,
            Height = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });

        var mapPainter = new MapPainter(new RoomBuilder(new IDProvider()), new OverlappingLinedefResolver());
        var udmf = mapPainter.Paint(map);

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
            Height = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });

        map.Rooms.Add(new Room
        {
            Floor = 0,
            Height = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(256, 0),
            BottomRight = new Point(356, -256)
        });

        var mapPainter = new MapPainter(new RoomBuilder(new IDProvider()), new OverlappingLinedefResolver());
        var udmf = mapPainter.Paint(map);

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
            Height = 256,
            WallTexture = Texture.STONE,
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });

        map.Rooms.Add(new Room
        {
            Floor = 0,
            Height = 256,
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

        var mapPainter = new MapPainter(new RoomBuilder(new IDProvider()), new OverlappingLinedefResolver());
        var udmf = mapPainter.Paint(map);

        var expected = File.ReadAllText("Fixtures//two_rooms_with_hall.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }
}

