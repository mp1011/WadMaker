namespace WadMaker.Models;

public class Map : IThemed
{
    public List<Room> Rooms { get; set; } = new List<Room>();

    public Theme? Theme { get; set; }

    public Room AddRoom(Room room)
    {
        room = room ?? new Room(this);
        Rooms.Add(room);
        return room;
    }

    public Room AddRoom(Point? center = null, Size? size = null) => AddRoom(new Room(this, center, size));
}

