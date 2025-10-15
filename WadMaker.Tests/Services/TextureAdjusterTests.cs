using WadMaker.Config;
using WadMaker.Models;
using WadMaker.Models.LineSpecials;

namespace WadMaker.Tests.Services;

internal class TextureAdjusterTests : StandardTest
{
    protected override int ConfigVersion { get; } = 1;
    
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
        TextureAdjuster.ApplyTextures(elements);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        Assert.That(elements.LineDefs[0].Front.Data.offsetx, Is.EqualTo(0));
        Assert.That(elements.LineDefs[1].Front.Data.offsetx, Is.EqualTo(6));
        Assert.That(elements.LineDefs[2].Front.Data.offsetx, Is.EqualTo(42));
        Assert.That(elements.LineDefs[3].Front.Data.offsetx, Is.EqualTo(48));
    }

    [Test]
    public void CanAlignTexturesVertically()
    {
        var map = new TestMaps().TwoConnectedRoomsWithDifferentCeilings();

        foreach (var room in map.Rooms)
        {
            room.WallTexture = new TextureInfo(Texture.STARTAN2);
        }

        var elements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(elements);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        var lowerRoomLines = elements.Sectors.First(p => p.Data.heightceiling == 112)
                                             .Lines
                                             .Where(p => p.Back == null)
                                             .ToArray();

        var upperRoomLines = elements.Sectors.First(p => p.Data.heightceiling == 128)
                                             .Lines
                                             .Where(p => p.Back == null)
                                             .ToArray();

        Assert.That(lowerRoomLines, Is.Not.Empty);
        Assert.That(upperRoomLines, Is.Not.Empty);

        foreach (var line in lowerRoomLines)
            Assert.That(line.Front.Data.offsety, Is.EqualTo(16));

        foreach (var line in upperRoomLines)
            Assert.That(line.Front.Data.offsety, Is.Null);
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
        TextureAdjuster.ApplyTextures(elements);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        Assert.That(elements.LineDefs[0].Front.Data.offsetx, Is.EqualTo(36)); // top
        Assert.That(elements.LineDefs[1].Front.Data.offsetx, Is.EqualTo(0)); // right
        Assert.That(elements.LineDefs[2].Front.Data.offsetx, Is.EqualTo(36)); // bottom
        Assert.That(elements.LineDefs[3].Front.Data.offsetx, Is.EqualTo(0)); // left
    }

    [Test]
    [WithStaticFlags(LegacyFlags.OverwriteExistingXOffset)]
    public void CanAdjustUpperAndLowerTextures()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = Point.Empty,
            BottomRight = new Point(300, -100),
            WallTexture = new TextureInfo(Texture.BRICK7),
        });

        StructureGenerator.AddStructure(map.Rooms[0],
           new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
           Side: Side.Top,
           Width: 100,
           Depth: 32,
           CenterPercent: 0.50));

        var elements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(elements);
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
            new ThemeRule(Texture: new TextureInfo(Texture.BIGDOOR5), Conditions: new IsDoor()),
        });

        foreach(var room in testMap.Rooms)
        {
            room.Theme = theme;
        }

        var mapElements = MapBuilder.Build(testMap);
        TextureAdjuster.ApplyTextures(mapElements);
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
            new ThemeRule(new TextureQuery(new [] { themeName, "Door" }, color), Conditions: new IsDoor()),
        });

        foreach (var room in testMap.Rooms)
        {
            room.Theme = theme;
        }

        var mapElements = MapBuilder.Build(testMap);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);

        var doorLines = mapElements.LineDefs.Where(p => p.LineSpecial?.Type == Models.LineSpecials.LineSpecialType.DoorRaise).ToArray();
        Assert.That(doorLines, Is.Not.Empty);
        foreach (var doorLine in doorLines)
        {
            Assert.That(doorLine.Front.Data.texturetop, Is.EqualTo(expectedDoor.ToString()));
        }
    }

    [TestCase("Tech", "Brown", Texture.BIGDOOR4)]
    [TestCase("Tech", "Green", Texture.BIGDOOR3)]
    [TestCase("Wood", "Brown", Texture.BIGDOOR5)]
    public void CanApplyNamedThemeToSwitchControlledDoors(string themeName, string color, Texture expectedDoor)
    {
        var map = new Map();
        var room1 = map.AddRoom(new Room { UpperLeft = Point.Empty, BottomRight = new Point(256, -256) });
        var room2 = map.AddRoom(new Room(map, size: new Size(128, 128)).Place().EastOf(room1, 32));
        var switchAlcove = StructureGenerator.AddStructure(room1, new Alcove(new Room { Floor = 16, Ceiling = -16 }, Side.Left, 64, 8, 0.5));
        int doorTag = IDProvider.NextSectorIndex();
        switchAlcove.LineSpecials[Side.Left] = new DoorOpen(doorTag, Speed.StandardDoor);
        map.AddRoom(HallGenerator.GenerateHall(new Hall(128, room1, room2,
            Door: new Door(8, Texture.BIGDOOR1, Texture.DOORTRAK, 16, Tag: doorTag))));

        var theme = new Theme(new ThemeRule[]
        {
            new ThemeRule(new TextureQuery(new [] { themeName, "Door" }, color), Conditions: new IsDoor()),
        });

        foreach (var room in map.Rooms)
        {
            room.Theme = theme;
        }

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);

        var door = mapElements.Sectors.Single(p => p.Room.VerticalHeight == 0);
        var doorLines = door.Lines.Where(p => p.Back != null).ToArray();
        Assert.That(doorLines, Is.Not.Empty);
        foreach (var doorLine in doorLines)
        {
            Assert.That(doorLine.Front.Data.texturetop, Is.EqualTo(expectedDoor.ToString()));
        }
    }

    // room for improvement but ok for now
    [Test]
    [WithStaticFlags(LegacyFlags.DontClearUnusedMapElements | LegacyFlags.DisableMoveTowardRoundingFix | LegacyFlags.OverwriteExistingXOffset)]
    public void CanApplyTechTheme()
    {
        var testMap = new TestMaps().TextureTestMap();
        testMap.Theme = new TechbaseTheme();
        ThingPlacer.AddPlayerStartToFirstRoomCenter(testMap);
        var udmf = MapToUDMF(testMap);
        var expected = File.ReadAllText("Fixtures//texture_test_techbase.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    [Test]
    public void CanApplyNukageTextureToPitWalls()
    {
        var map = new TestMaps().TwoConnectedRoomsWithDifferentCeilings();
        map.Theme = new TechbaseTheme();
        var pit = StructureGenerator.AddStructure(map.Rooms[1], new HazardPit(
            Depth: 32,
            Flat: AnimatedFlat.NUKAGE1,
            Damage: DamagingSectorSpecial.Damage_10Percent,
            Padding: new Padding(16)));
        pit.Tag = 1;

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyThemes(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);

        var pitSector = mapElements.Sectors.Single(p => p.Tag == 1);
        foreach(var line in pitSector.Lines)
        {
            var lowerTexture = line.Front.Data.texturebottom;
            var textureInfo = DoomConfig.DoomTextureInfo[lowerTexture!];
            Assert.That(textureInfo!.Size!.Height, Is.GreaterThanOrEqualTo(32));
            Assert.That(line.Data.dontpegbottom, Is.False);
            Assert.That(line.Front.Data.offsety, Is.EqualTo(-32));
        }
    }


    [TestCase(KeyType.Red, Texture.DOORRED)]
    [TestCase(KeyType.Yellow, Texture.DOORYEL)]
    [TestCase(KeyType.Blue, Texture.DOORBLU)]
    [TestCase(KeyType.RedSkull, Texture.DOORRED2)]
    [TestCase(KeyType.YellowSkull, Texture.DOORYEL2)]
    [TestCase(KeyType.BlueSkull, Texture.DOORBLU2)]

    public void CanApplyThemeToDoorColorBars(KeyType color, Texture colorBarTexture)
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(512, 0),
            BottomRight = new Point(768, -256)
        });

        var hall = HallGenerator.GenerateHall(
            new Hall(128,
            map.Rooms[0],
            map.Rooms[1],
            Door: new Door(16, new TextureInfo(Texture.BIGDOOR2), new TextureInfo(Texture.BIGDOOR2), 64, KeyColor: color, 
                           ColorBar: new DoorColorBarRecessedAlcoves())));

        map.Rooms.Add(hall);

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);

        var colorLines = mapElements.SideDefs.Where(p => p.Data.texturemiddle == colorBarTexture.ToString()).ToArray();

        Assert.That(colorLines.Length, Is.EqualTo(12));
    }

    [Test]
    public void CanApplyThemeBasedOnRoomType()
    {
        var theme = new Theme(new ThemeRule[] {
            new ThemeRule(new TextureQuery(Texture.TEKWALL4),
            Conditions: new FrontRoomBuildingBlockTypeIs<Alcove>()) });

        var map = new Map();
        map.Theme = theme;  

        var room = map.AddRoom(new Room(map, size: new Size(256,256)));
        var alcove = StructureGenerator.AddStructure(room,
            new Alcove(new Room { Floor = 16, Ceiling = -16, Tag = 1 }, Side.Left, 64, 8, 0.5));

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyThemes(mapElements);

        var alcoveSector = mapElements.Sectors.Single(p => p.Tag == 1);
        var alcoveLines = alcoveSector.Lines.Where(p => p.Back == null).ToArray();

        Assert.That(alcoveLines.Length, Is.EqualTo(3));
        foreach(var line in alcoveLines)
        {
            Assert.That(line.Front.Texture, Is.EqualTo(Texture.TEKWALL4.ToString()));
        }
    }

    [Test]
    public void CanGenerateDoorColorBars()
    {
        var map = new Map();
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(256, -256)
        });
        map.Rooms.Add(new Room
        {
            UpperLeft = new Point(512, 0),
            BottomRight = new Point(768, -256)
        });

        var doorColorBar = new DoorColorBarFlat(Distance: 0, Width: 16);
        var hall = HallGenerator.GenerateHall(
            new Hall(128,
            map.Rooms[0],
            map.Rooms[1],
            Door: new Door(16, new TextureInfo(Texture.BIGDOOR2), new TextureInfo(Texture.BIGDOOR2), 64, KeyColor: KeyType.Red,
                           ColorBar: doorColorBar)));

        map.Rooms.Add(hall);

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        var colorBarLines = mapElements.LineDefs.Where(p => p.Front.Texture == Texture.DOORRED.ToString()).ToArray();

        Assert.That(colorBarLines.Length, Is.EqualTo(4));

        foreach(var line in colorBarLines)
            Assert.That(line.Length, Is.EqualTo(16));
    }

    [Test]
    public void CanAutoAlignSwitch()
    {
        var map = new Map();
        var room = map.AddRoom(size: new Size(256, 256));

        var alcove = room.AddInnerStructure(StructureGenerator.AddStructure(room, new Alcove(new Room { Floor = 16, Ceiling = -64 }, Side.Top, 32, 8, 0.5)));
        alcove.SideTextures[Side.Top] = new TextureInfo(new TextureQuery(Texture.SW1CMT),
            AutoAlign: new AutoAlignment(RegionLabel: "Switch", TexturePosition: new PointF(0.5f,0.5f), WallPosition: new PointF(0.5f, 0.5f), Part: TexturePart.Middle));

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        var switchLine = mapElements.LineDefs.Single(p => p.Front.Texture == Texture.SW1CMT.ToString());
        Assert.That(switchLine.Front.Data.offsetx, Is.EqualTo(16));
        Assert.That(switchLine.Front.Data.offsety, Is.EqualTo(64));
    }

    [Test]
    public void CanAutoAlignSwitchOnLowerTexture()
    {
        var map = new Map();
        var room = map.AddRoom(size: new Size(256, 256));

        var inner = room.AddInnerStructure(new Room(map, size: new Size(8,32)) { Floor = 32, Ceiling = 0 });
        inner.Place().ToInsideOf(room, Side.Left, -32);

        inner.SideTextures[Side.Right] = new TextureInfo(new TextureQuery(Texture.SW1CMT),
            AutoAlign: new AutoAlignment(RegionLabel: "Switch", TexturePosition: new PointF(0.5f, 0.5f), WallPosition: new PointF(0.5f, 0.5f), Part: TexturePart.Lower));

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        var switchLine = mapElements.LineDefs.Single(p => p.Front.Texture == Texture.SW1CMT.ToString());
        Assert.That(switchLine.Back.Data.offsetx, Is.EqualTo(16));
        Assert.That(switchLine.Back.Data.offsety, Is.EqualTo(72));
    }

    [Test]
    public void CanAutoAlignSwitchOnUpperTexture()
    {
        var map = new Map();
        var room = map.AddRoom(size: new Size(256, 256));

        var inner = room.AddInnerStructure(new Room(map, size: new Size(8, 32)) { Floor = 0, Ceiling = -32 });
        inner.Place().ToInsideOf(room, Side.Left, -32);

        inner.SideTextures[Side.Right] = new TextureInfo(new TextureQuery(Texture.SW1CMT),
            AutoAlign: new AutoAlignment(RegionLabel: "Switch", TexturePosition: new PointF(0.5f, 0.5f), WallPosition: new PointF(0.5f, 0.5f), Part: TexturePart.Upper));

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.AdjustOffsetsAndPegs(mapElements);
        var switchLine = mapElements.LineDefs.Single(p => p.Front.Texture == Texture.SW1CMT.ToString());
        Assert.That(switchLine.Back.Data.offsetx, Is.EqualTo(16));
        Assert.That(switchLine.Back.Data.offsety, Is.EqualTo(104));
    }

    [Test]
    public void CanApplyMultipleThemesToMap()
    {
        var map = new TestMaps().TwoConnectedRoomsWithDifferentCeilings();

        var theme1 = new Theme(new ThemeRule[]
        {
            new ThemeRule(Texture: new TextureInfo(Texture.BFALL1), Conditions: new TrueCondition()),
        });

        var theme2 = new Theme(new ThemeRule[]
        {
            new ThemeRule(Texture: new TextureInfo(Texture.BRICK10), Conditions: new TrueCondition()),
        });

        map.Rooms[0].Theme = theme1;
        map.Rooms[0].Tag = 1;

        map.Rooms[1].Theme = theme2;
        map.Rooms[1].Tag = 2;

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);

        var room1Sides = mapElements.SideDefs.Where(p => p.Sector.Tag == 1).ToArray();
        var room2Sides = mapElements.SideDefs.Where(p => p.Sector.Tag == 2).ToArray();

        Assert.That(room1Sides, Is.Not.Empty);
        Assert.That(room2Sides, Is.Not.Empty);

        foreach(var side in room1Sides)
            Assert.That(side.Texture, Is.EqualTo(Texture.BFALL1.ToString()));

        foreach (var side in room2Sides)
            Assert.That(side.Texture, Is.EqualTo(Texture.BRICK10.ToString()));
    }
}

