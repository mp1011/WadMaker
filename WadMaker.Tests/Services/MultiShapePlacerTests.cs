
namespace WadMaker.Tests.Services;

class MultiShapePlacerTests : StandardTest
{
    [TestCase]
    public void CanPlaceShapesAlongALine()
    {
        var map = new Map();
        var room = map.AddRoom(new Room(map, size: new Size(1000, 1000)));

        room.AddPillar(new Cutout(size: new Size(100, 100)));
        room.AddPillar(new Cutout(size: new Size(100, 100)));
        room.AddPillar(new Cutout(size: new Size(100, 100)));
        room.AddPillar(new Cutout(size: new Size(100, 100)));

        room.Pillars.Place().InLine(room, direction: Side.Right, position: 0.5, new Padding(100));

        Assert.That(room.Pillars.ElementAt(0).UpperLeft, Is.EqualTo(new Point(100, -450)));
        Assert.That(room.Pillars.ElementAt(1).UpperLeft, Is.EqualTo(new Point(333, -450)));
        Assert.That(room.Pillars.ElementAt(2).UpperLeft, Is.EqualTo(new Point(566, -450)));
        Assert.That(room.Pillars.ElementAt(3).UpperLeft, Is.EqualTo(new Point(799, -450)));
    }

    [TestCase(3,3)]
    [TestCase(6, 3)]
    [TestCase(5, 5)]
    public void CanPlacesShapesInAGrid(int rows, int columns)
    {
        var map = new Map();
        var room = map.AddRoom(new Room(map, size: new Size(1000, 1000)));

        for(int i = 0; i < rows * columns; i++)
        {
            room.AddPillar(new Cutout(size: new Size(50, 50)));
        }

        room.Pillars.Place().InGrid(room, columns: columns, new Padding(Horizontal: 100, Vertical: 200));

        var rowGroups = room.Pillars.GroupBy(p => p.UpperLeft.Y).ToArray();

        Assert.That(rowGroups.Length, Is.EqualTo(rows));

        foreach(var rowGroup in rowGroups)
        {
            Assert.That(rowGroup.Count(), Is.EqualTo(columns));

            foreach(var pillar in rowGroup.ToArray().WithNeighbors())
            {
                // skip wraparound neighbors
                if(pillar.Item2.UpperLeft.X < pillar.Item1.UpperLeft.X || pillar.Item3.UpperLeft.X < pillar.Item2.UpperLeft.X)
                    continue;

                var dist1 = Math.Abs(pillar.Item1.Bounds().Center.X - pillar.Item2.Bounds().Center.X);
                var dist2 = Math.Abs(pillar.Item2.Bounds().Center.X - pillar.Item3.Bounds().Center.X);

                Assert.That(dist1, Is.Not.EqualTo(0));
                Assert.That(dist1, Is.EqualTo(dist2));
            }
        }
    }
}
