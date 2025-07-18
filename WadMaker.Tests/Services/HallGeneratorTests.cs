namespace WadMaker.Tests.Services;


public class HallGeneratorTests
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
    public void CanGeneratNorthHall()
    {
        var roomsWithHall = GenerateRoomsWithHall(0, 500);
        var hall = roomsWithHall[2];

        Assert.That(hall.UpperLeft.Y, Is.EqualTo(roomsWithHall[1].BottomRight.Y));
        Assert.That(hall.BottomRight.Y, Is.EqualTo(roomsWithHall[0].UpperLeft.Y));
        Assert.That(hall.Bounds.Width, Is.EqualTo(HallWidth));
    }


    [Test]
    public void CanGeneratSouthHall()
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

        var hallGenerator = new HallGenerator();
        map.Rooms.Add(hallGenerator.GenerateHall(new Hall(HallWidth, map.Rooms[0], map.Rooms[1])));
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
        var hallGenerator = new HallGenerator();
        var hall = hallGenerator.GenerateHall(
            new Hall(HallWidth,
            map.Rooms[0], 
            map.Rooms[1],
            Door: new Door(16, Texture.BIGDOOR2, 64)));

        var door = hall.InnerStructures.First();

        Assert.That(door.Ceiling, Is.EqualTo(-128));
        Assert.That(door.Floor, Is.EqualTo(0));
        Assert.That(door.Bounds.Width, Is.EqualTo(16));
        // still todo, doors in other directions, door open action
    }

}
