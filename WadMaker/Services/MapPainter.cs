using WadMaker.Models;

namespace WadMaker.Services;
public class MapPainter
{
    private readonly RoomBuilder _roomBuilder;
    private readonly OverlappingLinedefResolver _overlappingLinedefResolver;
    private readonly IAnnotator _annotator;

    public MapPainter(RoomBuilder roomPainter, OverlappingLinedefResolver overlappingLinedefResolver, IAnnotator annotator)
    {
        _roomBuilder = roomPainter;
        _overlappingLinedefResolver = overlappingLinedefResolver;
        _annotator = annotator;
    }

    public string Paint(Map map)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace = \"ZDoom\";");

        var mapElements = new MapElements();
        foreach (var room in map.Rooms)
        {
            var roomElements = _roomBuilder.Build(room);
            mapElements.Merge(roomElements);
            
            foreach(var pillar in room.Pillars)
            {
                mapElements.Merge(_roomBuilder.BuildPillar(room, roomElements, pillar));
            }
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

        mapElements.Vertices.Paint(sb);
        mapElements.Sectors.ToDataArray().Paint(sb);
        mapElements.SideDefs.ToDataArray().Paint(sb);
        mapElements.LineDefs.ToDataArray().AnnotateAll(_annotator).Paint(sb);

        var thing = new thing(
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
       );
        thing.Paint(sb);

        return sb.ToString();
    }
}

