namespace WadMaker.Models.BuildingBlocks;

public abstract record DoorColorBar() : RoomBuildingBlock();

public record DoorColorBarRecessedAlcoves(int Distance = 8, int Floor = 16, int Ceiling = -16, int Width = 16, int Depth = 8) : DoorColorBar 
{
    public int HalfWidth => Width / 2;
}
