using System.Xml.Linq;
using WadMaker.Models.LineSpecials;

namespace WadMaker.Services;

public class MapBuilder
{
    private readonly RoomBuilder _roomBuilder;
    private readonly OverlappingLinedefResolver _overlappingLinedefResolver;
    private readonly IsPointInSector _isPointInSector;

    public MapBuilder(RoomBuilder roomBuilder, OverlappingLinedefResolver overlappingLinedefResolver, IsPointInSector isPointInSector)
    {
        _roomBuilder = roomBuilder;
        _overlappingLinedefResolver = overlappingLinedefResolver;
        _isPointInSector = isPointInSector;
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
        RemoveInvalidSidedefs(mapElements);
        EnsureSingleSidedLinedefsAreFacingInward(mapElements);
        EnsureActionLinedefsFacingCorrectDirection(mapElements);

        foreach (var sideDef in mapElements.SideDefs)
        {
            sideDef.Resolve(mapElements);
        }

        foreach (var lineDef in mapElements.LineDefs)
        {
            lineDef.Resolve(mapElements);
        }

        foreach(var sector in mapElements.Sectors)
        {
            sector.Lines = [.. mapElements.LineDefs.Where(p => p.BelongsTo(sector))];
        }

        return mapElements;
    }

    private void EnsureActionLinedefsFacingCorrectDirection(MapElements mapElements)
    {
        foreach(var specialLine in mapElements.LineDefs.Where(p=>p.LineSpecial != null && p.Back != null))
        {
            // note, not able to handle when both sides have specials
            if (specialLine.LineSpecial!.AppliesToBackSector && specialLine.Front.Sector.Room.LineSpecials.Any())
                specialLine.FlipSides();
        }
    }

    private void EnsureSingleSidedLinedefsAreFacingInward(MapElements mapElements)
    {
        foreach(var line in mapElements.LineDefs.Where(p=>p.Back == null))
        {            
            if (!_isPointInSector.Execute(line.FrontTestPoint, line.Front.Sector, mapElements))
            {
                line.FlipDirection();
            }
        }
    }

    private void RemoveInvalidSidedefs(MapElements mapElements)
    {
        foreach(var lineDef in mapElements.LineDefs.Where(p=>p.Back != null))
        {
            Sector? frontSector = mapElements.Sectors.FirstOrDefault(p => _isPointInSector.Execute(lineDef.FrontTestPoint, p, mapElements));
            Sector? backSector = mapElements.Sectors.FirstOrDefault(p => _isPointInSector.Execute(lineDef.BackTestPoint, p, mapElements));

            if (frontSector == null)
            {
                // should do something, but don't know what
            }
            else if(backSector == null)
            {
                mapElements.SideDefs.Remove(lineDef.Back!);
                lineDef.RemoveBack();
                new TextureInfo(lineDef).ApplyTo(lineDef);
            }
        }
    }
}
