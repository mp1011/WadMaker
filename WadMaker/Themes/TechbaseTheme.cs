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

        if (version <= 1)
        {
            yield return new ThemeRule(new TextureQuery(new[] { "Step" }),
                Conditions: new FloorDifferenceLessOrEqualTo(16));
        }
        else if(version >= 2)
        {
            yield return new ThemeRule(Floor: Flat.STEP2,
                Conditions: new FrontRoomBuildingBlockTypeIs<Stairs>());

            yield return new ThemeRule(Texture: new TextureInfo(Texture.TEKWALL1),
                Conditions: new FrontRoomBuildingBlockTypeIs<Alcove>().And(new LineLengthGreaterOrEqualTo(32)));

            yield return new ThemeRule(new TextureQuery(new[] { "Step" }),
                Conditions: new FloorDifferenceLessOrEqualTo(16).And(new FrontRoomBuildingBlockTypeIs<Stairs>()));
        }


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
