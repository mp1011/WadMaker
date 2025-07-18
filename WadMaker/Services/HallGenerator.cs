namespace WadMaker.Services;

public class HallGenerator
{
    public Room GenerateHall(Hall hall)
    {
        var side = HallSide(hall);

        var room1Anchors = hall.Room1.Bounds.SidePoints(side)
                                            .MoveToDistance(hall.Width)
                                            .ToArray();

        var room2Anchors = hall.Room2.Bounds.SidePoints(side.Opposite())
                                            .MoveToDistance()
                                            .ToArray();
        var hallRoom = CreateHallRoom(hall, room1Anchors.Union(room2Anchors).ToArray());
        if(hall.Door != null)
        {
            hallRoom.InnerStructures.Add(GenerateDoor(hall.Door, hallRoom, side));
        }

        return hallRoom;
    }

    private Room GenerateDoor(Door door, Room hallRoom, Side hallSide)
    {
        var doorRoom = new Room
        {
            Ceiling = -hallRoom.Height,
            Floor = 0,
            CeilingTexture = hallRoom.CeilingTexture,
            FloorTexture = hallRoom.FloorTexture,
            WallTexture = door.Texture
        };

        doorRoom.UpperLeft = hallSide.ToPoint(door.PositionInHall);
        doorRoom.BottomRight = doorRoom.UpperLeft.Add(new Point(door.Thickness, -hallRoom.Bounds.Height));

        return doorRoom;
    }

    private Room CreateHallRoom(Hall hall, Point[] vertices)
    {
        if(hall.HallTemplate != null)
        {
            return new Room(vertices)
            {
                Ceiling = hall.HallTemplate.Ceiling,
                Floor = hall.HallTemplate.Floor,
                FloorTexture = hall.HallTemplate.FloorTexture,
                CeilingTexture = hall.HallTemplate.CeilingTexture,
                WallTexture = hall.HallTemplate.WallTexture
            };
        }
        else
        {
            return new Room(vertices)
            {
                Ceiling = hall.Room1.Ceiling,
                Floor = hall.Room1.Floor,
                FloorTexture = hall.Room1.FloorTexture,
                CeilingTexture = hall.Room1.CeilingTexture,
                WallTexture = hall.Room1.WallTexture
            };
        }
    }
    
    /// <summary>
    /// Determine which side of Room1 to build the hall
    /// </summary>
    /// <param name="hall"></param>
    /// <returns></returns>
    private Side HallSide(Hall hall) =>
        Enum.GetValues<Side>().FirstOrDefault(s=> CanMakeHallOnSide(hall, s));

    private static bool CanMakeHallOnSide(Hall hall, Side side)
    {
        if (side == Side.None)
            return false;

        var bounds = hall.Room1.Bounds.ExtendOnSide(side, 5000);
        return bounds.IntersectsWith(hall.Room2.Bounds);
    }
}
