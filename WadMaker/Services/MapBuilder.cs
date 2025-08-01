﻿namespace WadMaker.Services;

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

    private void EnsureActionLinedefsFacingCorrectDirection(MapElements mapElements)
    {
        foreach(var specialLine in mapElements.LineDefs.Where(p=>p.LineSpecial != null && p.Back != null))
        {
            // need logic for this
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
