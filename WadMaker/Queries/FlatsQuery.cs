namespace WadMaker.Queries;

public record FlatsQuery(string[] ThemeNames, string? ColorName = null)
{
    public Flat[] Execute()
    {
        var positiveThemes = ThemeNames.Where(p => !p.StartsWith("-")).ToArray();
        var negativeThemes = ThemeNames.Where(p => p.StartsWith("-")).Select(p=> p.TrimStart('-')).ToArray();

        var matches = DoomConfig.DoomFlatsInfo.Values
            .Where(p => p.Themes.ContainsAll(positiveThemes) && !p.Themes.ContainsAny(negativeThemes));

        if (ColorName != null)
            matches = matches.Where(p => p.Color == ColorName);

        return matches.Select(p => Enum.Parse<Flat>(p.Name)).ToArray();
    }
}
