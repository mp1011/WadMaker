using WadMaker.Models;

namespace WadMaker.Tests.Services;

internal class ThingPlacerTests : StandardTest
{

    [TestCase(1, new int[] { 628, -128 })]
    [TestCase(2, new int[] { 628, -108, 628, -148 })]
    [TestCase(4, new int[] { 628, -68, 628, -108, 628, -148, 628, -188 })]
    public void CanAddMonsterFormation(int count, int[] expectedPositions)
    {
        var map = new TestMaps().LinearMap();
        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        ThingPlacer.AddFormation(ThingType.Imp, mainRooms[1], count, Angle.West, ThingFlags.AllSkillsAndModes, 0.5, 0.5, 40);

        var expectedPoints = IntArrayToPointList(expectedPositions);
        Assert.That(mainRooms[1].Things.Count, Is.EqualTo(expectedPoints.Length));

        foreach(var thing in mainRooms[1].Things.WithIndex())
        {
            Assert.That(thing.Item.Position, Is.EqualTo(expectedPoints[thing.Index])); 
        }
    }

    [TestCase]
    public void CanPlaceMonsters()
    {
        var map = new TestMaps().LinearMap();
        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        var path = new PlayerPath(mainRooms.Select(p=> new PlayerPathNode(new Room[] { p }, Array.Empty<Room>())).ToArray());

        ThingPlacer.AddMonsters(path,
            new MonsterPlacement(ThingType.Imp, 0.5, 1.0, EnemyDensity.Common));

        Assert.That(mainRooms[0].Things.Count(p=>p.ThingType == ThingType.Imp), Is.EqualTo(0));
        Assert.That(mainRooms[1].Things.Count(p => p.ThingType == ThingType.Imp), Is.EqualTo(0));
        Assert.That(mainRooms[2].Things.Count(p => p.ThingType == ThingType.Imp), Is.InRange(3, 5));
        Assert.That(mainRooms[3].Things.Count(p => p.ThingType == ThingType.Imp), Is.InRange(3, 5));
        Assert.That(mainRooms[4].Things.Count(p => p.ThingType == ThingType.Imp), Is.InRange(3, 5));
    }

    [TestCase]
    public void CanPlaceTwoMonstersWithoutOverlap()
    {
        var map = new TestMaps().LinearMap();
        ThingPlacer.AddThing(ThingType.Imp, map.Rooms[1], 0.5, 0.5, ThingFlags.AllSkillsAndModes);
        ThingPlacer.AddThing(ThingType.Hell_knight, map.Rooms[1], 0.5, 0.5, ThingFlags.AllSkillsAndModes);

        var imp = map.Rooms[1].Things.First(p => p.ThingType == ThingType.Imp);
        var hellKnight = map.Rooms[1].Things.First(p => p.ThingType == ThingType.Hell_knight);

        Assert.That(imp.Overlaps(hellKnight), Is.False);
    }

    [TestCase]
    public void CanPlaceMultipleMonstersWithoutOverlap()
    {
        var map = new TestMaps().LinearMap();

        for(int i = 0; i < 4; i++)
            ThingPlacer.AddThing(ThingType.Imp, map.Rooms[1], 0.5, 0.5, ThingFlags.AllSkillsAndModes);

        // separate for easier debugging
        ThingPlacer.AddThing(ThingType.Hell_knight, map.Rooms[1], 0.5, 0.5, ThingFlags.AllSkillsAndModes);

        EnsureNoOverlaps(map.Rooms[1]);
    }

    private void EnsureNoOverlaps(Room room)
    {
        foreach (var thing in room.Things)
        {
            foreach (var otherThing in room.Things)
            {
                if (thing == otherThing)
                    continue;

                Assert.That(thing.Overlaps(otherThing), Is.False);
            }
        }
    }

    [TestCase]
    public void CanPlaceMultipleMonsterTypesAlongPath()
    { 
        var map = new TestMaps().LinearMap();
        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        var path = new PlayerPath(mainRooms.Select(p => new PlayerPathNode(new Room[] { p }, Array.Empty<Room>())).ToArray());

        ThingPlacer.AddMonsters(path,
            new MonsterPlacement(ThingType.Imp, 0.5, 1.0, EnemyDensity.Common),
            new MonsterPlacement(ThingType.Hell_knight, 1.0, 1.0, EnemyDensity.Single));

        Assert.That(mainRooms[4].Things.Count(p => p.ThingType == ThingType.Imp), Is.InRange(3, 5));
        Assert.That(mainRooms[4].Things.Count(p => p.ThingType == ThingType.Hell_knight), Is.EqualTo(1));

        EnsureNoOverlaps(mainRooms[4]);
    }
}