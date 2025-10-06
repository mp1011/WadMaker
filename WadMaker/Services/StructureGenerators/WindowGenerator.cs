namespace WadMaker.Services.StructureGenerators;

internal class WindowGenerator : IStructureGenerator<Window>
{
    private readonly StructureGenerator _structureGenerator;
    public WindowGenerator(StructureGenerator structureGenerator)
    {
        _structureGenerator = structureGenerator;
    }
    public Room AddStructure(Room room, Window window)
    {
        var side = window.AdjacentRoom.Bounds.SideRelativeTo(room.Bounds);

        int spaceBetween = Math.Abs(room.Bounds.SidePosition(side) - window.AdjacentRoom.Bounds.SidePosition(side.Opposite()));

        return window.SetOn(_structureGenerator.AddStructure(room, structure: new Alcove(
            window.Template,
            Side: side,
            Width: window.Width,
            Depth: spaceBetween,
            CenterPercent: window.CenterPercent)));
    }
}
