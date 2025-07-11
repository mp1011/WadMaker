﻿var map = new Map();
map.Rooms.Add(new Room
{
    Floor = 0,
    Height = 256,
    WallTexture = Texture.STONE,
    FloorTexture = Flat.FLOOR0_1,
    CeilingTexture = Flat.FLOOR0_3,
    UpperLeft = new Point(0, 0),
    BottomRight = new Point(256, -256)
});

map.Rooms.Add(new Room
{
    Floor = 0,
    Height = 256,
    WallTexture = Texture.STONE,
    FloorTexture = Flat.FLOOR0_1,
    CeilingTexture = Flat.FLOOR0_3,
    UpperLeft = new Point(0, 500),
    BottomRight = new Point(256, 400)
});

var hall = new Hall(Room1: map.Rooms[0], Room2: map.Rooms[1], Width: 64);
map.Rooms[0].Halls.Add(hall);
map.Rooms[1].Halls.Add(hall);

map.Rooms.Add(new HallGenerator().GenerateHall(hall));

var mapPainter = new MapPainter(new RoomBuilder(new IDProvider()), new OverlappingLinedefResolver());
var udmf = mapPainter.Paint(map);

Console.WriteLine("Done");