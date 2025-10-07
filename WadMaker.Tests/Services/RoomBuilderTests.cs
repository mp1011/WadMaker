namespace WadMaker.Tests.Services;

internal class RoomBuilderTests : StandardTest
{
    [Test]
    public void CanBuildRoomWithInvertedCorners()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(128, -128),
        };

        room.Shape.Modifiers.Add(new InvertCorners { Width = 16 });

        var elements = RoomBuilder.Build(room);
        Assert.That(elements.Vertices, Has.Count.EqualTo(12));
        Assert.That(elements.LineDefs, Has.Count.EqualTo(12));
    }

    [Test]
    public void CanBuildRoomWithAngledCorners()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400),
        };

        room.Shape.Modifiers.Add(new AngledCorners { Width = 16 });

        var elements = RoomBuilder.Build(room);
        Assert.That(elements.Vertices, Has.Count.EqualTo(8));
    }


    [Test]
    public void CanBuildRoundRoom()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400),
        };

        room.Shape.Modifiers.Add(new NGon { Sides = 32 });

        var elements = RoomBuilder.Build(room);
        Assert.That(elements.Vertices, Has.Count.EqualTo(32));
    }

    [Test]
    public void CanBuildRoundWithNotchedSides()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400),
        };

        room.Shape.Modifiers.Add(new NotchedSides { Width = 128, Depth = 32 });

        var elements = RoomBuilder.Build(room);
        var linesByLength = elements.LineDefs.GroupBy(p => p.Length).ToDictionary(k=>k.Key, k => k.ToList());

        Assert.That(linesByLength[32].Count, Is.EqualTo(8));
        Assert.That(linesByLength[128].Count, Is.EqualTo(4));
    }


    [Test]
    public void CanBuildRoomWithInnerStructure()
    {
        var room = new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256),
        };

        room.AddInnerStructure(new Room
        {
            UpperLeft = new Point(64, -64),
            BottomRight = new Point(128, -128),
            Floor = -16,
            Ceiling = 8,
        });

        var roomElements = RoomBuilder.Build(room);
        var innerLines = roomElements.LineDefs.Skip(4).ToArray();

        Assert.That(innerLines, Has.Length.EqualTo(4));
        foreach (var line in innerLines)
        {
            Assert.That(line.Data.twosided, Is.True);
            Assert.That(line.Back, Is.Not.Null);
            Assert.That(line.Back.Sector, Is.EqualTo(roomElements.Sectors[0]));
        }
    }
}
