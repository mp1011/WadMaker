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

    public TextureInfo GetTexture(LineDef lineDef)
    {
        if (Query == null)
            return Texture ?? new TextureInfo();
        
        var upper = Query.Execute(lineDef, TexturePart.Upper).FirstOrDefault();
        var middle = Query.Execute(lineDef, TexturePart.Middle).FirstOrDefault();
        var lower = Query.Execute(lineDef, TexturePart.Lower).FirstOrDefault();

        if (Texture == null)
            return new TextureInfo(Mid: middle, Upper: upper, Lower: lower);

        return Texture with
        {
            Upper = Texture.Upper ?? new TextureQuery(upper),
            Mid = Texture.Mid ?? new TextureQuery(middle),
            Lower = Texture.Lower ?? new TextureQuery(lower)
        };
    }

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

