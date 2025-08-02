namespace WadMaker.Tests.Services;

internal class RoomGeneratorTests : StandardTest
{
    [Test]
    public void CanAddAlcove()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400),
        };

        var alcove = RoomGenerator.AddStructure(room,
            new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
            Side: Side.Left,
            Width: 100,
            Depth: 32,
            CenterPercent: 0.50));

        Assert.That(alcove.Bounds.Width, Is.EqualTo(32));
        Assert.That(alcove.Bounds.Height, Is.EqualTo(100));

        Assert.That(alcove.UpperLeft.X, Is.EqualTo(-32));
        Assert.That(alcove.BottomRight.X, Is.EqualTo(0));
    }

    [Test]
    public void CanAddNorthAlcove()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -300),
        };

        var alcove = RoomGenerator.AddStructure(room,
            new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
            Side: Side.Top,
            Width: 100,
            Depth: 32,
            CenterPercent: 0.50));

        Assert.That(alcove.Bounds.Height, Is.EqualTo(32));
        Assert.That(alcove.Bounds.Width, Is.EqualTo(100));

        Assert.That(alcove.UpperLeft.X, Is.EqualTo(150));
   }
}
