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
}
