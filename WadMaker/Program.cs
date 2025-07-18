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
var hallGenerator = new HallGenerator();
map.Rooms.Add(hallGenerator.GenerateHall(
    new Hall(128,
    map.Rooms[0],
    map.Rooms[1],
    Door: new Door(16, Texture.BIGDOOR2, 64))));

var services = ServiceContainer.CreateServiceProvider(ServiceContainer.StandardDependencies);
var mapElements = services.GetRequiredService<MapBuilder>().Build(map);
var mapPainter = services.GetService<MapPainter>()!;
var udmf = mapPainter.Paint(mapElements);

Console.WriteLine("Done");