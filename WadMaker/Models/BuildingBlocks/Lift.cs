namespace WadMaker.Models.BuildingBlocks;

public record Lift(TextureInfo SideTexture, int PositionInHall, int Width, Room StartRoom, Room EndRoom)
{
}