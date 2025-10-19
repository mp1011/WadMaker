namespace WadMaker.Models.Theming;

public record Theme(ThemeRule[] Rules)
{
    public Theme(IEnumerable<ThemeRule> Rules) : this(Rules.ToArray()) { }

    public IEnumerable<ThemeRule> LineRules => Rules.Where(r => r.Query != null || r.Texture != null);
    public IEnumerable<ThemeRule> SectorRules => Rules.Where(r => r.FloorQuery != null || r.CeilingQuery != null
        || r.Floor != null || r.Ceiling != null);
}

public record ThemeRule(TextureQuery? Query = null,
    TextureInfo? Texture = null,
    FlatsQuery? FloorQuery = null,
    FlatsQuery? CeilingQuery = null,   
    Flat? Floor = null,
    Flat? Ceiling = null,
    params ThemeCondition[] Conditions)
{
    public bool AppliesTo(LineDef lineDef) => Conditions.All(c => c.AppliesTo(lineDef));
    public bool AppliesTo(Sector sector) => Conditions.All(c => c.AppliesTo(sector));

    public Flat GetFloor(Sector sector)
    {
        if (FloorQuery == null)
            return Floor ?? sector.Floor;

        var floor = FloorQuery.Execute().FirstOrDefault();
        return floor;
    }

    public Flat GetCeiling(Sector sector)
    {
        if (CeilingQuery == null)
            return Ceiling ?? sector.Ceiling;

        var ceiling = CeilingQuery.Execute().FirstOrDefault();
        return ceiling;
    }
}

