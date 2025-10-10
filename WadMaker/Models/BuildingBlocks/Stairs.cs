namespace WadMaker.Models.BuildingBlocks;

public record Stairs(TextureInfo StepTexture, int StartPosition, int EndPosition, int StepWidth, Room StartRoom, Room EndRoom, 
    int? FixedCeiling = null, Room? StepTemplate = null)
    : MultiRoomBuildingBlock()
{
}
