namespace WadMaker.Queries;

public record FlatsQuery(string? FlatName, string[]? ThemeNames = null, string? ColorName = null)
{
    public static FlatsQuery Default = new FlatsQuery(FlatName: Flat.Default.ToString());

    public FlatsQuery(string[] ThemeNames, string? ColorName = null) : this(null, ThemeNames, ColorName) { }
    public Flat[] Execute()
    {
        if (FlatName != null)
            return new[] { Enum.Parse<Flat>(FlatName) };

        if (ThemeNames == null)
            throw new Exception("At least one Theme must be provided");

        var positiveThemes = ThemeNames.Where(p => !p.StartsWith("-")).ToArray();
        var negativeThemes = ThemeNames.Where(p => p.StartsWith("-")).Select(p=> p.TrimStart('-')).ToArray();

        var matches = DoomConfig.DoomFlatsInfo.Values
            .Where(p => p.Themes.ContainsAll(positiveThemes) && !p.Themes.ContainsAny(negativeThemes));

        if (ColorName != null)
            matches = matches.Where(p => p.Color == ColorName);

        return matches.Select(p => Enum.Parse<Flat>(p.Name)).ToArray();
    }
    
    /// <summary>
    /// Prefer calling "Execute" directly
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Execute().FirstOrDefault().ToString() ?? Flat.Default.ToString();

    public static implicit operator FlatsQuery(Flat flat) => new FlatsQuery(flat.ToString());
}
