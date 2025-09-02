using WadMaker.Models;

namespace WadMaker.Tests.Fixtures;

class TestMaps : StandardTest
{
    public Map TextureTestMap()
    {
        var map = new Map();

        var hallTemplates = new Room(map) { Ceiling = 112 };

        var centerRoom = new Room(map)
        {
            UpperLeft = Point.Empty,
            BottomRight = new Point(200, -200)
        };

        centerRoom.ShapeModifiers.Add(new InvertCorners { Width = 16 });
        map.Rooms.Add(centerRoom);
        centerRoom.InnerStructures.Add(new Room(centerRoom)
        {
            Floor = -16,
            Ceiling = 0,
            UpperLeft = new Point(32, -32),
            BottomRight = new Point(128, -128),
        });
        centerRoom.InnerStructures[0].ShapeModifiers.Add(new NGon { Sides = 16 });

        var leftRoom = new Room(map)
        {
            UpperLeft = new Point(-600, 0),
            BottomRight = new Point(-400, -200),
        };
        map.Rooms.Add(leftRoom);
        leftRoom.ShapeModifiers.Add(new AngledCorners { Width = 16 });
        leftRoom.InnerStructures.Add(new Room(leftRoom)
        {
            UpperLeft = new Point(50, -50),
            BottomRight = new Point(50 + 64, -50 - 64),
            Floor = 64,
            Ceiling =0,
        });

        var rightRoom = new Room(map)
        {
            UpperLeft = new Point(600, 0),
            BottomRight = new Point(800, -200),
            Ceiling = 0,
            Floor = -128
        };
        map.Rooms.Add(rightRoom);
        RoomGenerator.AddStructure(rightRoom, new Alcove(new Room(rightRoom) { Floor = 32, Ceiling = -32 }, Side.Right, 128, 16, 0.5));

        var topRoom = new Room(map)
        {
            UpperLeft = new Point(-100, 600),
            BottomRight = new Point(300, 400),
            Floor = 128,
            Ceiling = 256
        };

        topRoom.Pillars.Add(new Cutout {  UpperLeft = new Point(64,-64), BottomRight = new Point(96,-96) });
        map.Rooms.Add(topRoom);


        map.Rooms.Add(HallGenerator.GenerateHall(new Hall(128, centerRoom, leftRoom, hallTemplates,
            Door: new Door(Thickness: 16, Texture.BIGDOOR1, Texture.DOORSTOP, 64))));

        map.Rooms.Add(HallGenerator.GenerateHall(new Hall(128, centerRoom, rightRoom, hallTemplates,
            Lift: new Lift(new TextureInfo(Mid: Texture.STONE, Lower: Texture.PLAT1), 64, 128, centerRoom, rightRoom))));

        map.Rooms.Add(HallGenerator.GenerateHall(new Hall(128, centerRoom, topRoom, hallTemplates,
            Stairs: new Stairs(new TextureInfo(Mid: Texture.STONE, Lower: Texture.STEP1), 32, 32, 32, centerRoom, topRoom))));

        return map;
    }

    public Map TwoConnectedRoomsWithDifferentCeilings()
    {
        var map = new Map();
        var leftRoom = new Room
        {
            UpperLeft = Point.Empty,
            BottomRight = new Point(256, -256),
            Ceiling = 128
        };
        var rightRoom = new Room
        {
            UpperLeft = new Point(256, 0),
            BottomRight = new Point(512, -256),
            Ceiling = 112
        };
        map.Rooms.Add(leftRoom);
        map.Rooms.Add(rightRoom);
        return map;
    }

    public Map LinearMap()
    {
        var map = new Map();
        Room? lastRoom = null;

        for(int i = 0; i < 5; i++)
        {
            var room = new Room
            {
                UpperLeft = new Point(500*i, 0),
                BottomRight = new Point((500*i)+256, -256),
                Ceiling = 128
            };
            map.Rooms.Add(room);

            if (lastRoom != null)
            {
                map.Rooms.Add(HallGenerator.GenerateHall(new Hall(128, lastRoom, room, 
                    Door: new Door(16, Texture.BIGDOOR2, Texture.DOORTRAK, 32))));
            }

            lastRoom = room;
        }
              
        ThingPlacer.AddPlayerStartToFirstRoomCenter(map);
        return map;
    }


    public Map RoomWithPillar()
    {
        var map = new Map();
        var room = new Room()
        {
            UpperLeft = new Point(0, 0),
            BottomRight = new Point(400, -400)
        };

        room.Pillars.Add(new Cutout
        {
            UpperLeft = new Point(100, -100),
            BottomRight = new Point(300, -300)
        });

        map.Rooms.Add(room);
        return map;
    }
}
