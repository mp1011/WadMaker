using WadMaker.Models.LineSpecials;

namespace WadMaker.Tests.FullMapTests;

class TechbaseSep25FullMapTest : StandardTest
{
    [TestCase]
    [WithStaticFlags(LegacyFlags.DisableMoveTowardRoundingFix)]
    public void CanCreateFullyPlayableMap()
    {
        var map = new Map();
        map.Theme = new TechbaseTheme();

        //entrance
        var entrance = map.AddRoom(new Room(parent: map, center: new Point(0, 0), size: new Size(256, 256)));
        entrance.Shape.Modifiers.Add(new AngledCorners { Width = 64 });
        entrance.AddPillar(new Cutout { UpperLeft = new Point(64, -64), BottomRight = new Point(96, -32) });
        entrance.AddPillar(new Cutout { UpperLeft = new Point(256 - 64 - 32, -64), BottomRight = new Point(256 - 64, -32) });
        entrance.AddPillar(new Cutout { UpperLeft = new Point(64, -256 + 64 + 32), BottomRight = new Point(96, -256 + 64) });
        entrance.AddPillar(new Cutout { UpperLeft = new Point(256 - 64 - 32, -256 + 64 + 32), BottomRight = new Point(256 - 64, -256 + 64) });
        ThingPlacer.AddThing(ThingType.Player1_start, entrance, 0.5, 0.5, ThingFlags.AllSkillsAndModes, Angle.North);

        //long hall
        var longHall = map.AddRoom(new Room(parent: map)
        {
            UpperLeft = new Point(entrance.Center.X - 512, entrance.Bounds.Y + 256),
            BottomRight = new Point(entrance.Center.X + 512, entrance.Bounds.Y + 128)
        });

        // entrance-hall door
        map.AddRoom(HallGenerator.GenerateHall(
            new Hall(128, entrance, longHall, Door: new Door(Thickness: 16, Texture.BIGDOOR1, Texture.DOORTRAK, PositionInHall: 16))));

        // west room
        var westRoom = map.AddRoom(new Room(parent: map, size: new Size(512, 512)).Place().WestOf(longHall, 32));
        westRoom.Shape.Modifiers.Add(new InvertCorners { Width = 128 });
        StructureGenerator.AddStructure(westRoom, new Alcove(new Room { Floor = 32, Ceiling = -32 }, Side.Bottom, 128, 16, 0.5));

        // west room to long hall door
        var doorTag = IDProvider.NextSectorIndex();
        var westDoor = map.AddRoom(HallGenerator.GenerateHall(
         new Hall(128, longHall, westRoom, Door: new Door(Thickness: 16, Texture.BIGDOOR1, Texture.DOORTRAK, PositionInHall: 8, Tag: doorTag))));
        
        // east room
        var eastRoom = map.AddRoom(new Room(parent: map, size: new Size(128, 256)).Place().EastOf(longHall, 32));
        var westDoorSwitch = StructureGenerator.AddStructure(eastRoom, new Alcove(new Room { Floor = 32, Ceiling = -32 }, Side.Right, 64, 8, 0.5));
        westDoorSwitch.LineSpecials[Side.Right] = new DoorOpen(doorTag, Speed.StandardDoor);
        westDoorSwitch.SideTextures[Side.Right] = new TextureInfo(Texture.SW1BLUE, LowerUnpegged: true);

        // east room to long hall door
        map.AddRoom(HallGenerator.GenerateHall(
         new Hall(128, longHall, eastRoom, Door: new Door(Thickness: 16, Texture.BIGDOOR1, Texture.DOORTRAK, PositionInHall: 8))));

        // stair corner
        var stairCorner = map.AddRoom(new Room(parent: map, size: new Size(128, 128)).Place().NorthOf(westRoom, 256));
        stairCorner.Floor = 128;
        stairCorner.Ceiling = 256;

        // stairs part 1 
        map.AddRoom(HallGenerator.GenerateHall(
            new Hall(128, westRoom, stairCorner, Stairs: new Stairs(
                StepTexture: new TextureInfo(Texture.STEP1),
                StartPosition: 8,
                EndPosition: 0,
                StepWidth: 16,
                FixedCeiling: 128,
                StartRoom: westRoom,
                EndRoom: stairCorner))));

        // outdoor area
        var outdoorArea = map.AddRoom(new Room(parent: map, size: new Size(longHall.Bounds.Width, (int)(longHall.Bounds.Width * .60))).Place().NorthOf(longHall, 16));
        outdoorArea.Ceiling = 512;
        var block1 = outdoorArea.AddInnerStructure(new Room { UpperLeft = Point.Empty, BottomRight = new Point(256, -256) });
        block1.Floor = 256;
        block1.Ceiling = 0;

        // stairs part 2
        map.AddRoom(HallGenerator.GenerateHall(
           new Hall(128, stairCorner, outdoorArea, Stairs: new Stairs(
               StepTexture: new TextureInfo(Texture.STEP1),
               StartPosition: 8,
               EndPosition: 0,
               StepWidth: 16,
               FixedCeiling: 128,
               StartRoom: stairCorner,
               EndRoom: block1))));


        // exit room
        var exitRoom = map.AddRoom(new Room(parent: map, size: new Size(256, 256)).Place().EastOf(outdoorArea, 64));

        // exit door
        var exitDoor = map.AddRoom(HallGenerator.GenerateHall(
         new Hall(128, outdoorArea, exitRoom, 
            HallTemplate: new Room { Floor = 0, Ceiling = 128 }, 
            Door: new Door(Thickness: 16, Texture.BIGDOOR1, Texture.DOORTRAK, PositionInHall: 16))));

        var exitDoorSwitch = StructureGenerator.AddStructure(exitRoom, new Alcove(new Room { Floor = 32, Ceiling = -32 }, Side.Right, 64, 8, 0.5));
        exitDoorSwitch.LineSpecials[Side.Right] = new ExitNormal();
        exitDoorSwitch.SideTextures[Side.Right] = new TextureInfo(Texture.SW1BLUE, LowerUnpegged: true);

        var path = new PlayerPath(new PlayerPathNode[]
        {
            new PlayerPathNode(new Room[] { entrance }),
            new PlayerPathNode(new Room[] { longHall, eastRoom }),
            new PlayerPathNode(new Room[] { westRoom }),
            new PlayerPathNode(new Room[] { outdoorArea }),
            new PlayerPathNode(new Room[] { exitRoom }),
        });

        ThingPlacer.AddThing(ThingType.Shotgun, eastRoom, 0.5, 0.5, ThingFlags.AllSkillsAndModes);

        ThingPlacer.AddMonsters(path,
            new MonsterPlacement(ThingType.Zombieman, 0.2, 1.0, EnemyDensity.Common, Angle.South, ThingPattern.Triangle),
            new MonsterPlacement(ThingType.Shotgun_guy, 0.3, 1.0, EnemyDensity.Sparse, Angle.South, ThingPattern.Triangle),
            new MonsterPlacement(ThingType.Imp, 0.5, 1.0, EnemyDensity.Sparse, Angle.South, ThingPattern.Triangle),
            new MonsterPlacement(ThingType.Hell_knight, 1.0, 1.0, EnemyDensity.Single, Angle.South, ThingPattern.Triangle)
            );

        ThingPlacer.AddAmmo(path, ResourceBalance.Adequate);

        var udmf = MapToUDMF(map);
        var expected = File.ReadAllText("Fixtures//sep25fullmap.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }
}
