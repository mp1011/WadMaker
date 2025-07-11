﻿namespace WadMaker.Services;

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

        return new Room(room1Anchors.Union(room2Anchors))
        {
            Height = hall.Room1.Height,
            Floor = hall.Room1.Floor,
            FloorTexture = hall.Room1.FloorTexture,
            CeilingTexture = hall.Room1.CeilingTexture,
            WallTexture = hall.Room1.WallTexture
        };
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
