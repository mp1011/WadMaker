namespace WadMaker.Models.BuildingBlocks;

public record Lift(TextureInfo SideTexture, Room DestinationRoom, Size Size, bool AddWalkTrigger = false) 
    : MultiRoomBuildingBlock()
{
}
public record HallwayLift(TextureInfo SideTexture, int PositionInHall, int Width, Room StartRoom, Room EndRoom, bool AddWalkTrigger = false) 
    : MultiRoomBuildingBlock()
{
}
