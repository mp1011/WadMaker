namespace WadMaker.Tests.Services;

internal class RoomPositionResolverTests : StandardTest
{

    [Test]
    public void CanBuildMapWithDeferredGeometry_Basic()
    {
        var map = new Map();

        var centerRoom = map.AddRoom(new Room(map));
        var northRoom = centerRoom.CreateNeighbor(Side.Top, Anchor.MidPoint, Anchor.MidPoint, spacing: 0).AddTo(map);
        centerRoom.Size = new Size(256, 256);
        centerRoom.Center = new Point(1028, -128);
       
        northRoom.Center = new Point(64, -64);

        centerRoom.Tag = 1;
        northRoom.Tag = 2;

        RoomPositionResolver.Execute(map);

        Assert.That(northRoom.UpperLeft.Y, Is.EqualTo(centerRoom.UpperLeft.Y + northRoom.Size.Height));
        Assert.That(northRoom.BottomRight.Y, Is.EqualTo(centerRoom.UpperLeft.Y));
    }

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

    [Test]
    public void CanPlaceItemsNotConnectedAtCenter()
    {
        var map = new Map();

        var centerRoom = map.AddRoom(new Room(map));

        var north1 = centerRoom.CreateNeighbor(Side.Top, Anchor.Percent(0.25), Anchor.MidPoint, 0).AddTo(map);
        var north2 = centerRoom.CreateNeighbor(Side.Top, Anchor.Percent(0.75), Anchor.MidPoint, 0).AddTo(map);

        var south1 = centerRoom.CreateNeighbor(Side.Bottom, Anchor.Percent(0.25), Anchor.MidPoint, 0).AddTo(map);
        var south2 = centerRoom.CreateNeighbor(Side.Bottom, Anchor.Percent(0.75), Anchor.MidPoint, 0).AddTo(map);

        var west1 = centerRoom.CreateNeighbor(Side.Left, Anchor.Percent(0.25), Anchor.MidPoint, 0).AddTo(map);
        var west2 = centerRoom.CreateNeighbor(Side.Left, Anchor.Percent(0.75), Anchor.MidPoint, 0).AddTo(map);

        var east1 = centerRoom.CreateNeighbor(Side.Right, Anchor.Percent(0.25), Anchor.MidPoint, 0).AddTo(map);
        var east2 = centerRoom.CreateNeighbor(Side.Right, Anchor.Percent(0.75), Anchor.MidPoint, 0).AddTo(map);

        centerRoom.Center = new Point(0, 0);
        centerRoom.Size = new Size(512, 512);

        RoomPositionResolver.Execute(map);

        Assert.That(north1.UpperLeft, Is.EqualTo(new Point(-192, 384)));
        Assert.That(north2.UpperLeft, Is.EqualTo(new Point(64, 384)));
        Assert.That(south1.UpperLeft, Is.EqualTo(new Point(-192, -256)));
        Assert.That(south2.UpperLeft, Is.EqualTo(new Point(64, -256)));
        Assert.That(west1.UpperLeft, Is.EqualTo(new Point(-384, 192)));
        Assert.That(west2.UpperLeft, Is.EqualTo(new Point(-384, -64)));
        Assert.That(east1.UpperLeft, Is.EqualTo(new Point(256, 192)));
        Assert.That(east2.UpperLeft, Is.EqualTo(new Point(256, -64)));
    }
}