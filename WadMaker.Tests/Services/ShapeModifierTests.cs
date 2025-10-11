
namespace WadMaker.Tests.Services;

class ShapeModifierTests : StandardTest
{
    [Test]
    public void CanInvertCorners()
    {
        var modifier = new InvertCorners {  Width = 10 };

        var points = new[]
        {
            new Point(0, 0),
            new Point(100, 0),
            new Point(100, -100),
            new Point(0,-100)
        };

        var modifiedPoints = modifier.AlterPoints(points);

        var expectedPoints = new[]
        {
            new Point(0, -10),
            new Point(10, -10),
            new Point(10, 0),

            new Point(90, 0),
            new Point(90, -10),
            new Point(100, -10),

            new Point(100, -90),
            new Point(90, -90),
            new Point(90, -100),

            new Point(10, -100),
            new Point(10, -90),
            new Point(0, -90)
        };

        Assert.That(modifiedPoints, Is.EquivalentTo(expectedPoints));
    }

    [Test]
    public void CanMatchShapeOfParent()
    {
        var map = new Map();
        var room = map.AddRoom(new Room(map, center: Point.Empty, size: new Size(400, 400)));
        room.Shape.Modifiers.Add(new InvertCorners { Width = 64 });

        var innerShape = room.AddInnerStructure(new Room());
        innerShape.Shape.Initializer = new CopyParentShape(parent: room, padding: new Padding(32));

        var mapElements = MapBuilder.Build(map);

        var innerLines = mapElements.Sectors[1].Lines.GroupBy(p => p.Length).ToDictionary(g => g.Key, v => v.ToArray());

        Assert.That(innerLines[64].Length, Is.EqualTo(8));
        Assert.That(innerLines[208].Length, Is.EqualTo(4));
    }

    [Test]
    public void CanMatchShapeOfParentWithUnevenPadding()
    {
        var map = new Map();
        var room = map.AddRoom(new Room(map, center: Point.Empty, size: new Size(400, 400)));
        room.Shape.Modifiers.Add(new InvertCorners { Width = 64 });

        var innerShape = room.AddInnerStructure(new Room());
        innerShape.Shape.Initializer = new CopyParentShape(parent: room, 
            padding: new Padding(8, 16, 32, 64));

        var mapElements = MapBuilder.Build(map);

        var innerLines = mapElements.Sectors[1].Lines.GroupBy(p => p.Length).ToDictionary(g => g.Key, v => v.ToArray());

        Assert.That(innerLines[64].Length, Is.EqualTo(8));
        Assert.That(innerLines[232].Length, Is.EqualTo(2));
        Assert.That(innerLines[192].Length, Is.EqualTo(2));
    }

    [Test]
    public void ShapeBoundsAreUpdatedWhenShapeMatchesParent()
    {
        var map = new Map();
        var room = map.AddRoom(new Room(map, center: Point.Empty, size: new Size(400, 400)));

        var inner = room.AddInnerStructure(size: new Size(0, 0));
        inner.Shape.Initializer = new CopyParentShape(new Padding(10), room);

        Assert.That(inner.UpperLeft, Is.EqualTo(new Point(10, -10)));
        Assert.That(inner.Bounds.Width, Is.EqualTo(380));
    }

    [Test]
    public void CanMakeLShapedRoom()
    {
        var map = new Map();
        var room = map.AddRoom(center: Point.Empty, size: new Size(400, 400));
        var room2 = map.AddRoom(center: Point.Empty, size: new Size(400, 400));
        room2.Place().EastOf(room, 256);
        room2.Shape.Center = room2.Shape.Center.Add(0, 400);

        var bendyHall = map.AddRoom(size: new Size(128, 128 * 5));
        bendyHall.Place().NorthOf(room, 0);
        bendyHall.Shape.UpperLeft = bendyHall.Shape.UpperLeft.Set(y: room2.UpperLeft.Y - 64);
        bendyHall.Shape.BottomRight = bendyHall.Shape.BottomRight.Set(x: room2.UpperLeft.X);
        bendyHall.Shape.Modifiers.Add(new Bend(Side.Bottom | Side.Right, 128));
        bendyHall.Tag = 1;

        var mapElements = MapBuilder.Build(map);
        var bendSector = mapElements.Sectors.First(s => s.Tag == 1);

        var cornerVertex = bendSector.Lines.SelectMany(p => p.Vertices).FirstOrDefault(p => p == new vertex(64, 408));
        Assert.That(cornerVertex, Is.Not.Null);
    }


    [TestCase(Side.Top | Side.Left)]
    [TestCase(Side.Top | Side.Right)]
    [TestCase(Side.Bottom | Side.Left)]
    [TestCase(Side.Bottom | Side.Right)]
    public void CanMakeLShapedRoomForAllCorners(Side corner)
    {
        var map = new Map();
        var room = map.AddRoom(center: Point.Empty, size: new Size(400, 400));
        room.Shape.Modifiers.Add(new Bend(corner, 128));
       
        var mapElements = MapBuilder.Build(map);

        var hallEnds = mapElements.LineDefs.Where(p => p.Length == 128).ToArray();
        Assert.That(hallEnds.Length == 2); 
    }
}
