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
    BottomRight = new Point(256,-700)
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


var services = ServiceContainer.CreateServiceProvider(ServiceContainer.StandardDependencies);
var mapElements = services.GetRequiredService<MapBuilder>().Build(map);
var mapPainter = services.GetService<MapPainter>()!;
var udmf = mapPainter.Paint(mapElements);

Console.WriteLine("Done");

