namespace WadMaker.Tests.Services;

internal class RoomPositionResolverTests : StandardTest
{

    [Test]
    public void CanBuildMapWithDeferredGeometry_Basic()
    {
        var map = new Map();

        var centerRoom = map.AddRoom(new Room(map));
        var northRoom = map.AddRoom();
        centerRoom.Size = new Size(256, 256);
        centerRoom.Center = new Point(1028, -128);       
        northRoom.Center = new Point(64, -64);

        centerRoom.Tag = 1;
        northRoom.Tag = 2;
        northRoom.Place().NorthOf(centerRoom);

        Assert.That(northRoom.UpperLeft.Y, Is.EqualTo(centerRoom.UpperLeft.Y + northRoom.Size.Height));
        Assert.That(northRoom.BottomRight.Y, Is.EqualTo(centerRoom.UpperLeft.Y));
    }

    [Test]
    public void CanBuildMapWithDeferredGeometry()
    {
        var map = new Map();

        var centerRoom = map.AddRoom(new Room(map));
        var northRoom = map.AddRoom();
        var southRoom = map.AddRoom();
        var eastRoom = map.AddRoom();
        var westRoom = map.AddRoom();
        var southRoom2 = map.AddRoom();

        southRoom2.Size = new Size(64, 64);

        centerRoom.Center = new Point(0, 0);
        centerRoom.Size = new Size(256, 256);

        centerRoom.Tag = 1;
        northRoom.Tag = 2;

        northRoom.Place().NorthOf(centerRoom);
        southRoom.Place().SouthOf(centerRoom);
        eastRoom.Place().EastOf(centerRoom);
        westRoom.Place().WestOf(centerRoom);
        southRoom2.Place().WestOf(southRoom);

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

        var north1 = map.AddRoom();
        var north2 = map.AddRoom();

        var south1 = map.AddRoom();
        var south2 = map.AddRoom();

        var west1 = map.AddRoom();
        var west2 = map.AddRoom();

        var east1 = map.AddRoom();
        var east2 = map.AddRoom();

        centerRoom.Center = new Point(0, 0);
        centerRoom.Size = new Size(512, 512);

        north1.Place().ToSideOf(centerRoom, Side.Top, targetAnchor: Anchor.Percent(0.25));
        north2.Place().ToSideOf(centerRoom, Side.Top, targetAnchor: Anchor.Percent(0.75));

        south1.Place().ToSideOf(centerRoom, Side.Bottom, targetAnchor: Anchor.Percent(0.25));
        south2.Place().ToSideOf(centerRoom, Side.Bottom, targetAnchor: Anchor.Percent(0.75));

        west1.Place().ToSideOf(centerRoom, Side.Left, targetAnchor: Anchor.Percent(0.25));
        west2.Place().ToSideOf(centerRoom, Side.Left, targetAnchor: Anchor.Percent(0.75));

        east1.Place().ToSideOf(centerRoom, Side.Right, targetAnchor: Anchor.Percent(0.25));
        east2.Place().ToSideOf(centerRoom, Side.Right, targetAnchor: Anchor.Percent(0.75));

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