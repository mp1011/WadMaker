namespace WadMaker.Tests.Services;

internal class OutOfBoundsThingResolverTests : StandardTest
{
    [TestCase(ThingPattern.Circle)]
    [TestCase(ThingPattern.Row)]
    public void ItemsAreNotPlacedInVoid(ThingPattern pattern)
    {
        var map = new TestMaps().RoomWithPillar();
        var mapElements = MapBuilder.Build(map);

        var path = new PlayerPath([new PlayerPathNode(new Room[] { map.Rooms[0] }, Array.Empty<Room>())]);
        ThingPlacer.AddMonsters(path, new MonsterPlacement(ThingType.Imp, 0.0, 1.0, EnemyDensity.Excessive, Angle.East, pattern));

        OutOfBoundsThingResolver.EnsureThingsAreInBounds(map, mapElements);

        Assert.That(map.Rooms[0].Things.Count, Is.GreaterThan(0));

        foreach (var thing in map.Rooms[0].Things)
        {
            Assert.That(Query.IsThingInBounds.Execute(thing, mapElements), Is.True);
        }
    }
}