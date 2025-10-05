using WadMaker.Models.LineSpecials;

namespace WadMaker.Tests.FullMapTests;

class TechbaseOct25FullMapTest : StandardTest
{
    [TestCase]
    public void CanCreateFullyPlayableMap()
    {
        var map = new Map();
        map.Theme = new TechbaseTheme(Version: 1);

        var entrance = map.AddRoom(new Room(map, size: new Size(256,256)));
        entrance.ShapeModifiers.Add(new AngledCorners(64));

        var roomInner = entrance.AddInnerStructure(new Room(map) {  Floor = -8, Ceiling = 8 });
        roomInner.UpperLeft = Point.Empty;
        roomInner.BottomRight = new Point(entrance.Size.Width, -entrance.Size.Height);
        roomInner.ShapeModifiers.Add(new CopyParentShape(new Padding(8), entrance));
        var skylight = roomInner.AddInnerStructure(new Room(map, size: new Size(128, 128)));
        skylight.Center = roomInner.Bounds().RelativePoint(0.5, 0.5);
        skylight.CeilingTexture = Flat.F_SKY1;
        skylight.Ceiling = 16;

        StructureGenerator.AddStructure(entrance, new Alcove(
            Template: new Room {  Floor = 32, Ceiling = -32 },
            Side: Side.Left,
            Width: 128,
            Depth: 16,
            CenterPercent: 0.5));

        StructureGenerator.AddStructure(entrance, new Alcove(
           Template: new Room { Floor = 32, Ceiling = -32 },
           Side: Side.Right,
           Width: 128,
           Depth: 16,
           CenterPercent: 0.5));


        var northHall = entrance.CreateNeighbor(Side.Top, Anchor.MidPoint, Anchor.MidPoint, 256)
            .AddTo(map);
        northHall.Floor = 128;
        northHall.Ceiling = 256;
        northHall.Size = new Size(512 + 64, 128);
        RoomPositionResolver.Execute(map);

        HallGenerator.GenerateHall(new Hall(
            Width: 128,
            Room1: entrance,
            Room2: northHall,
            Stairs: new Stairs(TextureInfo.Default, 0, 0, 16, entrance, northHall))).AddTo(map);

        var northWestRoom = northHall.CreateNeighbor(Side.Left, Anchor.MidPoint, Anchor.Absolute(64), spacing: 0).AddTo(map);
        northWestRoom.MatchFloorAndCeilingTo(northHall);
        northWestRoom.Size = new Size(256, 256);       
        var pillar1 = new Cutout(size: new Size(128, 128), upperLeft: new Point(64,-64));
        northWestRoom.Pillars.Add(pillar1);

        var northEastRoom = northHall.CreateNeighbor(Side.Right, Anchor.MidPoint, Anchor.Absolute(64), spacing: 0).AddTo(map);
        northEastRoom.MatchFloorAndCeilingTo(northHall);
        northEastRoom.Size = new Size(256, 256);
        var pillar2 = new Cutout(size: new Size(128, 128), upperLeft: new Point(64, -64));
        northEastRoom.Pillars.Add(pillar2);
        RoomPositionResolver.Execute(map);

        var bigRoom = northEastRoom.CreateNeighbor(Side.Bottom, Anchor.MidPoint, Anchor.MidPoint, 64).AddTo(map);
        bigRoom.Size = new Size(512, 512);
        bigRoom.ShapeModifiers.Add(new InvertCorners { Width = 128 });
        bigRoom.MatchFloorAndCeilingTo(northEastRoom, ceilingAdjust: 128);
        RoomPositionResolver.Execute(map);

        HallGenerator.GenerateHall(new Hall(
            Width: 128,
            HallTemplate: new Room(map) { Floor = 128, Ceiling = 256 },
            Room1: northEastRoom,
            Room2: bigRoom,
            Door: new Door(16, TextureInfo.Default, TextureInfo.Default, 16))).AddTo(map);

        var skylight2 = bigRoom.AddInnerStructure(new Room { Ceiling = 32, CeilingTexture = Flat.F_SKY1 });
        skylight2.ShapeModifiers.Add(new NGon { Sides = 6 });
        skylight2.Center = bigRoom.Bounds().RelativePoint(0.5, 0.5);

        var leftLedge = bigRoom.AddInnerStructure(new Room { Floor = 128, Ceiling = 0 });
        leftLedge.UpperLeft = new Point(0, -128);
        leftLedge.BottomRight = new Point(64, -128 - 256);

        var rightLedge = bigRoom.AddInnerStructure(new Room { Floor = 128, Ceiling = 0 });
        rightLedge.UpperLeft = new Point(512 - 64, -128);
        rightLedge.BottomRight = new Point(512, -128 - 256);

        ThingPlacer.AddFormation(ThingType.Imp, leftLedge, 4, Angle.East, ThingFlags.AllSkillsAndModes | ThingFlags.Ambush, ThingPlacement.Center, ThingPattern.Row, 32);
        ThingPlacer.AddFormation(ThingType.Imp, rightLedge, 4, Angle.West, ThingFlags.AllSkillsAndModes | ThingFlags.Ambush, ThingPlacement.Center, ThingPattern.Row, 32);

        var southRoom = bigRoom.CreateNeighbor(Side.Bottom, Anchor.MidPoint, Anchor.Percent(0.75), 128).AddTo(map);
        southRoom.Size = new Size(256 + 128, 128 + 64);
        southRoom.Floor = 0;
        southRoom.Ceiling = 128;
        RoomPositionResolver.Execute(map);

        HallGenerator.GenerateHall(new Hall(
            Width: 128,
            Room1: bigRoom,
            Room2: southRoom,
            HallTemplate: new Room { Floor = 128, Ceiling = 128 + 112 },
            Lift: new Lift(TextureInfo.Default, 64, 64, bigRoom, southRoom, AddWalkTrigger: true))).AddTo(map);

        var keyRoom = southRoom.CreateNeighbor(Side.Left, Anchor.MidPoint, Anchor.MidPoint, 16).AddTo(map);
        keyRoom.Size = new Size(128, 128);
        RoomPositionResolver.Execute(map);

        StructureGenerator.AddStructure(southRoom, new Window(
            Template: new Room { Floor = 32, Ceiling = -32 },
            AdjacentRoom: keyRoom,
            Width: 64,
            CenterPercent: 0.5));

        var southPassage1 = map.AddRoom(new Room(map) { Ceiling = 64 });
        southPassage1.UpperLeft = new Point(keyRoom.UpperLeft.X + 32, keyRoom.UpperLeft.Y + 128 + 32);
        southPassage1.BottomRight = new Point(southRoom.UpperLeft.X + 128, southRoom.UpperLeft.Y + 64);

        var southPassage2 = map.AddRoom(new Room(map) { Ceiling = 64 });
        southPassage2.UpperLeft = new Point(southPassage1.BottomRight.X - 64, southPassage1.BottomRight.Y);
        southPassage2.BottomRight = new Point(southPassage1.BottomRight.X, southRoom.UpperLeft.Y);

        var southPassage3 = map.AddRoom(new Room(map) { Ceiling = 64 });
        southPassage3.UpperLeft = new Point(keyRoom.UpperLeft.X + 32, southPassage1.BottomRight.Y);
        southPassage3.BottomRight = new Point(keyRoom.UpperLeft.X + 32 + 64, keyRoom.UpperLeft.Y);

        var keyPedestal = keyRoom.AddInnerStructure(new Room(map, size: new Size(32, 32)) { Floor = 32 });
        keyPedestal.ShapeModifiers.Add(new NGon { Sides = 8 });
        keyPedestal.Center = keyRoom.Bounds().RelativePoint(0.5, 0.5);
        ThingPlacer.AddThing(ThingType.Red_keycard, keyPedestal, 0.5, 0.5);

        var acidRoom = northWestRoom.CreateNeighbor(Side.Bottom, Anchor.MidPoint, Anchor.MidPoint, 96).AddTo(map);
        acidRoom.MatchFloorAndCeilingTo(northWestRoom);
        acidRoom.Size = new Size(256, 512);
        RoomPositionResolver.Execute(map);

        var acidPit = StructureGenerator.AddStructure(acidRoom,
            new HazardPit(Depth: 24, AnimatedFlat.NUKAGE1, DamagingSectorSpecial.Damage_8Lava, new Padding(32, 64)));

        HallGenerator.GenerateHall(new Hall(
            Width: 128,
            HallTemplate: new Room(map) {  Floor = 128, Ceiling = 256 },
            Room1: northWestRoom,
            Room2: acidRoom,
            Door: new Door(
                Thickness: 16,
                Texture: TextureInfo.Default,
                TrackTexture: TextureInfo.Default,
                PositionInHall: 32,
                KeyColor: KeyType.Red,
                ColorBar: new DoorColorBarRecessedAlcoves()))).AddTo(map);

        var exitAlcove = StructureGenerator.AddStructure(acidRoom,
            new Alcove(new Room { Floor = 16, Ceiling = -16 }, Side.Bottom, 64, 8, 0.5));

        exitAlcove.SideTextures[Side.Bottom] = new TextureInfo(Texture.SW1BRN1);
        exitAlcove.LineSpecials[Side.Bottom] = new ExitNormal();

        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);


        var playerPath = new PlayerPath(new PlayerPathNode[]
        {
            new PlayerPathNode(entrance),
            new PlayerPathNode(northHall, northWestRoom, northEastRoom),
            new PlayerPathNode(bigRoom),
            new PlayerPathNode(southRoom),
            new PlayerPathNode(acidRoom)
        });

        ThingPlacer.AddThing(ThingType.Shotgun, entrance, 0.5, 0);

        ThingPlacer.AddMonsters(playerPath,
            new MonsterPlacement(ThingType.Zombieman, BeginAt: 0.2, EndAt: 1.0, EnemyDensity.Common, Angle.South, Flags: ThingFlags.AllSkillsAndModes | ThingFlags.Ambush),
            new MonsterPlacement(ThingType.Imp, BeginAt: 0.4, EndAt: 1.0, EnemyDensity.Rare, Angle.East, Flags: ThingFlags.AllSkillsAndModes | ThingFlags.Ambush),
            new MonsterPlacement(ThingType.Shotgun_guy, BeginAt: 0.6, EndAt: 1.0, EnemyDensity.Rare, Angle.East, Flags: ThingFlags.AllSkillsAndModes | ThingFlags.Ambush));

        ThingPlacer.AddFormation(ThingType.Cacodemon, acidRoom, 3, Angle.North, ThingFlags.AllSkillsAndModes | ThingFlags.Ambush, ThingPlacement.Center, ThingPattern.Circle,24);
        ThingPlacer.AddAmmo(playerPath, ResourceBalance.Adequate);

        var udmf = MapToUDMF(map, ensureThingsInBounds: true);
        var expected = File.ReadAllText("Fixtures//oct25fullmap.udmf");
        Assert.That(udmf, Is.EqualTo(expected));
    }
} 
