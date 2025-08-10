namespace WadMaker.Tests.Services;

internal class ThingPlacerTests : StandardTest
{

    [TestCase(1, new int[] { 628, -128 })]
    [TestCase(2, new int[] { 628, -112, 628, -144 })]
    [TestCase(4, new int[] { 628, -80, 628, -112, 628, -144, 628, -176 })]
    public void CanAddMonsterFormation(int count, int[] expectedPositions)
    {
        var map = new TestMaps().LinearMap();
        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        ThingPlacer.AddFormation(ThingType.Imp, mainRooms[1], count, Angle.West, ThingFlags.AllSkillsAndModes, 0.5, 0.5, 32);

        var expectedPoints = IntArrayToPointList(expectedPositions);
        Assert.That(mainRooms[1].Things.Count, Is.EqualTo(expectedPoints.Length));

        foreach(var thing in mainRooms[1].Things.WithIndex())
        {
            Assert.That(thing.Item.Position, Is.EqualTo(expectedPoints[thing.Index])); 
        }
    }

    [Test]
    public void CanDetermineAmmoToMonsterBalance()
    {
        var map = new TestMaps().LinearMap();

        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        var path = new PlayerPath(mainRooms.Select(p=> new PlayerPathNode([p], Array.Empty<Room>())).ToArray());

        var foo = MapToUDMF(map);
        throw new NotImplementedException();
    }
}