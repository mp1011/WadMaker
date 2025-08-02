using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using WadMaker.Models;

namespace WadMaker.Services;

public class HallGenerator
{
    public Room GenerateHall(Hall hall)
    {
        var side = HallSide(hall);

        var room1Anchors = GetHallAnchors(hall.Room1, side, hall.Width);
        var room2Anchors = GetHallAnchors(hall.Room2, side.Opposite(), hall.Width);

        var hallRoom = CreateHallRoom(hall, room1Anchors.Union(room2Anchors).ToArray());
        if(hall.Door != null)
        {
            hallRoom.InnerStructures.Add(GenerateDoor(hall.Door, hallRoom, side));
        }

        if(hall.Stairs != null)
        {
            hallRoom.InnerStructures.AddRange(GenerateStairs(hall.Stairs, hallRoom, side));
        }

        if (hall.Lift != null)
        {
            hallRoom.InnerStructures.AddRange(GenerateLift(hall.Lift, hallRoom, side));
        }


        return hallRoom;
    }

    private Point[] GetHallAnchors(Room room, Side side, int hallWidth)
    {
        var anchors = room.Bounds.SidePoints(side)
                                 .MoveToDistance(hallWidth)
                                 .ToArray();

        var roomLines = room.GetPoints().WithNeighbors().ToArray();
        var intersectingLine = roomLines.FirstOrDefault(line => anchors.All(a => a.Intersects(line.Item2, line.Item3)));

        // if anchors are on a wall, we don't need to do anything
        if (intersectingLine != default)
            return anchors;

        int tries = 10000;
        while(--tries > 0)
        {
            // probably is a more efficient way to do this...
            anchors = anchors.Select(p => p.Move(side.Opposite(), 1)).ToArray();
            intersectingLine = roomLines.FirstOrDefault(line => anchors.All(a => a.Intersects(line.Item2, line.Item3)));

            if (intersectingLine != default)
                return anchors;
        }

        throw new Exception("Unable to fit hall");
    }

    private (Point,Point) GetHallSegment(Side side, DRectangle hallBounds, int position, int width)
    {
        Point upperLeft, bottomRight;
        switch (side)
        {
            case Side.Right:
                upperLeft = side.ToPoint(position);
                bottomRight = upperLeft.Add(new Point(width, -hallBounds.Height));
                break;
            case Side.Left:
                upperLeft = side.ToPoint(position).Add(new Point(hallBounds.Width - width, 0));
                bottomRight = upperLeft.Add(new Point(width, -hallBounds.Height));
                break;
            case Side.Bottom:
                upperLeft = side.ToPoint(position);
                bottomRight = upperLeft.Add(new Point(hallBounds.Width, -width));
                break;
            case Side.Top:
                upperLeft = side.ToPoint(position).Add(new Point(0, -(hallBounds.Height - width)));
                bottomRight = upperLeft.Add(new Point(hallBounds.Width, -width));
                break;
            default:
                throw new Exception("Invalid Side");

        }

        return (upperLeft, bottomRight);
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

        var doorPoints = GetHallSegment(hallSide, hallRoom.Bounds, door.PositionInHall, door.Thickness);
        doorRoom.UpperLeft = doorPoints.Item1;
        doorRoom.BottomRight = doorPoints.Item2;

        doorRoom.LineSpecials[hallSide] = new DoorRaise(0, Speed.StandardDoor);
        doorRoom.LineSpecials[hallSide.Opposite()] = new DoorRaise(0, Speed.StandardDoor);

        doorRoom.SideTextures[hallSide] = door.Texture;
        doorRoom.SideTextures[hallSide.Opposite()] = door.Texture;

        return doorRoom;
    }

    private IEnumerable<Room> GenerateStairs(Stairs stairs, Room hallRoom, Side hallSide)
    {        
        int totalWidth = hallRoom.Bounds.AxisLength(hallSide) - stairs.StartPosition - stairs.EndPosition;
        int numSteps = totalWidth / stairs.StepWidth;
        int heightPerStep = (stairs.EndRoom.Floor - stairs.StartRoom.Floor) / numSteps;
        int currentHeight = stairs.StartRoom.Floor;

        var nextStepPoints = GetHallSegment(hallSide, hallRoom.Bounds, stairs.StartPosition, stairs.StepWidth);
        int stepNum = 0;
        while(stepNum++ < numSteps)
        {
            currentHeight += heightPerStep;
            yield return CreateStep(stairs, hallRoom, nextStepPoints.Item1, nextStepPoints.Item2, currentHeight);

            nextStepPoints = (nextStepPoints.Item1.Move(hallSide, stairs.StepWidth),
                              nextStepPoints.Item2.Move(hallSide, stairs.StepWidth));
        }

        var remainingWidth = hallRoom.Bounds.AxisLength(hallSide) - stairs.StartPosition - (numSteps * stairs.StepWidth);

        if (remainingWidth > 0)
        {
            var endSegment = GetHallSegment(hallSide.Opposite(), hallRoom.Bounds, 0, remainingWidth);
            yield return CreateStep(stairs, hallRoom, endSegment.Item1, endSegment.Item2, stairs.EndRoom.Floor);
        }
    }

    private IEnumerable<Room> GenerateLift(Lift lift, Room hallRoom, Side hallSide)
    {
        var liftPoints = GetHallSegment(hallSide, hallRoom.Bounds, lift.PositionInHall, lift.Width);

        var startFloor = lift.StartRoom.Floor;
        var endFloor = lift.EndRoom.Floor;

        int lowerFloor = Math.Min(startFloor, endFloor);
        int upperFloor = Math.Max(startFloor, endFloor);

        var liftRoom = new Room
        {
            UpperLeft = liftPoints.Item1,
            BottomRight = liftPoints.Item2,
            WallTexture = lift.SideTexture,
            Floor = upperFloor - hallRoom.Floor
        };
        liftRoom.LineSpecials[hallSide.Opposite()] = new Plat_DownWaitUpStay(0, Speed.StandardLift);
        liftRoom.LineSpecials[hallSide] = new Plat_DownWaitUpStay(0, Speed.StandardLift);

        yield return liftRoom;

        if(hallRoom.Floor == startFloor)
        {
            int upperSectionWidth = hallRoom.Bounds.AxisLength(hallSide) - lift.PositionInHall - lift.Width;
            var upperSectionPoints = GetHallSegment(hallSide, hallRoom.Bounds, lift.PositionInHall + lift.Width, upperSectionWidth);

            var upperSection = new Room
            {
                UpperLeft = upperSectionPoints.Item1,
                BottomRight = upperSectionPoints.Item2,
                Floor = endFloor - hallRoom.Floor,               
                WallTexture = hallRoom.WallTexture
            };
            yield return upperSection;
        }
        else
        {
            throw new NotImplementedException("implement me");
        }


    }

    private Room CreateStep(Stairs stairs, Room hallRoom, Point stepUpperLeft, Point stepBottomRight, int stepHeight)
    {
        return new Room
        {
            Floor = stepHeight - hallRoom.Floor,
            UpperLeft = stepUpperLeft,
            BottomRight = stepBottomRight,
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
