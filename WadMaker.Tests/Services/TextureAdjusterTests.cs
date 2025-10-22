using WadMaker.Config;
using WadMaker.Models.Theming;

namespace WadMaker.Tests.Services;

public class TextureAdjusterTests : StandardTest
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

    [Test]
    public void CanApplyThemeRuleToFlats()
    {
        var map = new TestMaps().TextureTestMap();
        var theme = new Theme(new ThemeRule[]
        {
            new ThemeRule(FloorQuery: new FlatsQuery( new [] { "Step" }), Conditions: new FrontRoomBuildingBlockTypeIs<Stairs>()),
        });
        map.Theme = theme;

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyThemes(mapElements);
        var steps = mapElements.Sectors.Where(p => p.Room.BuildingBlock is Stairs).ToArray();

        Assert.That(steps, Is.Not.Empty);
        foreach(var step in steps)
        {
            Assert.That(step.Floor, Is.EqualTo(Flat.STEP1));
        }
    }


    [Test]
    public void CanApplyThemeRuleToDoorSectors()
    {
        var map = new TestMaps().TextureTestMap();
        var theme = new Theme(new ThemeRule[]
        {
            new ThemeRule(Ceiling: Flat.TLITE6_1, Conditions: new IsDoor()),
        });
        map.Theme = theme;

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyThemes(mapElements);
        var doors = mapElements.Sectors.Where(p => p.Room.BuildingBlock is Door).ToArray();

        Assert.That(doors, Is.Not.Empty);
        foreach (var door in doors)
        {
            Assert.That(door.Ceiling, Is.EqualTo(Flat.TLITE6_1));
        }
    }

    // still room for improvement
    [Test]
    [WithStaticFlags(LegacyFlags.IgnoreColumnStops)]
    public void CanApplyTechTheme()
    {
        var testMap = new TestMaps().TextureTestMap();
        testMap.Theme = new TechbaseTheme(Version: 2);
        ThingPlacer.AddPlayerStartToFirstRoomCenter(testMap);
        var udmf = MapToUDMF(testMap);
        var expected = File.ReadAllText("Fixtures//texture_test_techbase.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    [Test]
    [WithStaticFlags(LegacyFlags.IgnoreColumnStops)]
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

        var alcove = StructureGenerator.AddStructure(room, new Alcove(new Room { Floor = 16, Ceiling = -64 }, Side.Top, 32, 8, 0.5));
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

    [Test]
    public void OffsetsObeyColumnStops()
    {
        var map = new Map();
        var room = map.AddRoom();
        room.BottomRight = room.UpperLeft.Add(128, -112);
      
        room.WallTexture = new TextureInfo(new TextureQuery(Texture.STARBR2), Alternate: new TextureInfo(Texture.BROWN1));

        var elements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(elements);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        var fitLines = elements.LineDefs.Where(p => p.Length == 128).ToArray();
        var nofitLines = elements.LineDefs.Where(p => p.Length < 128).ToArray();

        Assert.That(fitLines, Is.Not.Empty);
        Assert.That(nofitLines, Is.Not.Empty);

        foreach (var line in fitLines)
            Assert.That(line.Front.Data.texturemiddle, Is.EqualTo(Texture.STARBR2.ToString()));

        foreach (var line in nofitLines)
            Assert.That(line.Front.Data.texturemiddle, Is.EqualTo(Texture.BROWN1.ToString()));
    }

    [Test]
    public void OffsetsObeyColumnStops2()
    {
        var map = new Map();
        var room = map.AddRoom(size: new Size(246,246));
        room.Shape.Modifiers.Add(new InvertCorners { Width = 64 });
        
        room.WallTexture = new TextureInfo(new TextureQuery(Texture.STARBR2), Alternate: new TextureInfo(Texture.BROWN1));

        var elements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(elements);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        var fitLines = elements.LineDefs.Where(p => p.Length == 64).ToArray();
        var nofitLines = elements.LineDefs.Where(p => p.Length != 64).ToArray();

        Assert.That(fitLines, Is.Not.Empty);
        Assert.That(nofitLines, Is.Not.Empty);

        foreach (var line in fitLines)
            Assert.That(line.Front.Data.texturemiddle, Is.EqualTo(Texture.STARBR2.ToString()));

        foreach (var line in nofitLines)
            Assert.That(line.Front.Data.texturemiddle, Is.EqualTo(Texture.BROWN1.ToString()));
    }

    [Test]
    public void OffsetsObeyColumnStopsWithTwoSidedLines()
    {
        var map = new Map();
        var room = map.AddRoom(size: new Size(256, 256));

        StructureGenerator.AddStructure(room, new Alcove(64, 0, Side.Top, 64, 16, 0.4));      
        room.WallTexture = new TextureInfo(new TextureQuery(Texture.STARBR2), Alternate: new TextureInfo(Texture.BROWN1));

        var elements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(elements);
        TextureAdjuster.AdjustOffsetsAndPegs(elements);

        var fitLines = elements.Sectors[0].Lines.Where(p => p.Length == 256).ToArray();
        var nofitLines = elements.Sectors[0].Lines.Where(p => p.SingleSided && p.Length != 256).ToArray();
        var twoSidedLine = elements.Sectors[0].Lines.Single(p => p.Back != null);


        Assert.That(fitLines, Is.Not.Empty);
        Assert.That(nofitLines, Is.Not.Empty);

        foreach (var line in fitLines)
            Assert.That(line.Front.Data.texturemiddle, Is.EqualTo(Texture.STARBR2.ToString()));

        foreach (var line in nofitLines)
            Assert.That(line.Front.Data.texturemiddle, Is.EqualTo(Texture.BROWN1.ToString()));

        Assert.That(twoSidedLine.Front!.Data.texturebottom, Is.EqualTo(Texture.STARBR2.ToString()));
    }


    [Test]
    public void CanDistributeTexturesThatMatchQuery()
    {
        var map = new Map();
        map.Theme = new Theme(new ThemeRule[] {
            new ThemeRule(new TextureQuery(ThemeNames: ["Marble"], Distribution: TextureDistribution.Random)) });

        var room = map.AddRoom(size: new Size(256, 256));

        var mapElements = MapBuilder.Build(map);
        TextureAdjuster.ApplyTextures(mapElements);
        TextureAdjuster.ApplyThemes(mapElements);

        var uniqueTextures = mapElements.SideDefs.Select(p => p.Data.texturemiddle).Distinct().ToArray();
        Assert.That(uniqueTextures.Length, Is.GreaterThan(1));
    }

    /// <summary>
    /// map to visualize texture info
    /// </summary>
  //  [TestCase("Vines")]
  //  [TestCase("Marble")]
    [TestCase(null)]
    public void TextureInfoTestMap(string? theme)
    {
        Legacy.Flags = LegacyFlags.DontResolveCrossingLines;
        var map = new Map();
        
        var themes = DoomConfig.DoomTextureInfo.Values.SelectMany(p => p.Themes).Distinct().ToArray();

        var themeRooms = themes.Where(p=> p == theme || theme == null).Select(t=> CreateThemeRoom(map,t)).ToArray();
        int index = 0;
        foreach (var room in themeRooms.WithNeighbors().Take(themeRooms.Length - 1))
        {
            if(++index < 25)
                room.Item3.Place().EastOf(room.Item2, gap: 64);
            else
                room.Item3.Place().SouthOf(room.Item2, gap: 64);

            var hall = StructureGenerator.AddStructure(room.Item2,
                    new Hall(Width: 128, room.Item2, room.Item3));

            map.Rooms.Add(hall);
        }

        var start = map.AddRoom(size: new Size(64, 64));
        start.Place().WestOf(themeRooms[0]);
        ThingPlacer.AddThing(ThingType.Player1_start, start, 0.5, 0.5, angle: Angle.East);
        
        var udmf = MapToUDMF(map);
        var expected = File.ReadAllText("Fixtures//all_textures_map.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }

    /// <summary>
    /// Room showcasing all textures in this theme
    /// </summary>
    /// <param name="theme"></param>
    /// <returns></returns>
    private Room CreateThemeRoom(Map map, string theme)
    {
        var room = map.AddRoom(size: new Size(512, 512));
        room.Comment = theme;
        room.Ceiling = 256;
        room.LightLevel = 255;

        var textures = DoomConfig.DoomTextureInfo.Values.Where(p => p.Themes.Contains(theme)).ToArray();

        var texturePillars = textures.Select(texture =>
        {
            var pillar = room.AddInnerStructure(size: new Size(texture.Size!.Width, texture.Size!.Width));
            pillar.Ceiling = 0;
            pillar.LightLevel = 255;
            pillar.Floor = texture.Size!.Height;
            pillar.WallTexture = new TextureInfo(new TextureQuery(TextureName: texture.Name), DrawLowerFromBottom: true);

            pillar.LineSpecials[Side.Top] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);
            pillar.LineSpecials[Side.Bottom] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);
            pillar.LineSpecials[Side.Right] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);
            pillar.LineSpecials[Side.Left] = new Plat_DownWaitUpStay(Tag: 0, Speed: Speed.StandardLift);

            return pillar;
        }).ToArray();

        if(texturePillars.Length > 1 && texturePillars.Length < 4)
            room.Size = new Size(320, 1024);

        PlaceTexturePillars(room, texturePillars);

        while (texturePillars.Any(p => texturePillars.Any(q => p != q && p.Bounds.IntersectsWith(q.Bounds))))
        {
            room.Size = new Size(room.Size.Width + 128, room.Size.Height + 128);
            PlaceTexturePillars(room, texturePillars);
        }

        return room;
    }

    private void PlaceTexturePillars(Room room, Room[] pillars)
    {
        if (pillars.Length > 12)
            pillars.Place().InGrid(room, 6, new Padding(64));
        else if (pillars.Length > 6)
            pillars.Place().InGrid(room, 4, new Padding(64));
        else if (pillars.Length >= 4)
            pillars.Place().InGrid(room, 2, new Padding(64));
        else if (pillars.Length == 1)
            pillars[0].Place().InCenterOf(room);
        else
            pillars.Place().InLine(room, Side.Bottom, 0.5, new Padding(64));
    }
}

