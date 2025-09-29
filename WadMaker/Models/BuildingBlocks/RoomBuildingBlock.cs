namespace WadMaker.Models.BuildingBlocks;

public abstract record RoomBuildingBlock()
{
    public Room SetOn(Room room)
    {
        room.BuildingBlock = this;
        return room;
    }
}

