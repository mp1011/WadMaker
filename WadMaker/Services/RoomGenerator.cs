namespace WadMaker.Services;

public class RoomGenerator
{
    public Room AddStructure(Room room, Alcove alcove)
    {
        var alcoveRoom = alcove.Template.Copy();

        var centerPoint = GetRelativeSidePoint(room.Bounds, alcove.Side, (int)(room.Bounds.AxisLength(alcove.Side) * alcove.CenterPercent));

        var alcovePoints = GetAlcoveSegment(
            side: alcove.Side,
            room: room,
            centerPoint: centerPoint,
            width: alcove.Width,
            depth: alcove.Depth);

        alcoveRoom.UpperLeft = alcovePoints.Item1;
        alcoveRoom.BottomRight = alcovePoints.Item2;

        room.InnerStructures.Add(alcoveRoom);
        return alcoveRoom;
    }

    private Point GetRelativeSidePoint(DRectangle bounds, Side side, int position)
    {
        return side switch
        {
            Side.Left => new Point(0, -position),
            Side.Right => new Point(bounds.Width, -position),
            Side.Bottom => new Point(position, -bounds.Height),
            Side.Top => new Point(position, 0),
            _ => new Point(0, 0),
        };        
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
