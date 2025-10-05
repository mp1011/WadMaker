namespace WadMaker.Services;

public class StructureGenerator
{
    public Room AddStructure(Room room, Alcove alcove)
    {
        var alcoveRoom = alcove.Template.Copy(room);

        var centerPoint = room.Bounds.GetRelativeSidePoint(alcove.Side, (int)(room.Bounds.SideLength(alcove.Side) * alcove.CenterPercent));

        var alcovePoints = GetAlcoveSegment(
            side: alcove.Side,
            room: room,
            centerPoint: centerPoint,
            width: alcove.Width,
            depth: alcove.Depth);

        alcoveRoom.UpperLeft = alcovePoints.Item1;
        alcoveRoom.BottomRight = alcovePoints.Item2;

        room.InnerStructures.Add(alcoveRoom);
        return alcove.SetOn(alcoveRoom);
    }

    public Room AddStructure(Room room, Window window)
    {
        var side = window.AdjacentRoom.Bounds.SideRelativeTo(room.Bounds);

        int spaceBetween = Math.Abs(room.Bounds.SidePosition(side) - window.AdjacentRoom.Bounds.SidePosition(side.Opposite()));

        return window.SetOn(AddStructure(room, new Alcove(
            window.Template,
            Side: side,
            Width: window.Width,
            Depth: spaceBetween,
            CenterPercent: window.CenterPercent)));
    }

    public Room AddStructure(Room room, HazardPit hazardPit)
    {
        var pit = new Room
        {
            SectorSpecial = hazardPit.Damage.To<ZDoomSectorSpecial>(),
            Floor = -hazardPit.Depth,
            FloorTexture = hazardPit.Flat.To<Flat>(),
            Ceiling = 0
        };

        pit.UpperLeft = new Point(hazardPit.Padding.Left, -hazardPit.Padding.Top);
        pit.BottomRight = new Point(room.Bounds.Width - hazardPit.Padding.Right, -(room.Bounds.Height - hazardPit.Padding.Bottom));

        room.InnerStructures.Add(pit);
        return hazardPit.SetOn(pit);
    }

    private (Point, Point) GetAlcoveSegment(Side side, Room room, Point centerPoint, int width, int depth)
    {
        var pt1 = centerPoint.Move(side.ClockwiseTurn(), width / 2);
        var pt2 = centerPoint.Move(side.CounterClockwiseTurn(), width / 2);
        var pt3 = pt1.Move(side, depth);
        var pt4 = pt2.Move(side, depth);

        var x = new[] { pt1.X, pt2.X, pt3.X, pt4.X };
        var y = new[] { pt1.Y, pt2.Y, pt3.Y, pt4.Y };

        return (new Point(x.Min(), y.Max()), new Point(x.Max(), y.Min()));
    }
}
