namespace WadMaker.Services;

public class DoorColorBarGenerator
{
    private readonly StructureGenerator _structureGenerator;

    public DoorColorBarGenerator(StructureGenerator roomGenerator)
    {
        _structureGenerator = roomGenerator;
    }

    public void GenerateDoorColorBars(Room hall, Door door, Side hallSide)
    {
        if (door.ColorBar == null)
            return;

        if (door.ColorBar is DoorColorBarRecessedAlcoves c)
            GenerateRecessedAlcoves(hall, door, hallSide, c);         
    }

    private void GenerateRecessedAlcoves(Room hall, Door door, Side hallSide, DoorColorBarRecessedAlcoves colorBar)
    {
        var colorBarTemplate = new Room { Floor = colorBar.Floor, Ceiling = colorBar.Ceiling };

        var colorName = door.KeyColor.ToString().Replace("Skull", "");
        var themeNames = new List<string>();
        themeNames.Add("ColorStrip");
        if (door.KeyColor.ToString().Contains("Skull"))
            themeNames.Add("Skull");

        colorBarTemplate.WallTexture = new TextureInfo(new TextureQuery(ColorName: colorName, ThemeNames: themeNames.ToArray()));

        var doorStart = door.PositionInHall;
        var doorEnd = door.PositionInHall + door.Thickness;
        int hallWidth = hall.Bounds.AxisLength(hallSide);

        List<Room> alcoves = new List<Room>();
        alcoves.Add(_structureGenerator.AddStructure(hall,
            new Alcove(colorBarTemplate, hallSide.ClockwiseTurn(), colorBar.Width, colorBar.Depth, (doorStart - colorBar.Distance - colorBar.HalfWidth) / (double)hallWidth)));
        alcoves.Add(_structureGenerator.AddStructure(hall,
          new Alcove(colorBarTemplate, hallSide.CounterClockwiseTurn(), colorBar.Width, colorBar.Depth, (doorStart - colorBar.Distance - colorBar.HalfWidth) / (double)hallWidth)));
        alcoves.Add(_structureGenerator.AddStructure(hall,
           new Alcove(colorBarTemplate, hallSide.ClockwiseTurn(), colorBar.Width, colorBar.Depth, (doorEnd + colorBar.Distance + colorBar.HalfWidth) / (double)hallWidth)));
        alcoves.Add(_structureGenerator.AddStructure(hall,
          new Alcove(colorBarTemplate, hallSide.CounterClockwiseTurn(), colorBar.Width, colorBar.Depth, (doorEnd + colorBar.Distance + colorBar.HalfWidth) / (double)hallWidth)));

        foreach (var alcove in alcoves)
            colorBar.SetOn(alcove);
    }

}
