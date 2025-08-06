using WadMaker.Models.LineSpecials;

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
            WallTexture = new TextureInfo(Main: Texture.STONE),
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -256)
        });
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);

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
            UpperLeft = new Point(256, 0),
            BottomRight = new Point(356, -256)
        });
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
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
            UpperLeft = new Point(256, 0),
            BottomRight = new Point(356, -256)
        });

        var hall = new Hall(Room1: map.Rooms[0], Room2: map.Rooms[1], Width: 192);
        map.Rooms.Add(new HallGenerator().GenerateHall(hall));
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
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
            WallTexture = new TextureInfo(Main: Texture.STONE),
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
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
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
            WallTexture = new TextureInfo(Main: Texture.STONE),
            FloorTexture = Flat.FLOOR0_1,
            CeilingTexture = Flat.FLOOR0_3,
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });

        map.Rooms[0].InnerStructures.Add(new Room
        {
            UpperLeft = new Point(148, -128),
            BottomRight = new Point(148 + 64, -128 - 64),
            WallTexture = new TextureInfo(Main: Texture.STONE2),
            Floor = -16, 
            Ceiling = 32,
        });

        map.Rooms[0].InnerStructures[0].ShapeModifiers.Add(new InvertCorners { Width = 8 });
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//room_with_inner_structure.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    [Test]
    public void CanCreateRoomsConnectedByStairs()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(100, -256)
        });
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(500, 0),
            BottomRight = new Point(600, -256),
            Floor = 100,
            Ceiling = 300
        });

        var hallGenerator = new HallGenerator();
        var hall = hallGenerator.GenerateHall(
            new Hall(64,
            map.Rooms[0],
            map.Rooms[1],
            Stairs: new Stairs(
                StepTexture: new TextureInfo(Lower: Texture.STEP1),
                50,
                50,
                StepWidth: 30,
                map.Rooms[0], 
                map.Rooms[1])));

        map.Rooms.Add(hall);
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//hall_with_stairs.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    [Test]
    public void CanCreateRoomsConnectedByLift()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(100, -256),
            Ceiling = 500
        });
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(500, 0),
            BottomRight = new Point(600, -256),
            Floor = 128,
            Ceiling = 500
        });

        var hallGenerator = new HallGenerator();
        var hall = hallGenerator.GenerateHall(
            new Hall(64,
            map.Rooms[0],
            map.Rooms[1],
            Lift: new Lift(
                SideTexture: new TextureInfo(Lower: Texture.PLAT1),
                32,
                64,
                map.Rooms[0],
                map.Rooms[1])));

        map.Rooms.Add(hall);
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//hall_with_lift.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }


    [Test]
    public void CanCreateRoomWithButtonActivatedLift()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400),
        });

        //lift
        var liftRoom = new Room
        {
            Floor = 128,
            Ceiling = 0,
            UpperLeft = new Point(300, -200),
            BottomRight = new Point(364, -264),
            WallTexture = new TextureInfo(Texture.PLAT1),
            Tag = 1,
        };
        map.Rooms[0].InnerStructures.Add(liftRoom);

        //switch
        var switchRoom = new Room
        {
            Floor = 16,
            Ceiling = -16,
        };
        switchRoom.SideTextures[Side.Left] = new TextureInfo(Texture.SW1GRAY);
        switchRoom.LineSpecials[Side.Left] = new Plat_DownWaitUpStay(Tag: 1, Speed.StandardLift, Delay.StandardLift);

        RoomGenerator.AddStructure(map.Rooms[0], new Alcove(
            Template: switchRoom,
            Side: Side.Left,
            Width: 64,
            Depth: 8,
            CenterPercent: 0.25));
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
        var udmf = MapPainter.Paint(MapBuilder.Build(map));
        var expected = File.ReadAllText("Fixtures//room_with_button_activated_lift.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }


    [Test]
    public void CanGenerateTextureTestMap()
    {
        var map = new TestMaps().TextureTestMap();
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
        var udmf = MapToUDMF(map);

        var expected = File.ReadAllText("Fixtures//texture_test_map.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }
}

