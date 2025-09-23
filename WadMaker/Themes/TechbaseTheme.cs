namespace WadMaker.Themes;

public record TechbaseTheme() : Theme(CreateRules())
{
    public static IEnumerable<ThemeRule> CreateRules()
    { 
        // nukage pit walls
        yield return new ThemeRule(new TextureQuery(new[] { "SlimeBottom" }, RepeatsVertically: false),
                                   new TextureInfo(DrawLowerFromBottom: true),
            Conditions: new LowerFloorTextureIs(new FlatsQuery(new[] { "Slime" })));

        yield return new ThemeRule(new TextureQuery( new[] { "Tech", "Door" }, "Brown"), 
            Conditions: new IsDoor());

        yield return new ThemeRule(new TextureQuery( new[] { "Step" }), 
            Conditions: new FloorDifferenceLessOrEqualTo(16));

      
        // short walls
        yield return new ThemeRule(new TextureQuery(new[] { "Light" }, MaxWidth: 16, MinWidth:16),
            Conditions: new LineLengthIs(16).AndNot(new IsDoorSide()));

        // main walls
        yield return new ThemeRule(new TextureQuery(new[] { "Tech", "-Door" }, "Brown", MinWidth: 128),
           Conditions: new SectorHeightGreaterOrEqualTo(112)
            .AndNot(new HasLineSpecial()));
    }
}
