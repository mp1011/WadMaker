
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

        var modifiedPoints = modifier.AlterPoints(points, new Room());

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
        room.ShapeModifiers.Add(new InvertCorners { Width = 64 });

        var innerShape = room.AddInnerStructure(new Room());
        innerShape.ShapeModifiers.Add(new CopyParentShape(parent: room, padding: new Padding(32)));

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
        room.ShapeModifiers.Add(new InvertCorners { Width = 64 });

        var innerShape = room.AddInnerStructure(new Room());
        innerShape.ShapeModifiers.Add(new CopyParentShape(parent: room, 
            padding: new Padding(8, 16, 32, 64)));

        var mapElements = MapBuilder.Build(map);

        var innerLines = mapElements.Sectors[1].Lines.GroupBy(p => p.Length).ToDictionary(g => g.Key, v => v.ToArray());

        Assert.That(innerLines[64].Length, Is.EqualTo(8));
        Assert.That(innerLines[232].Length, Is.EqualTo(2));
        Assert.That(innerLines[192].Length, Is.EqualTo(2));

    }
}
