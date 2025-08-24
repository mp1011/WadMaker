namespace WadMaker.Tests.Queries;

internal class GetAmmoBalanceTests : StandardTest
{
    [TestCase(ThingType.Imp, 1, ThingType.Four_shotgun_shells, 1, AmmoBalance.Generous)]
    [TestCase(ThingType.Imp, 2, ThingType.Four_shotgun_shells, 1, AmmoBalance.Adequate)]
    [TestCase(ThingType.Imp, 4, ThingType.Four_shotgun_shells, 1, AmmoBalance.BarelyEnough)]
    [TestCase(ThingType.Imp, 6, ThingType.Four_shotgun_shells, 1, AmmoBalance.Insufficient)]
    public void CanDetermineAmmoToMonsterBalance(ThingType monster, int monsterCount, ThingType ammo, int ammoAmount, AmmoBalance expectedBalance)
    {
        var map = new TestMaps().LinearMap();

        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        ThingPlacer.AddFormation(ammo, mainRooms[1], ammoAmount, Angle.West, ThingFlags.AllSkillsAndModes, 0.5, 0.5, 32);
     
        ThingPlacer.AddFormation(monster, mainRooms[3], monsterCount, Angle.West, ThingFlags.AllSkillsAndModes, 0.5, 0.5, 32);

        var result = new GetAmmoBalance().Execute(mainRooms);
        Assert.That(result, Is.EqualTo(expectedBalance));
    }
}
