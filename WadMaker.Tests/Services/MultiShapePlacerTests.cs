
namespace WadMaker.Tests.Services;

class MultiShapePlacerTests : StandardTest
{
    [TestCase]
    public void CanPlaceShapesAlongALine()
    {
        var map = new Map();
        var room = map.AddRoom(new Room(map, size: new Size(1000, 1000)));

        room.Pillars.Add(new Cutout(size: new Size(100, 100)));
        room.Pillars.Add(new Cutout(size: new Size(100, 100)));
        room.Pillars.Add(new Cutout(size: new Size(100, 100)));
        room.Pillars.Add(new Cutout(size: new Size(100, 100)));

        room.Pillars.Place().InLine(room, direction: Side.Right, position: 0.5, new Padding(100));

        Assert.That(room.Pillars[0].UpperLeft, Is.EqualTo(new Point(100, -450)));
        Assert.That(room.Pillars[1].UpperLeft, Is.EqualTo(new Point(333, -450)));
        Assert.That(room.Pillars[2].UpperLeft, Is.EqualTo(new Point(566, -450)));
        Assert.That(room.Pillars[3].UpperLeft, Is.EqualTo(new Point(799, -450)));
    }
}
