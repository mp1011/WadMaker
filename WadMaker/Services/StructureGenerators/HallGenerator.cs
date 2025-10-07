namespace WadMaker.Services.StructureGenerators;

public class HallGenerator : IStructureGenerator<Hall>
{
    private readonly StructureGenerator _structureGenerator;

    public HallGenerator(StructureGenerator structureGenerator)
    {
        _structureGenerator = structureGenerator;
    }

    public Room AddStructure(Room room, Hall hall)
    {
        var side = HallSide(hall);

        var room1Anchors = GetHallAnchors(hall.Room1, side, hall.Width, hall.Room1.Bounds.SideCenter(side));
        var room2Anchors = GetHallAnchors(hall.Room2, side.Opposite(), hall.Width, hall.Room2.Bounds.SideCenter(side.Opposite()));

        if (!room1Anchors.Any() || !room2Anchors.Any())
            throw new Exception("Unable to fit hall");

        // first, try using anchors as-s
        var hallRoom = CreateHallRoom(hall, room1Anchors.Union(room2Anchors).ToArray());
        hall.SetOn(hallRoom);

        // if that didn't work, try getting room2 anchors aligned to room1's
        if (hallRoom.Bounds.AxisLength(side.ClockwiseTurn()) != hall.Width)
        {
            var newRoom2Anchors = GetHallAnchors(hall.Room2, side.Opposite(), hall.Width,
                hall.Room2.Bounds.SideCenter(side.Opposite()).AlignWith(hall.Room1.Bounds.SideCenter(side), side));

            hallRoom.SetFromVertices(room1Anchors.Union(newRoom2Anchors));
        }

        // finally, try getting room1 anchors aligned to room 2's
        if (hallRoom.Bounds.AxisLength(side.ClockwiseTurn()) != hall.Width)
        {
            var newRoom1Anchors = GetHallAnchors(hall.Room1, side, hall.Width,
                hall.Room1.Bounds.SideCenter(side).AlignWith(hall.Room2.Bounds.SideCenter(side.Opposite()), side));

            hallRoom.SetFromVertices(newRoom1Anchors.Union(room2Anchors));
        }

        if (hallRoom.Bounds.AxisLength(side.ClockwiseTurn()) != hall.Width)
        {
            throw new Exception("Unable to place hall");
        }

        if (hall.Door != null)
        {
            var door = hallRoom.AddInnerStructure(_structureGenerator.AddStructure(hallRoom, hall.Door, side));
            if (hall.Door.KeyColor != KeyType.None && hall.Door.ColorBar != null)
            {
                _structureGenerator.GenerateDoorColorBars(hallRoom, hall.Door, side);
            }
        }

        if(hall.Stairs != null)
        {
            hallRoom.AddInnerStructures(_structureGenerator.AddMultiRoomStructure(hallRoom, hall.Stairs, side));
        }

        if (hall.Lift != null)
        {
            hallRoom.AddInnerStructures(_structureGenerator.AddMultiRoomStructure(hallRoom, hall.Lift, side));
        }


        return hallRoom;
    }

    private Point[] GetHallAnchors(Room room, Side side, int hallWidth, Point hallCenter)
    {
        var anchors = new Point[]
        {
            hallCenter.Move(side.ClockwiseTurn(), hallWidth / 2),
            hallCenter.Move(side.CounterClockwiseTurn(), hallWidth / 2)
        };

        var roomLines = room.Shape.CalculatePoints().WithNeighbors().ToArray();
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

        return Array.Empty<Point>();
    }

    private Room CreateHallRoom(Hall hall, Point[] vertices)
    {
        if(hall.HallTemplate != null)
        {
            return new Room(hall.Room1.Parent, vertices)
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
            return new Room(hall.Room1.Parent, vertices)
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
    private Side HallSide(Hall hall)
    {
        return hall.Room2.Bounds().SideRelativeTo(hall.Room1.Bounds());
    }
}
