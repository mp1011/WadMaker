using System.ComponentModel.DataAnnotations.Schema;
using WadMaker.Models.LineSpecials;

namespace WadMaker.Tests.Services;


class HallGeneratorTests : StandardTest
{
    private const int HallWidth = 128;

    [Test]
    public void CanGenerateWestHall()
    {
        var roomsWithHall = GenerateRoomsWithHall(-500, 0);
        var hall = roomsWithHall[2];

        Assert.That(hall.UpperLeft.X, Is.EqualTo(roomsWithHall[1].BottomRight.X));
        Assert.That(hall.BottomRight.X, Is.EqualTo(roomsWithHall[0].UpperLeft.X));
        Assert.That(hall.Bounds.Height, Is.EqualTo(HallWidth));
    }


    [Test]
    public void CanGenerateEastHall()
    {
        var roomsWithHall = GenerateRoomsWithHall(1000, 0);
        var hall = roomsWithHall[2];

        Assert.That(hall.UpperLeft.X, Is.EqualTo(roomsWithHall[0].BottomRight.X));
        Assert.That(hall.BottomRight.X, Is.EqualTo(roomsWithHall[1].UpperLeft.X));
        Assert.That(hall.Bounds.Height, Is.EqualTo(HallWidth));
    }

    [Test]
    public void CanGenerateNorthHall()
    {
        var roomsWithHall = GenerateRoomsWithHall(0, 500);
        var hall = roomsWithHall[2];

        Assert.That(hall.UpperLeft.Y, Is.EqualTo(roomsWithHall[1].BottomRight.Y));
        Assert.That(hall.BottomRight.Y, Is.EqualTo(roomsWithHall[0].UpperLeft.Y));
        Assert.That(hall.Bounds.Width, Is.EqualTo(HallWidth));
    }


    [Test]
    public void CanGenerateSouthHall()
    {
        var roomsWithHall = GenerateRoomsWithHall(0, -1000);
        var hall = roomsWithHall[2];

        Assert.That(hall.UpperLeft.Y, Is.EqualTo(roomsWithHall[0].BottomRight.Y));
        Assert.That(hall.BottomRight.Y, Is.EqualTo(roomsWithHall[1].UpperLeft.Y));
        Assert.That(hall.Bounds.Width, Is.EqualTo(HallWidth));
    }

    private Room[] GenerateRoomsWithHall(int secondRoomOffsetX, int secondRoomOffsetY)
    {
        var map = new Map();

        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });

        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(secondRoomOffsetX, secondRoomOffsetY),
            BottomRight = new Point(secondRoomOffsetX + 256, secondRoomOffsetY - 256)
        });

        map.Rooms.Add(HallGenerator.GenerateHall(new Hall(HallWidth, map.Rooms[0], map.Rooms[1])));
        return map.Rooms.ToArray();
    }

    [Test]
    public void CanGenerateHallWithDoor()
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
        var hall = HallGenerator.GenerateHall(
            new Hall(HallWidth,
            map.Rooms[0], 
            map.Rooms[1],
            Door: new Door(16, Texture.BIGDOOR2, Texture.DOORTRAK, 64)));

        var door = hall.InnerStructures.First();

        Assert.That(door.Ceiling, Is.EqualTo(-128));
        Assert.That(door.Floor, Is.EqualTo(0));
        Assert.That(door.Bounds.Width, Is.EqualTo(16));
    }

    [TestCase(KeyType.Red)]
    [TestCase(KeyType.Yellow)]
    [TestCase(KeyType.Blue)]
    public void CanGenerateHallWithColoredDoor(KeyType color)
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

        var hall = HallGenerator.GenerateHall(
            new Hall(HallWidth,
            map.Rooms[0],
            map.Rooms[1],
            Door: new Door(16, new TextureInfo(Texture.BIGDOOR2), new TextureInfo(Texture.BIGDOOR2), 64, KeyColor: color)));

        map.Rooms.Add(hall);

        var mapElements = MapBuilder.Build(map);

        var doorLines = mapElements.LineDefs.Where(p => p.LineSpecial != null && p.LineSpecial.Type == LineSpecialType.Door_LockedRaise)
            .ToArray();

        Assert.That(doorLines.Length, Is.EqualTo(2));
        foreach(var doorLine in doorLines)
        {
            Assert.That(doorLine.LineSpecial?.arg3, Is.EqualTo((int)color));
        }
    }
    
    [Test]
    public void CanGenerateStairs()
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

        var hall = HallGenerator.GenerateHall(
            new Hall(HallWidth,
            map.Rooms[0],
            map.Rooms[1],
            Stairs: new Stairs(
                StepTexture: new TextureInfo(Main: Texture.STEP1),
                50,
                50,
                StepWidth: 30,
                map.Rooms[0], 
                map.Rooms[1])));


        Assert.That(hall.InnerStructures.Count, Is.EqualTo(11));

        int lastHeight = map.Rooms[0].Floor;
        foreach(var step in hall.InnerStructures.Take(10))
        {
            Assert.That(step.Floor, Is.GreaterThan(lastHeight));
            lastHeight = step.Floor;
        }
    }

    [Test]
    public void CanGenerateStairsWithAscendingCeiling()
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

        var hall = HallGenerator.GenerateHall(
            new Hall(HallWidth,
            map.Rooms[0],
            map.Rooms[1],
            Stairs: new Stairs(
                StepTexture: new TextureInfo(Main: Texture.STEP1),
                50,
                50,
                StepWidth: 30,
                map.Rooms[0],
                map.Rooms[1]))).AddTo(map);


        Assert.That(hall.InnerStructures.Count, Is.EqualTo(11));

        int lastFloor = hall.InnerStructures.First().Floor;
        int lastCeiling = hall.InnerStructures.First().Ceiling;
        foreach (var step in hall.InnerStructures.Skip(1).Take(9))
        {
            Assert.That(step.Floor, Is.GreaterThan(lastFloor));
            Assert.That(step.Ceiling, Is.GreaterThan(lastCeiling));

            lastFloor = step.Floor;
            lastCeiling = step.Ceiling;
        }
    }
    

    [Test]
    public void CanGenerateHallWhenRoomSidesNotOnBounds()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(200, -200)
        });

        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(1000, 0),
            BottomRight = new Point(1200, -200)
        });

        map.Rooms[0].Shape.Modifiers.Add(new NotchedSides { Width = 160, Depth = 20 });
        map.Rooms[1].Shape.Modifiers.Add(new NotchedSides { Width = 160, Depth = 20 });

        var hall = new Hall(100, map.Rooms[0], map.Rooms[1]);

        var generatedHall = HallGenerator.GenerateHall(hall);

        Assert.That(generatedHall.UpperLeft.X, Is.EqualTo(180));
        Assert.That(generatedHall.BottomRight.X, Is.EqualTo(1020));
    }

    [Test]
    public void CanGenerateHallForDifferentSizedRooms()
    {
        var map = new Map();
        var room1 = map.AddRoom(new Room(map)
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -128)
        });

        var room2 = map.AddRoom(new Room(map) {  
            UpperLeft = new Point(256, 0), 
            BottomRight = new Point(512, -1028) });

        room1.Floor = 128;
        room1.Ceiling = 256;

        room2.Floor = 0;
        room2.Ceiling = 512;

        var hall = map.AddRoom(HallGenerator.GenerateHall(new Hall(128, room1, room2)));

        Assert.That(hall.Bounds.Height, Is.EqualTo(room1.Bounds.Height));
        Assert.That(hall.UpperLeft.Y, Is.EqualTo(room1.UpperLeft.Y));
    }

    [Test]
    public void WhenDoorIsActivatedBySwitchItDoesNotHaveItsOwnLineSpecials()
    {
        var map = new Map();
        var room1 = map.AddRoom(new Room {  UpperLeft = Point.Empty, BottomRight = new Point(256,-256) });
        var room2 = map.AddRoom(new Room(map, size: new Size(128, 128)).Place().EastOf(room1, 32));

        var switchAlcove = StructureGenerator.AddStructure(room1, new Alcove(new Room { Floor = 16, Ceiling = -16 }, Side.Left, 64, 8, 0.5));
        int doorTag = IDProvider.NextSectorIndex();
        switchAlcove.LineSpecials[Side.Left] = new DoorOpen(doorTag, Speed.StandardDoor);

        var hall = map.AddRoom(HallGenerator.GenerateHall(new Hall(128, room1, room2,
            Door: new Door(8, Texture.BIGDOOR1, Texture.DOORTRAK, 16, Tag: doorTag))));

        var door = hall.InnerStructures.First();

        Assert.That(door.LineSpecials.Try(Side.Left), Is.Null);
        Assert.That(door.LineSpecials.Try(Side.Right), Is.Null);
    }
}
