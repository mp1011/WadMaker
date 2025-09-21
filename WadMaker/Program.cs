
using WadMaker.Services.Extractors;

Map FourRoom()
{
    var map = new Map();

    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, 0),
        BottomRight = new Point(256, -256)
    });

    // right room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(512, 0),
        BottomRight = new Point(768, -256)
    });

    // left room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(-512, 0),
        BottomRight = new Point(-412, -256)
    });

    // up room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, 400),
        BottomRight = new Point(256, 300)
    });

    // down room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, -500),
        BottomRight = new Point(256, -700)
    });


    var hallGenerator = new HallGenerator();
    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[1],
        Door: new Door(16, Texture.BIGDOOR2, Texture.DOORTRAK, 64))));


    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[2],
        Door: new Door(16, Texture.BIGDOOR2, Texture.DOORTRAK, 64))));

    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[3],
        Door: new Door(16, Texture.BIGDOOR2, Texture.DOORTRAK, 64))));

    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[4],
        Door: new Door(16, Texture.BIGDOOR2, Texture.DOORTRAK, 64))));

    return map;
}

Map AdjacentOverlappingInner()
{

    var map = new Map();
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, 0),
        BottomRight = new Point(300, -128)
    });

    map.Rooms[0].InnerStructures.Add(
        new Room
        {
            UpperLeft = new Point(50, 0),
            BottomRight = new Point(100, -128),
        });

    map.Rooms[0].InnerStructures.Add(
       new Room
       {
           UpperLeft = new Point(100, 0),
           BottomRight = new Point(150, -128),
       });

    return map;
}

Map RoomSplitInTwo()
{
    var map = new Map();
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, 0),
        BottomRight = new Point(200, -128)
    });

    map.Rooms[0].InnerStructures.Add(
        new Room
        {
            UpperLeft = new Point(100, 0),
            BottomRight = new Point(200, -128),
        });


    return map;
}

Map FourStairs()
{
    var map = new Map();

    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, 0),
        BottomRight = new Point(256, -256),
        Ceiling = 300,
        Floor = 100,
    });

    // right room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(512, 0),
        BottomRight = new Point(768, -256),
        Ceiling = 300,
    });

    // left room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(-512, 0),
        BottomRight = new Point(-412, -256),
        Ceiling = 300,
    });

    // up room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, 400),
        BottomRight = new Point(256, 300),
        Ceiling = 300,
    });

    // down room 
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, -500),
        BottomRight = new Point(256, -700),
        Ceiling = 300,
    });


    var hallGenerator = new HallGenerator();
    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[1],
        Stairs: new Stairs(StepTexture: new TextureInfo(Lower: Texture.STEP1), 16, 16, 32, map.Rooms[0], map.Rooms[1]))));

    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[2],
        Stairs: new Stairs(StepTexture: new TextureInfo(Lower: Texture.STEP1), 16, 16, 32, map.Rooms[0], map.Rooms[2]))));

    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[3],
        Stairs: new Stairs(StepTexture: new TextureInfo(Lower: Texture.STEP1), 16, 16, 32, map.Rooms[0], map.Rooms[3]))));

    map.Rooms.Add(hallGenerator.GenerateHall(
        new Hall(128,
        map.Rooms[0],
        map.Rooms[4],
        Stairs: new Stairs(StepTexture: new TextureInfo(Lower: Texture.STEP1), 16, 16, 32, map.Rooms[0], map.Rooms[4]))));

    return map;
}

Map RoomWithAlcove(RoomGenerator roomGenerator)
{
    var room = new Room
    {
        UpperLeft = new Point(0, 0),
        BottomRight = new Point(400, -400),
    };

    var alcove = roomGenerator.AddStructure(room,
        new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
        Side: Side.Left,
        Width: 100,
        Depth: 32,
        CenterPercent: 0.50));

    var map = new Map();
    map.Rooms.Add(room);
    return map;
}

Map NotchedRoomHall()
{
    var map = new Map();
    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(0, 0),
        BottomRight = new Point(200, -200)
    });

    map.Rooms.Add(new Room
    {
        UpperLeft = new Point(1000, 0),
        BottomRight = new Point(1200, -200)
    });

    map.Rooms[0].ShapeModifiers.Add(new NotchedSides { Width = 140, Depth = 20 });
    map.Rooms[1].ShapeModifiers.Add(new NotchedSides { Width = 140, Depth = 20 });

    var hall = new Hall(100, map.Rooms[0], map.Rooms[1]);

    map.Rooms.Add(new HallGenerator().GenerateHall(hall));
    return map;
}

var things = DoomConfig.DoomThingInfo;


var services = ServiceContainer.CreateServiceProvider(ServiceContainer.StandardDependencies);

var map = new Map();
map.Rooms.Add(new Room
{
    UpperLeft = Point.Empty,
    BottomRight = new Point(400, -300),
    WallTexture = new TextureInfo(Texture.BRICK7),
});
 services.GetRequiredService<RoomGenerator>().AddStructure(map.Rooms[0],
   new Alcove(Template: new Room { Floor = 32, Ceiling = -32 },
   Side: Side.Top,
   Width: 100,
   Depth: 32,
   CenterPercent: 0.50));

var mapElements = services.GetRequiredService<MapBuilder>().Build(map);

services.GetRequiredService<TextureAdjuster>().AdjustOffsetsAndPegs(mapElements);

var mapPainter = services.GetService<MapPainter>()!;
var udmf = mapPainter.Paint(mapElements);

Console.WriteLine("Done");


