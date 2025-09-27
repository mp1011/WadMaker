namespace WadMaker.Tests.Services;

internal class RoomPositionResolverTests : StandardTest
{
    [Test]
    public void CanBuildMapWithDeferredGeometry()
    {
        var map = new Map();

        var centerRoom = map.AddRoom(new Room(map));
        var northRoom = centerRoom.CreateNeighbor(Side.Top, Anchor.MidPoint, Anchor.MidPoint, spacing: 0).AddTo(map);
        var southRoom = centerRoom.CreateNeighbor(Side.Bottom, Anchor.MidPoint, Anchor.MidPoint, spacing: 0).AddTo(map);
        var eastRoom = centerRoom.CreateNeighbor(Side.Right, Anchor.MidPoint, Anchor.MidPoint, spacing: 0).AddTo(map);
        var westRoom = centerRoom.CreateNeighbor(Side.Left, Anchor.MidPoint, Anchor.MidPoint, spacing: 0).AddTo(map);
        var southRoom2 = southRoom.CreateNeighbor(Side.Left, Anchor.MidPoint, Anchor.MidPoint, spacing: 0).AddTo(map);
        southRoom2.Size = new Size(64, 64);

        centerRoom.Center = new Point(0, 0);
        centerRoom.Size = new Size(256, 256);

        centerRoom.Tag = 1;
        northRoom.Tag = 2;

        RoomPositionResolver.Execute(map);

        Assert.That(northRoom.UpperLeft.Y, Is.EqualTo(centerRoom.UpperLeft.Y + northRoom.Size.Height));
        Assert.That(northRoom.BottomRight.Y, Is.EqualTo(centerRoom.UpperLeft.Y));

        Assert.That(westRoom.BottomRight.X, Is.EqualTo(centerRoom.UpperLeft.X));
        Assert.That(eastRoom.UpperLeft.X, Is.EqualTo(centerRoom.BottomRight.X));
        Assert.That(southRoom.UpperLeft.Y, Is.EqualTo(centerRoom.BottomRight.Y));
        Assert.That(southRoom2.BottomRight.X, Is.EqualTo(southRoom.UpperLeft.X));
    }
}
