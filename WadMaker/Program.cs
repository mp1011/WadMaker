var map = new Map();
map.Rooms.Add(new Room
{
    Floor = 0,
    Ceiling = 256,
    WallTexture = Texture.STONE2,
    FloorTexture = Flat.FLOOR0_1,
    CeilingTexture = Flat.FLOOR0_3,
    UpperLeft = new Point(0, 0),
    BottomRight = new Point(256, -256)
});

map.Rooms.Add(new Room
{
    Floor = 0,
    Ceiling = 256,
    WallTexture = Texture.STONE,
    FloorTexture = Flat.FLOOR0_1,
    CeilingTexture = Flat.FLOOR0_3,
    UpperLeft = new Point(0, 500),
    BottomRight = new Point(256, 400)
});

var hall = new Hall(
    Room1: map.Rooms[0],
    Room2: map.Rooms[1],
    Width: 64,
    HallTemplate: new Room
    {
        Floor = 16,
        Ceiling = 128 - 16,
        FloorTexture = Flat.FLOOR0_3,
        WallTexture = Texture.STONE2
    });


map.Rooms[0].Halls.Add(hall);
map.Rooms[1].Halls.Add(hall);
map.Rooms[0].ShapeModifiers.Add(new InvertCorners { Width = 64 });
map.Rooms.Add(new HallGenerator().GenerateHall(hall));

var serviceContainer = ServiceContainer.CreateServiceProvider(ServiceContainer.StandardDependencies);

var mapPainter = serviceContainer.GetService<MapPainter>()!;
var mapBuilder = serviceContainer.GetService<MapBuilder>()!;
var udmf = mapPainter.Paint(mapBuilder.Build(map));

Console.WriteLine("Done");