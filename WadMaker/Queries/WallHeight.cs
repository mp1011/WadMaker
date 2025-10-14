namespace WadMaker.Queries;

public class WallHeight(LineDef Line, TexturePart Part)
{
    public int Execute()
    {
        if (Part == TexturePart.Middle)
        {
            return Line.Sectors.Select(p => p.Height).Min();
        }
        else if (Part == TexturePart.Lower)
        {
            int[] floors = Line.Sectors.Select(p => p.FloorHeight).ToArray();
            if (floors.Length < 2)
                return 0;
            else
                return floors.Max() - floors.Min();
        }
        else if (Part == TexturePart.Upper)
        {
            int[] ceilings = Line.Sectors.Select(p => p.CeilingHeight).ToArray();
            if (ceilings.Length < 2)
                return 0;
            else
                return ceilings.Max() - ceilings.Min();
        }

        return 0;
    }

}
