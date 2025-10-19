namespace WadMaker.Models.BuildingBlocks;

public record Alcove(Room Template, Side Side, int Width, int Depth, double CenterPercent) : RoomBuildingBlock()
{
    public Alcove(int Floor, int Ceiling, Side Side, int Width, int Depth, double CenterPercent) :
        this(new Room { Floor = Floor, Ceiling = Ceiling }, Side, Width, Depth, CenterPercent)
    {
    }
}
