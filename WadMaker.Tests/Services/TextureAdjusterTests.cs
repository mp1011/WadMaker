namespace WadMaker.Tests.Services;

internal class TextureAdjusterTests : StandardTest
{
    [Test]
    public void CanAlignTextures()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = Point.Empty,
            BottomRight = new Point(70, -100),
            WallTexture = new TextureInfo(Texture.BRICK7),
        });

        var elements = MapBuilder.Build(map);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        Assert.That(elements.LineDefs[0].Front.Data.offsetx, Is.EqualTo(0));
        Assert.That(elements.LineDefs[1].Front.Data.offsetx, Is.EqualTo(6));
        Assert.That(elements.LineDefs[2].Front.Data.offsetx, Is.EqualTo(42));
        Assert.That(elements.LineDefs[3].Front.Data.offsetx, Is.EqualTo(48));
    }

    [Test]
    public void CanAlignMultipleTextures()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = Point.Empty,
            BottomRight = new Point(70, -100),
        });
        map.Rooms[0].SideTextures[Side.Left] = new TextureInfo(Texture.BRICK7);
        map.Rooms[0].SideTextures[Side.Top] = new TextureInfo(Texture.BRICK7);
        map.Rooms[0].SideTextures[Side.Right] = new TextureInfo(Texture.BRICK6);
        map.Rooms[0].SideTextures[Side.Bottom] = new TextureInfo(Texture.BRICK6);


        var elements = MapBuilder.Build(map);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        Assert.That(elements.LineDefs[0].Front.Data.offsetx, Is.EqualTo(36)); // top
        Assert.That(elements.LineDefs[1].Front.Data.offsetx, Is.EqualTo(0)); // right
        Assert.That(elements.LineDefs[2].Front.Data.offsetx, Is.EqualTo(36)); // bottom
        Assert.That(elements.LineDefs[3].Front.Data.offsetx, Is.EqualTo(0)); // left
    }

    [Test] 
    public void CanAdjustUpperAndLowerTextures()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = Point.Empty,
            BottomRight = new Point(300, -100),
            WallTexture = new TextureInfo(Texture.BRICK7),
        });

        RoomGenerator.AddStructure(map.Rooms[0],
           new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
           Side: Side.Top,
           Width: 100,
           Depth: 32,
           CenterPercent: 0.50));

        var elements = MapBuilder.Build(map);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        var topLines = elements.LineDefs.Where(p => p.V1.y == 0 && p.V2.y == 0).ToArray();

        Assert.That(topLines[0].Front.Data.offsetx, Is.EqualTo(52)); // before alcove
        Assert.That(topLines[1].Front.Data.offsetx, Is.EqualTo(24)); // alcove
        Assert.That(topLines[2].Front.Data.offsetx, Is.EqualTo(60)); // after alcove

        var twoSidedLine = elements.LineDefs.First(p => p.Back != null);
        Assert.That(twoSidedLine.Data.dontpegtop, Is.True);
        Assert.That(twoSidedLine.Data.dontpegbottom, Is.True);
    }

    [Test]
    public void CanApplyThemeToDoors()
    {
        var testMap = new TestMaps().TextureTestMap();

        var theme = new Theme(new ThemeRule[]
        {
            new ThemeRule(new TextureInfo(Texture.BIGDOOR5), new IsDoor()),
        });

        foreach(var room in testMap.Rooms)
        {
            room.Theme = theme;
        }

        var mapElements = MapBuilder.Build(testMap);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);

        var doorLines = mapElements.LineDefs.Where(p => p.LineSpecial?.Type == Models.LineSpecials.LineSpecialType.DoorRaise).ToArray();
        Assert.That(doorLines, Is.Not.Empty);
        foreach(var doorLine in doorLines)
        {
            Assert.That(doorLine.Front.Data.texturetop, Is.EqualTo(Texture.BIGDOOR5.ToString()));
        }
    }

    [TestCase("Tech", "Brown", Texture.BIGDOOR4)]
    [TestCase("Tech", "Green", Texture.BIGDOOR3)]
    [TestCase("Wood", "Brown", Texture.BIGDOOR5)]
    public void CanApplyNamedThemeToDoors(string themeName, string color, Texture expectedDoor)
    {
        var testMap = new TestMaps().TextureTestMap();

        var theme = new Theme(new ThemeRule[]
        {
            new ThemeRule(new [] { themeName, "Door" }, color, new IsDoor()),
        });

        foreach (var room in testMap.Rooms)
        {
            room.Theme = theme;
        }

        var mapElements = MapBuilder.Build(testMap);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);

        var doorLines = mapElements.LineDefs.Where(p => p.LineSpecial?.Type == Models.LineSpecials.LineSpecialType.DoorRaise).ToArray();
        Assert.That(doorLines, Is.Not.Empty);
        foreach (var doorLine in doorLines)
        {
            Assert.That(doorLine.Front.Data.texturetop, Is.EqualTo(expectedDoor.ToString()));
        }
    }

    [Test]
    public void CanApplyTechTheme()
    {
        var testMap = new TestMaps().TextureTestMap();
        testMap.Theme = new TechbaseTheme();
 
        var udmf = MapToUDMF(testMap);
        var expected = File.ReadAllText("Fixtures//texture_test_techbase.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }
}

