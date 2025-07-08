namespace WadMaker.Services;

public class HallGenerator
{
    public Room GenerateHall(Hall hall)
    {
        return new Room()
        {
            UpperLeft = new Point(hall.Room1.BottomRight.X, hall.Room1.UpperLeft.Y - 32),
            BottomRight = new Point(hall.Room2.UpperLeft.X, hall.Room2.BottomRight.Y + 32),
            Height = hall.Room1.Height,
            Floor = hall.Room1.Floor,
            FloorTexture = hall.Room1.FloorTexture,
            CeilingTexture = hall.Room1.CeilingTexture,
            WallTexture = hall.Room1.WallTexture
        };

    }
}
