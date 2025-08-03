namespace WadMaker.Models;

public class Map : IThemed
{
    public List<Room> Rooms { get; set; } = new List<Room>();

    public Theme? Theme { get; set; }
}

