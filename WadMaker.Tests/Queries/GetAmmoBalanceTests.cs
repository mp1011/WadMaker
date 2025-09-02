namespace WadMaker.Tests.Queries;

internal class GetAmmoBalanceTests : StandardTest
{
    [TestCase(ThingType.Imp, 1, ThingType.Four_shotgun_shells, 1, ResourceBalance.Generous)]
    [TestCase(ThingType.Imp, 2, ThingType.Four_shotgun_shells, 1, ResourceBalance.Adequate)]
    [TestCase(ThingType.Imp, 4, ThingType.Four_shotgun_shells, 1, ResourceBalance.BarelyEnough)]
    [TestCase(ThingType.Imp, 6, ThingType.Four_shotgun_shells, 1, ResourceBalance.Insufficient)]
    public void CanDetermineAmmoToMonsterBalance(ThingType monster, int monsterCount, ThingType ammo, int ammoAmount, ResourceBalance expectedBalance)
    {
        var map = new TestMaps().LinearMap();

        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        ThingPlacer.AddFormation(ammo, mainRooms[1], ammoAmount, Angle.West, ThingFlags.AllSkillsAndModes, ThingPlacement.Center, ThingPattern.Row, 32);
     
        ThingPlacer.AddFormation(monster, mainRooms[3], monsterCount, Angle.West, ThingFlags.AllSkillsAndModes, ThingPlacement.Center, ThingPattern.Row, 32);

        var result = new GetAmmoBalance().Execute(mainRooms);
        Assert.That(result, Is.EqualTo(expectedBalance));
    }
}
