namespace WadMaker.Models.BuildingBlocks;

public abstract record DoorColorBar() : RoomBuildingBlock();

public record DoorColorBarRecessedAlcoves(int Distance = 8, int Floor = 16, int Ceiling = -16, int Width = 16, int Depth = 8) : DoorColorBar 
{
    public int HalfWidth => Width / 2;
}

public record DoorColorBarFlat(int Distance = 8, int Width = 16) 
    : DoorColorBarRecessedAlcoves(Distance: Distance, Width: Width, Ceiling:0, Floor:0, Depth: 0)
{
}
