namespace WadMaker.Queries;

public record TextureQueryValue(int Value);

public record TextureQuery(string[]? ThemeNames = null, string? ColorName = null, 
    int? MinWidth = null, 
    int? MaxWidth = null,
    int? MinHeight = null, 
    int? MaxHeight = null,
    bool? RepeatsHorizontally = null,
    bool? RepeatsVertically = null,
    string? TextureName = null)
{
    public Texture[] Execute(LineDef target, TexturePart texturePart)
    {
        if(TextureName != null)
            return new[] { Enum.Parse<Texture>(TextureName) };

        if (ThemeNames == null)
            throw new Exception("At least one Theme must be provided");

        var positiveThemes = ThemeNames.Where(p => !p.StartsWith("-")).ToArray();
        var negativeThemes = ThemeNames.Where(p => p.StartsWith("-")).Select(p=> p.TrimStart('-')).ToArray();

        var matches = DoomConfig.DoomTextureInfo.Values
            .Where(p => p.Themes.ContainsAll(positiveThemes) && !p.Themes.ContainsAny(negativeThemes));

        if(RepeatsHorizontally.HasValue)
        {
            if (RepeatsHorizontally.Value) // texture is less wide than the line
                matches = matches.Where(p => p.Size!.Width < target.Length);
            else
                matches = matches.Where(p => p.Size!.Width >= target.Length);
        }

        if (RepeatsVertically.HasValue)
        {
            int lineHeight = LineHeight(target, texturePart);
            if (RepeatsVertically.Value) // texture is less tall than the line
                matches = matches.Where(p => p.Size!.Height < lineHeight);
            else
                matches = matches.Where(p => p.Size!.Height >= lineHeight);
        }

        if (ColorName != null)
            matches = matches.Where(p => p.Color == ColorName);

        if(MinWidth != null)
            matches = matches.Where(p=> p.Size != null && p.Size.Width >= MinWidth);

        if (MaxWidth != null)
            matches = matches.Where(p => p.Size != null && p.Size.Width <= MaxWidth);

        if (MinHeight != null)
            matches = matches.Where(p => p.Size != null && p.Size.Height >= MinHeight);

        if (MaxHeight != null)
            matches = matches.Where(p => p.Size != null && p.Size.Height <= MaxHeight);

        return matches.Select(p => Enum.Parse<Texture>(p.Name)).ToArray();
    }


    private int LineHeight(LineDef target, TexturePart texturePart)
    {
        if(texturePart == TexturePart.Middle)
        {
            return target.Sectors.Select(p => p.Height).Min();
        }
        else if(texturePart == TexturePart.Lower)
        {
            int[] floors = target.Sectors.Select(p => p.FloorHeight).ToArray();
            if (floors.Length < 2)
                return 0;
            else
                return floors.Max() - floors.Min();
        }
        else if (texturePart == TexturePart.Upper)
        {
            int[] ceilings = target.Sectors.Select(p => p.CeilingHeight).ToArray();
            if (ceilings.Length < 2)
                return 0;
            else
                return ceilings.Max() - ceilings.Min();
        }

        return 0;
    }
}
