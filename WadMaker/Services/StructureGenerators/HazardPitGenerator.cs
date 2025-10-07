namespace WadMaker.Services.StructureGenerators;

internal class HazardPitGenerator : IStructureGenerator<HazardPit>
{
    public Room AddStructure(Room room, HazardPit hazardPit)
    {
        var pit = new Room
        {
            SectorSpecial = hazardPit.Damage.To<ZDoomSectorSpecial>(),
            Floor = -hazardPit.Depth,
            FloorTexture = hazardPit.Flat.To<Flat>(),
            Ceiling = 0
        };

        pit.UpperLeft = new Point(hazardPit.Padding.Left, -hazardPit.Padding.Top);
        pit.BottomRight = new Point(room.Bounds.Width - hazardPit.Padding.Right, -(room.Bounds.Height - hazardPit.Padding.Bottom));

        room.AddInnerStructure(pit);
        return hazardPit.SetOn(pit);
    }
}
