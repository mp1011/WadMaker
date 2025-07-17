using WadMaker.Models;

namespace WadMaker.Services;

public class MapBuilder
{
    private readonly RoomBuilder _roomBuilder;
    private readonly OverlappingLinedefResolver _overlappingLinedefResolver;

    public MapBuilder(RoomBuilder roomBuilder, OverlappingLinedefResolver overlappingLinedefResolver)
    {
        _roomBuilder = roomBuilder;
        _overlappingLinedefResolver = overlappingLinedefResolver;
    }

    public MapElements Build(Map map)
    {
        var mapElements = new MapElements();
        foreach (var room in map.Rooms)
        {
            var roomElements = _roomBuilder.Build(room);
            mapElements.Merge(roomElements);
        }

        _overlappingLinedefResolver.Execute(mapElements);

        foreach (var sideDef in mapElements.SideDefs)
        {
            sideDef.Resolve(mapElements);
        }

        foreach (var lineDef in mapElements.LineDefs)
        {
            lineDef.Resolve(mapElements);
        }

        // add player start (temporary)
        mapElements.Things.Add(new Thing(new thing(
           x: map.Rooms.First().Center.X,
           y: map.Rooms.First().Center.Y,
           angle: 0,
           type: 1,
           skill1: true,
           skill2: true,
           skill3: true,
           skill4: true,
           skill5: true,
           single: true,
           dm: false,
           coop: false
       )));

        return mapElements;
    }
}
