namespace WadMaker.Queries;

public record TextureQuery(string[] ThemeNames, string? ColorName = null, int? MinWidth = null, int? MaxWidth = null)
{
    public Texture[] Execute()
    {
        var positiveThemes = ThemeNames.Where(p => !p.StartsWith("-")).ToArray();
        var negativeThemes = ThemeNames.Where(p => p.StartsWith("-")).Select(p=> p.TrimStart('-')).ToArray();

        var matches = DoomConfig.DoomTextureInfo.Values
            .Where(p => p.Themes.ContainsAll(positiveThemes) && !p.Themes.ContainsAny(negativeThemes));

        if (ColorName != null)
            matches = matches.Where(p => p.Color == ColorName);

        if(MinWidth != null)
            matches = matches.Where(p=> p.Size != null && p.Size.Width >= MinWidth);

        if (MaxWidth != null)
            matches = matches.Where(p => p.Size != null && p.Size.Width <= MaxWidth);

        return matches.Select(p => Enum.Parse<Texture>(p.Name)).ToArray();
    }
}
