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

        if(hall.Stairs != null)
        {
            hallRoom.InnerStructures.AddRange(GenerateStairs(hall.Stairs, hallRoom, side));
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
            WallTexture = door.TrackTexture,
        };

        switch(hallSide)
        {
            case Side.Right:
                doorRoom.UpperLeft = hallSide.ToPoint(door.PositionInHall);
                doorRoom.BottomRight = doorRoom.UpperLeft.Add(new Point(door.Thickness, -hallRoom.Bounds.Height));
                break;
            case Side.Left:
                doorRoom.UpperLeft = hallSide.ToPoint(door.PositionInHall).Add(new Point(hallRoom.Bounds.Width - door.Thickness, 0));
                doorRoom.BottomRight = doorRoom.UpperLeft.Add(new Point(door.Thickness, -hallRoom.Bounds.Height));
                break;
            case Side.Bottom:
                doorRoom.UpperLeft = hallSide.ToPoint(door.PositionInHall);
                doorRoom.BottomRight = doorRoom.UpperLeft.Add(new Point(hallRoom.Bounds.Width, -door.Thickness));
                break;
            case Side.Top:
                doorRoom.UpperLeft = hallSide.ToPoint(door.PositionInHall).Add(new Point(0, -(hallRoom.Bounds.Height - door.Thickness)));
                doorRoom.BottomRight = doorRoom.UpperLeft.Add(new Point(hallRoom.Bounds.Width, -door.Thickness));
                break;

        }

        doorRoom.LineSpecials[hallSide] = new DoorRaise(0, Speed.StandardDoor);
        doorRoom.LineSpecials[hallSide.Opposite()] = new DoorRaise(0, Speed.StandardDoor);

        doorRoom.SideTextures[hallSide] = door.Texture;
        doorRoom.SideTextures[hallSide.Opposite()] = door.Texture;

        return doorRoom;
    }

    private IEnumerable<Room> GenerateStairs(Stairs stairs, Room hallRoom, Side hallSide)
    {
        int lowerFloor = stairs.Rooms.OrderBy(r => r.Floor).First().Floor;
        int upperFloor = stairs.Rooms.OrderByDescending(r => r.Floor).First().Floor;

        //todo, other directions
        int totalWidth = hallRoom.Bounds.Width - stairs.StartPosition - stairs.EndPosition;
        int numSteps = totalWidth / stairs.StepWidth;
        int heightPerStep = (upperFloor - lowerFloor) / numSteps;

        int currentHeight = lowerFloor;
        Point nextStepPosition = hallSide.ToPoint(stairs.StartPosition);

        while(currentHeight < upperFloor)
        {
            currentHeight += heightPerStep;
            yield return CreateStep(stairs, hallRoom, nextStepPosition, currentHeight);            
            nextStepPosition.X += stairs.StepWidth;
        }

        yield return new Room
        {
            Floor = upperFloor,
            UpperLeft = nextStepPosition,
            BottomRight = nextStepPosition.Add(stairs.EndPosition, -hallRoom.Bounds.Height),
            WallTexture = stairs.StepTexture
        };
    }

    private Room CreateStep(Stairs stairs, Room hallRoom, Point position, int stepHeight)
    {
        return new Room
        {
            Floor = stepHeight,
            UpperLeft = position,
            BottomRight = position.Add(stairs.StepWidth, -hallRoom.Bounds.Height),
            WallTexture = stairs.StepTexture
        };
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
