namespace WadMaker.Themes;

public record TechbaseTheme() : Theme(CreateRules())
{
    public static IEnumerable<ThemeRule> CreateRules()
    {
        yield return new ThemeRule(new TextureQuery( new[] { "Tech", "Door" }, "Brown"), new IsDoor());
        yield return new ThemeRule(new TextureQuery( new[] { "Step" }), new FloorDifferenceLessOrEqualTo(16));

        // short walls
        yield return new ThemeRule(new TextureQuery(new[] { "Light" }, MaxWidth: 16, MinWidth:16),
            new LineLengthIs(16).AndNot(new IsDoorSide()));

        // main walls
        yield return new ThemeRule(new TextureQuery(new[] { "Tech", "-Door" }, "Brown", MinWidth: 128), 
            new SectorHeightGreaterOrEqualTo(112)
            .AndNot(new HasLineSpecial()));
    }
}
