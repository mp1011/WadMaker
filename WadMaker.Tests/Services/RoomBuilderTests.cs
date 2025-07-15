using NuGet.Frameworks;

namespace WadMaker.Tests.Services;

internal class RoomBuilderTests
{
    [Test]
    public void CanBuildRoomWithInvertedCorners()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -128),
        };

        room.ShapeModifiers.Add(new InvertCorners { Width = 16 });

        var elements = new RoomBuilder(new IDProvider()).Build(room);
        Assert.That(elements.Vertices, Has.Count.EqualTo(12));
        Assert.That(elements.LineDefs, Has.Count.EqualTo(12));
    }

    [Test]
    public void CanBuildRoomWithInnerStructure()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256),
        };

        room.InnerStructures.Add(new Room
        {
            UpperLeft = new Point(64, -64),
            BottomRight = new Point(128, -128),
            Floor = -16,
            Ceiling = 8,
        });

        var roomBuilder = new RoomBuilder(new IDProvider());
        var roomElements = roomBuilder.Build(room);
        var innerElements = roomBuilder.BuildInternalElement(room, roomElements, room.InnerStructures.First());

        Assert.That(innerElements.LineDefs, Has.Count.EqualTo(4));
        foreach (var line in innerElements.LineDefs)
        {
            Assert.That(line.Data.twoSided, Is.True);
            Assert.That(line.Back, Is.Not.Null);
            Assert.That(line.Back.Sector, Is.EqualTo(roomElements.Sectors[0]));
        }
    }
}
