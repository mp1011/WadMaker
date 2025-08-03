namespace WadMaker.Themes;

public record TechbaseTheme() : Theme(CreateRules())
{
    public static IEnumerable<ThemeRule> CreateRules()
    {
        yield return new ThemeRule(new[] { "Tech", "Door" }, "Brown", new IsDoor());
        yield return new ThemeRule(new[] { "Step" }, "Gray", new FloorDifferenceLessOrEqualTo(16));

        // main wall
        yield return new ThemeRule(new[] { "Tech", "-Door" }, "Brown", 
            new SectorHeightGreaterOrEqualTo(112).AndNot(new HasLineSpecial()));

        yield return new ThemeRule(new TextureInfo(Texture.TEKWALL1), new TrueCondition());
    }
}
