using WadMaker.Queries;

namespace WadMaker.Tests.Queries;

class IsPointInSectorTests : StandardTest
{
    [TestCase(0,0, false)]
    [TestCase(-5, 0, false)] // outside to the left
    [TestCase(0, 5, false)] // within upper left cutout
    [TestCase(60, -5, true)] // inside
    [TestCase(401, -5, false)] // outside to the right
    [TestCase(150, -150, false)] // within first pillar
    [TestCase(305, -310, false)] // within second pillar
    [TestCase(90, -100, true)] // inside, next to first pillar
    [TestCase(360, -350, true)] // inside, next to second pillar
    public void CanDetermineIfPointIsInsideSector(int x, int y, bool shouldBeInside)
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400)
        };

        room.Pillars.Add(new Cutout
        {
            UpperLeft = new Point(100, -100),
            BottomRight = new Point(200, -200),
        });

        room.Pillars.Add(new Cutout
        {
            UpperLeft = new Point(300, -300),
            BottomRight = new Point(340, -340),
        });

        room.ShapeModifiers.Add(new InvertCorners { Width = 50 });

        var map = new Map();
        map.Rooms.Add(room);

        var mapElements = MapBuilder.Build(map);

        var query = new IsPointInSector();
        Assert.That(query.Execute(new Point(x,y), mapElements.Sectors[0], mapElements), Is.EqualTo(shouldBeInside));
    }
}
