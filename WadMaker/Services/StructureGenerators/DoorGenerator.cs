namespace WadMaker.Services.StructureGenerators;

internal class DoorGenerator : ISideStructureGenerator<Door>
{
    public Room AddStructure(Room hallRoom, Door door, Side hallSide)
    {
        var doorRoom = new Room
        {
            Ceiling = -hallRoom.VerticalHeight,
            Floor = 0,
            CeilingTexture = hallRoom.CeilingTexture,
            FloorTexture = hallRoom.FloorTexture,
            WallTexture = door.TrackTexture,
        };
        door.SetOn(doorRoom);

        var doorPoints = hallRoom.Bounds.GetSegment(hallSide, door.PositionInHall, door.Thickness);
        doorRoom.UpperLeft = doorPoints.Item1;
        doorRoom.BottomRight = doorPoints.Item2;

        if (door.Tag == null)
        {
            doorRoom.LineSpecials[hallSide] = door.DoorSpecial();
            doorRoom.LineSpecials[hallSide.Opposite()] = door.DoorSpecial();
        }
        else
        {
            doorRoom.Tag = door.Tag;
        }

        doorRoom.SideTextures[hallSide] = door.Texture;
        doorRoom.SideTextures[hallSide.Opposite()] = door.Texture;

        return doorRoom;
    }
}
