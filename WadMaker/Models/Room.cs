namespace WadMaker.Models;

public class Room
{
    public Point UpperLeft { get; set; } = Point.Empty;
    public Point BottomRight { get; set; } = Point.Empty;

    public Point Center => new Point(UpperLeft.X + (BottomRight.X - UpperLeft.X) / 2, 
                                     UpperLeft.Y + (BottomRight.Y - UpperLeft.Y) / 2);

    public int Height { get; set; } = 128;
    public int Floor { get; set; } = 0;
    public Flat FloorTexture { get; set; } = Flat.Default;
    public Flat CeilingTexture { get; set; } = Flat.Default;
    public Texture WallTexture { get; set; } = Texture.Default;

    public List<Hall> Halls { get; set; } = new List<Hall>();
}
