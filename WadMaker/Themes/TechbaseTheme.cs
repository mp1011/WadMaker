using WadMaker.Models.Theming;

namespace WadMaker.Themes;

public record TechbaseTheme(int Version = 0) : Theme(CreateRules(Version))
{
    public static IEnumerable<ThemeRule> CreateRules(int version)
    { 
        // nukage pit walls
        yield return new ThemeRule(new TextureQuery(new[] { "SlimeBottom" }, RepeatsVertically: false),
                                   Texture: new TextureInfo(DrawLowerFromBottom: true),
            Conditions: new LowerFloorTextureIs(new FlatsQuery(new[] { "Slime" })));

        yield return new ThemeRule(new TextureQuery( new[] { "Tech", "Door" }, "Brown"), 
            Conditions: new IsDoor());

        yield return new ThemeRule(new TextureQuery( new[] { "Step" }), 
            Conditions: new FloorDifferenceLessOrEqualTo(16));

        // door traks
        if (version > 0)
        {
            yield return new ThemeRule(new TextureQuery(new[] { "DoorSide" }),
                Texture: new TextureInfo(LowerUnpegged: true),
                Conditions: new IsDoorSide());
        }

        // short walls
        yield return new ThemeRule(new TextureQuery(new[] { "Light" }, MaxWidth: 16, MinWidth:16),
            Conditions: new LineLengthIs(16).AndNot(new IsDoorSide()));

        // main walls
        yield return new ThemeRule(new TextureQuery(new[] { "Tech", "-Door" }, "Brown", MinWidth: 128),
           Conditions: new SectorHeightGreaterOrEqualTo(112)
            .AndNot(new HasLineSpecial()));
    }
}
