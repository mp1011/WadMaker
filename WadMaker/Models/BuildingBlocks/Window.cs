namespace WadMaker.Models.BuildingBlocks;

public record Window(Room Template, Room AdjacentRoom, int Width, double CenterPercent) : RoomBuildingBlock();

