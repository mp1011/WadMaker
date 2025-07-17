var map = new Map();
map.Rooms.Add(new Room
{
    UpperLeft = new Point(0, 0),
    BottomRight = new Point(400, -400)
});

map.Rooms[0].InnerStructures.Add(new Room
{
    UpperLeft = new Point(100, -100),
    BottomRight = new Point(300, -300),
    Floor = 16,
    Ceiling = 0,
});

map.Rooms[0].InnerStructures[0].InnerStructures.Add(new Room
{
    UpperLeft = new Point(50, -50),
    BottomRight = new Point(100, -100),
    Floor = 16,
    Ceiling = 0,
});



var services = ServiceContainer.CreateServiceProvider(ServiceContainer.StandardDependencies);
var mapElements = services.GetRequiredService<MapBuilder>().Build(map);
var mapPainter = services.GetService<MapPainter>()!;
var udmf = mapPainter.Paint(mapElements);

Console.WriteLine("Done");