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

        ThingPlacer.AddFormation(ThingType.Imp, mainRooms[1], count, Angle.West, ThingFlags.AllSkillsAndModes, ThingPlacement.Center, ThingPattern.Row, 40);

        var expectedPoints = IntArrayToPointList(expectedPositions);
        Assert.That(mainRooms[1].Things.Count, Is.EqualTo(expectedPoints.Length));

        foreach(var thing in mainRooms[1].Things.WithIndex())
        {
            Assert.That(thing.Item.Position, Is.EqualTo(expectedPoints[thing.Index])); 
        }
    }

    [WithStaticFlags(clearUpperAndLowerTexturesOnTwoSidedLines: false)]
    [TestCase(ThingPattern.Triangle)]
    [TestCase(ThingPattern.Square)] // could be better, but fine for now
    [TestCase(ThingPattern.Circle)]
    public void CanAddMonsterFormation(ThingPattern pattern)
    {
        var map = new TestMaps().LinearMap();
        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        foreach(var room in mainRooms.WithIndex())
        {
            ThingPlacer.AddFormation(ThingType.Imp, room.Item, 3 + room.Index, Angle.West, ThingFlags.AllSkillsAndModes, ThingPlacement.Center, pattern, 40);
        }

        var udmf = MapToUDMF(map);
        var expected = File.ReadAllText($"Fixtures//{pattern.ToString().ToLower()}_formation_test_map.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
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
            new MonsterPlacement(ThingType.Imp, 0.5, 1.0, EnemyDensity.Common, Angle.East, ThingPattern.Row));

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
    public void ThingsOfDifferentKindsCanOverlap()
    {
        var map = new TestMaps().LinearMap();
        ThingPlacer.AddThing(ThingType.Imp, map.Rooms[1], 0.5, 0.5, ThingFlags.AllSkillsAndModes);
        ThingPlacer.AddThing(ThingType.Clip, map.Rooms[1], 0.5, 0.5, ThingFlags.AllSkillsAndModes);

        var imp = map.Rooms[1].Things.First(p => p.ThingType == ThingType.Imp);
        var bullets = map.Rooms[1].Things.First(p => p.ThingType == ThingType.Clip);

        Assert.That(imp.Overlaps(bullets), Is.True);

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
            new MonsterPlacement(ThingType.Imp, 0.5, 1.0, EnemyDensity.Common, Angle.East),
            new MonsterPlacement(ThingType.Hell_knight, 1.0, 1.0, EnemyDensity.Single, Angle.East));

        Assert.That(mainRooms[4].Things.Count(p => p.ThingType == ThingType.Imp), Is.InRange(3, 5));
        Assert.That(mainRooms[4].Things.Count(p => p.ThingType == ThingType.Hell_knight), Is.EqualTo(1));

        EnsureNoOverlaps(mainRooms[4]);
    }

    [TestCase]
    public void CanIncreaseMonsterDensityAlongPath()
    {
        var map = new TestMaps().LinearMap();
        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        var path = new PlayerPath(mainRooms.Select(p => new PlayerPathNode(new Room[] { p }, Array.Empty<Room>())).ToArray());

        ThingPlacer.AddMonsters(path,
            new MonsterPlacement(ThingType.Imp, 0.25, 0.49, EnemyDensity.Rare, Angle.East),
            new MonsterPlacement(ThingType.Imp, 0.5, 0.99, EnemyDensity.Common, Angle.East),
            new MonsterPlacement(ThingType.Imp, 0.99, 1.0, EnemyDensity.Excessive, Angle.East));


        Assert.That(mainRooms[0].Things.Count(p => p.ThingType == ThingType.Imp), Is.EqualTo(0));
        Assert.That(mainRooms[1].Things.Count(p => p.ThingType == ThingType.Imp), 
            Is.InRange(EnemyDensity.Rare.MonsterCount().Min, EnemyDensity.Rare.MonsterCount().Max)); // 0.25
        Assert.That(mainRooms[2].Things.Count(p => p.ThingType == ThingType.Imp),
            Is.InRange(EnemyDensity.Common.MonsterCount().Min, EnemyDensity.Common.MonsterCount().Max)); // 0.5
        Assert.That(mainRooms[3].Things.Count(p => p.ThingType == ThingType.Imp),
            Is.InRange(EnemyDensity.Common.MonsterCount().Min, EnemyDensity.Common.MonsterCount().Max)); // 0.75
        Assert.That(mainRooms[4].Things.Count(p => p.ThingType == ThingType.Imp),
            Is.InRange(EnemyDensity.Excessive.MonsterCount().Min, EnemyDensity.Excessive.MonsterCount().Max)); // 1.0
    }

    [TestCase(ResourceBalance.BarelyEnough)] // -3 imps
    [TestCase(ResourceBalance.Adequate)] // +10 shells
    [TestCase(ResourceBalance.Comfortable)] // +28 shells
    [TestCase(ResourceBalance.Generous)] // +35 shells
    public void CanAddAmmoAppropriateForMonstersAlongPath(ResourceBalance balance)
    {
        var map = new TestMaps().LinearMap();
        var mainRooms = map.Rooms
            .Where(p => p.Bounds.Width == 256)
            .OrderBy(p => p.UpperLeft.X)
            .ToArray();

        var path = new PlayerPath(mainRooms.Select(p => new PlayerPathNode(new Room[] { p }, Array.Empty<Room>())).ToArray());

        ThingPlacer.AddMonsters(path,
            new MonsterPlacement(ThingType.Imp, 0.25, 0.49, EnemyDensity.Rare, Angle.East),
            new MonsterPlacement(ThingType.Imp, 0.5, 0.99, EnemyDensity.Common, Angle.East),
            new MonsterPlacement(ThingType.Imp, 0.99, 1.0, EnemyDensity.Excessive, Angle.East));

        ThingPlacer.AddThing(ThingType.Shotgun, mainRooms[0], 0.75, 0.5);
        ThingPlacer.AddAmmo(path, balance);

        var result = new GetAmmoBalance().Execute(mainRooms);
        Assert.That(result, Is.EqualTo(balance));
    }

    [TestCase(0.5,0.5, true)]
    [TestCase(2.0, 0.5, true)]
    [TestCase(0.1, 0.5, false)]
    [TestCase(0.9, 0.9, false)]
    public void CanIdentifyIfThingIsInVoid(double x, double y, bool inVoid)
    {
        var map = new TestMaps().RoomWithPillar();
        var mapElements = MapBuilder.Build(map);

        ThingPlacer.AddThing(ThingType.Clip, map.Rooms[0], x, y);

        var placedThing = map.Rooms[0].Things.Single();

        Assert.That(Query.IsPointInSector.Execute(placedThing.Position, mapElements.Sectors[0], mapElements), Is.EqualTo(!inVoid));
    }
}